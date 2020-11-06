using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace YK_Bot.Entities.ReactionRoleIO
{
    public class RemoveRole : ReactionRoleOperation
    {
        public RemoveRole(DiscordMember m, DiscordRole r) : base(m, r)
        {
        }

        public override async Task DoOperationAsync() => await _member.RevokeRoleAsync(_role);
    }
}
