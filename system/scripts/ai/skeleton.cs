//--- Aura Script -----------------------------------------------------------
// Skeleton AI
//--- Description -----------------------------------------------------------
// AI for skeletons.
//---------------------------------------------------------------------------

[AiScript("skeleton")]
public class SkeletonAi : AiScript
{
	public SkeletonAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(400);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		Do(KeepDistance(400, false, 2000));
		Do(Circle(300, 1000, 1000));

		SwitchRandom();
		if (Case(40))
		{	
			if (HasEquipped("/bow/") || HasEquipped("/bow01/") || HasEquipped("/crossbow/"))
			{
				Do(SwitchTo(WeaponSet.Second));
				Do(RangedAttack());
			}
			else
				Do(Attack(3));
		}
		else if (Case(20))
		{
			Do(SwitchTo(WeaponSet.First));
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (Case(20))
		{
			Do(SwitchTo(WeaponSet.First));
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			Do(SwitchTo(WeaponSet.First));
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(5000, 5000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(SwitchTo(WeaponSet.First));
		Do(Attack(3));
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{

		if (Creature.Life < Creature.LifeMax * 0.20f)
		{
			
			if (Random() < 50)
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
			else 
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
		}
		else
		{
			SwitchRandom();
			if (Case(40))
			{
				if (HasSkill(SkillId.Windmill))
				{
					Do(SwitchTo(WeaponSet.First));
					Do(PrepareSkill(SkillId.Windmill));
					//Do(Wait(4000, 4000));
					Do(UseSkill());
				}
				else if (Random() < 50)
				{
					Do(SwitchTo(WeaponSet.First));
					Do(PrepareSkill(SkillId.Smash));
					Do(Attack(1, 4000));
				}
				else
				{
					Do(SwitchTo(WeaponSet.First));
					Do(PrepareSkill(SkillId.Defense));
					Do(Wait(2000, 4000));
					Do(CancelSkill());
				}
			}
			else if (Case(30))
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else if(Case(30))
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
		}
	}
}
