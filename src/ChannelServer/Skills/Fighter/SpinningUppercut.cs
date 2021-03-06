﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Combat;
using Aura.Channel.Skills.Magic;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Fighter
{
	/// <summary>
	/// Spinning Uppercut skill handler
	/// </summary>
	/// <remarks>
	/// STAGE 2 CHAIN
	/// Var1: Damage
	/// Var2: Cooldown Decreased
	/// Var3: Defense Reduction
	/// Var4: Protection Reduction
	/// Var6: Reduction Chance
	/// </remarks>
	[Skill(SkillId.SpinningUppercut)]
	public class SpinningUppercut : ISkillHandler, IPreparable, ICompletable, ICancelable, IInitiableSkillHandler
	{
		/// <summary>
		/// Attacker's stun after skill use
		/// </summary>
		private const int AttackerStun = 1900;

		/// <summary>
		/// Target's stun after being hit
		/// </summary>
		private const int TargetStun = 4000;

		/// <summary>
		/// Target's stability reduction on hit
		/// </summary>
		private const int StabilityReduction = 10;

		/// <summary>
		/// Target's knockback distance if killed
		/// </summary>
		private const int KnockbackDistance = 220;

		/// <summary>
		/// Subscribes handlers to events required for training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttackedByPlayer += this.OnCreatureAttackedByPlayer;
		}

		/// <summary>
		/// Prepares and uses the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			// Item check
			if (creature.RightHand == null)
				return false;

			// Chain Check - Stage 2
			if (creature.Temp.FighterChainLevel != 2 || DateTime.Now > creature.Temp.FighterChainStartTime.AddMilliseconds((double)ChainMasteryInterval.Stage2))
				return false;

			// Discard unused packet string
			if (packet.Peek() == PacketElementType.String)
				packet.GetString();

			// No target entity ID...
			if (packet.Peek() == PacketElementType.None)
				return false;

			// In the case that there is a Long in the packet...
			var targetEntityId = packet.GetLong();
			var target = creature.Region.GetCreature(targetEntityId);
			if (target == null)
				return false;

			skill.State = SkillState.Ready;
			this.UseSkill(creature, skill, targetEntityId);

			return true;
		}

		/// <summary>
		/// Uses the skill
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void UseSkill(Creature attacker, Skill skill, long targetEntityId)
		{
			var target = attacker.Region.GetCreature(targetEntityId);

			var attackerPos = attacker.GetPosition();
			var targetPos = target.GetPosition();

			// Check target + collisions
			if (target == null || attacker.Region.Collisions.Any(attackerPos, targetPos))
			{
				Send.SkillUseSilentCancel(attacker);
				return;
			}

			// Stop movement
			attacker.StopMove();
			target.StopMove();

			Send.SkillUseEntity(attacker, skill.Info.Id, targetEntityId);
			skill.State = SkillState.Used;

			// Counter
			if (Counterattack.Handle(target, attacker))
				return;

			// Defense/Protection decrease on target
			var debuffChance = (int)skill.RankData.Var6;
			var defDecrease = (int)skill.RankData.Var3;
			var protDecrease = (int)skill.RankData.Var4;

			var extra = new MabiDictionary();
			extra.SetShort("NEW_DEF", (short)defDecrease);
			extra.SetShort("NEW_PROT", (short)protDecrease);
			extra.SetLong("DDP_CHAR", attacker.EntityId);
			extra.SetShort("DDP_SKILL", (short)skill.Info.Id);

			var rnd = RandomProvider.Get();
			if (rnd.NextDouble() * 100 < debuffChance)
			{
				Send.Effect(target, Effect.SpinningUppercutDebuff, (short)skill.Info.Id, 0, defDecrease, protDecrease);
				target.Conditions.Activate(ConditionsC.DefProtectDebuff, extra);
				attacker.Temp.SpinningUppercutDebuffApplied = true;
			}

			// Prepare Combat Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, targetEntityId, skill.Info.Id);
			aAction.Set(AttackerOptions.Result);

			var tAction = new TargetAction(CombatActionType.TakeHit | CombatActionType.Attacker, target, attacker, skill.Info.Id);
			tAction.Set(TargetOptions.Result | TargetOptions.FighterUnk);

			cap.Add(aAction, tAction);

			// Damage
			var damage = (attacker.GetRndFighterDamage() * (skill.RankData.Var1 / 100f));

			// Chain Mastery Damage Bonus
			var chainMasterySkill = attacker.Skills.Get(SkillId.ChainMastery);
			var damageBonus = (chainMasterySkill == null ? 0 : chainMasterySkill.RankData.Var1);
			damage += damage * (damageBonus / 100f);

			// Master Title - Damage +20%
			if (attacker.Titles.SelectedTitle == skill.Data.MasterTitle)
				damage += (damage * 0.2f);

			// Critical Hit
			var critChance = attacker.GetRightCritChance(target.Protection);
			CriticalHit.Handle(attacker, critChance, ref damage, tAction);

			// Handle skills and reductions
			SkillHelper.HandleDefenseProtection(target, ref damage);
			HeavyStander.Handle(attacker, target, ref damage, tAction);
			SkillHelper.HandleConditions(attacker, target, ref damage);
			ManaShield.Handle(target, ref damage, tAction);

			// Apply Damage
			target.TakeDamage(tAction.Damage = damage, attacker);

			// Aggro
			target.Aggro(attacker);

			// Stun Times
			tAction.Stun = TargetStun;
			aAction.Stun = AttackerStun;

			// Death and Knockback
			if (target.IsDead)
			{
				if (target.Is(RaceStands.KnockDownable))
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
					attacker.Shove(target, KnockbackDistance);
				}
				else
					tAction.Set(TargetOptions.Finished | TargetOptions.FinishingHit);
			}
			else
			{
				if (!target.IsKnockedDown)
					target.Stability -= StabilityReduction;

				if (target.IsUnstable && target.Is(RaceStands.KnockDownable))
				{
					tAction.Set(TargetOptions.KnockDown);
					attacker.Shove(target, KnockbackDistance);
				}
			}
			cap.Handle();

			// Chain Progress to Stage 3
			attacker.Temp.FighterChainStartTime = DateTime.Now;
			attacker.Temp.FighterChainLevel = 3;
		}

		/// <summary>
		/// Completes the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillCompleteEntity(creature, skill.Info.Id, packet.GetLong());
		}

		/// <summary>
		/// Cancels the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{

		}

		/// <summary>
		/// Skill training, called when someone attacks something
		/// </summary>
		/// <param name="action"></param>
		public void OnCreatureAttackedByPlayer(TargetAction action)
		{
			// Check skill
			if (action.AttackerSkillId != SkillId.SpinningUppercut)
				return;

			// Get skill
			var attackerSkill = action.Attacker.Skills.Get(SkillId.SpinningUppercut);
			if (attackerSkill == null) return;

			// Training
			switch (attackerSkill.Info.Rank)
			{
				case SkillRank.Novice:
					attackerSkill.Train(1); // Use the skill successfully.
					break;
				case SkillRank.RF:
				case SkillRank.RE:
				case SkillRank.RD:
				case SkillRank.RC:
				case SkillRank.RB:
				case SkillRank.RA:
				case SkillRank.R9:
				case SkillRank.R8:
				case SkillRank.R7:
				case SkillRank.R6:
					attackerSkill.Train(1); // Use the skill successfully.
					if (action.Attacker.Temp.SpinningUppercutDebuffApplied == true) attackerSkill.Train(2); // Decrease an enemy's Defense and Protection with Spinning Uppercut.
					action.Attacker.Temp.SpinningUppercutDebuffApplied = false; // Reset temp variable
					break;
				case SkillRank.R5:
				case SkillRank.R4:
				case SkillRank.R3:
					if (action.Has(TargetOptions.Critical)) attackerSkill.Train(1); // Get a Critical Hit with Spinning Uppercut.
					if (action.Attacker.Temp.SpinningUppercutDebuffApplied == true) attackerSkill.Train(2); // Decrease an enemy's Defense and Protection with Spinning Uppercut.
					action.Attacker.Temp.SpinningUppercutDebuffApplied = false; // Reset temp variable
					break;
				case SkillRank.R2:
					if (action.Attacker.Temp.SpinningUppercutDebuffApplied == true && action.Creature.HasTag("/ghost/")) attackerSkill.Train(1); // Lower the Defense and Protection of a Ghost.
					action.Attacker.Temp.SpinningUppercutDebuffApplied = false; // Reset temp variable
					break;
				case SkillRank.R1:
					if (action.Has(TargetOptions.Critical)) attackerSkill.Train(1); // Get a Critical Hit with Spinning Uppercut.
					if (action.Attacker.Temp.SpinningUppercutDebuffApplied == true) attackerSkill.Train(2); // Decrease an enemy's Defense and Protection with Spinning Uppercut.
					action.Attacker.Temp.SpinningUppercutDebuffApplied = false; // Reset temp variable
					break;
			}
		}
	}
}
