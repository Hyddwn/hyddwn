﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Shared.Database
{
	public class Guild
	{
		private Dictionary<long, GuildMember> _members;

		public long Id { get; set; }
		public string Name { get; set; }
		public string LeaderName { get; set; }
		public string Title { get; set; }

		public int Points { get; set; }
		public int Gold { get; set; }

		public string IntroMessage { get; set; }
		public string WelcomeMessage { get; set; }
		public string LeavingMessage { get; set; }
		public string RejectionMessage { get; set; }

		public GuildType Type { get; set; }
		public GuildLevel Level { get; set; }
		public GuildOptions Options { get; set; }
		public GuildStone Stone { get; set; }
		public bool HasStone { get { return (this.Stone.RegionId != 0); } }

		public int MemberCount { get { lock (_members) return _members.Count; } }

		public Guild()
		{
			_members = new Dictionary<long, GuildMember>();
			this.Stone = new GuildStone();
		}

		public bool Has(GuildOptions options)
		{
			return (this.Options & options) != 0;
		}

		public void InitMembers(IEnumerable<GuildMember> members)
		{
			lock (_members)
			{
				_members.Clear();

				foreach (var member in members)
					_members[member.CharacterId] = member;
			}
		}

		public GuildMember GetMember(long characterId)
		{
			GuildMember result;
			lock (_members)
				_members.TryGetValue(characterId, out result);
			return result;
		}

		public List<GuildMember> GetMembers()
		{
			lock (_members)
				return _members.Values.ToList();
		}
	}
}
