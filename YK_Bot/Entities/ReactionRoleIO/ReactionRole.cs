using System.Collections.Generic;

namespace YK_Bot.Entities.ReactionRoleIO
{
    public class ReactionRole
    {
        public ulong MessageId { get; set; }
        public List<ReactionRoleCombination> ReactionIds { get; set; } =
            new List<ReactionRoleCombination>();

        public class ReactionRoleCombination
        {
            public ulong RoleId { get; set; }
            public string EmojiName { get; set; }
        }
    }
}
