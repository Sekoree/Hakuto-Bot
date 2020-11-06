using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YK_Bot.Attributes
{
    public class RequireRoleById : CheckBaseAttribute
    {
        private ulong roleId { get; set; }

        public RequireRoleById(ulong id)
        {
            this.roleId = id;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Channel.Type != DSharpPlus.ChannelType.Text || ctx.Channel.Type == DSharpPlus.ChannelType.Private) return Task.FromResult(false);
            if (ctx.Member.Roles.Any(x => x.Id == roleId)) return Task.FromResult(true);
            return Task.FromResult(false);
        }
    }
}
