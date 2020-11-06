using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace YK_Bot.Entities.ReactionRoleIO
{
    public abstract class ReactionRoleOperation
    {
        internal DiscordMember _member { get; set; }

        internal DiscordRole _role { get; set; }

        public ReactionRoleOperation(DiscordMember m, DiscordRole r)
        {
            this._member = m;
            this._role = r;
        }

        public abstract Task DoOperationAsync();
    }
}
