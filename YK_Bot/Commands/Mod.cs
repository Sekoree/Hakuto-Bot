using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YK_Bot.Attributes;
using YK_Bot.Entities.ReactionRoleIO;

namespace YK_Bot.Commands
{
    [Group("Mod")]
    [RequireRoleById(761395935325454378)]
    public class Mod : BaseCommandModule
    {
        [Command("CreateReactonRoleMessage")]
        [Aliases("crrm")]
        public async Task CreateReactonRoleMessageAsync(CommandContext ctx, DiscordChannel channel = null)
        {
            if (channel == null) channel = ctx.Channel;
            var inter = ctx.Client.GetInteractivity();
            var msg = await ctx.RespondAsync("Please enter the ID of the Role, **without a prefix**");
            var result = await inter.WaitForMessageAsync(x => x.Channel == channel && x.Author.Id == ctx.User.Id);

            var rr = new ReactionRole();
            while (true)
            {
                if (result.TimedOut) return;
                else if (result.Result?.Content.ToLower() == "next")
                {
                    await result.Result.DeleteAsync();
                    break;
                }

                if (ulong.TryParse(result.Result.Content, out var roleId))
                {
                    var rrStuff = new ReactionRole.ReactionRoleCombination();
                    rrStuff.RoleId = roleId;
                    var role = ctx.Guild.GetRole(roleId);
                    await result.Result.DeleteAsync();
                    await msg.ModifyAsync("Please enter the emoji you'd like to use for this role now");
                    result = await inter.WaitForMessageAsync(x => x.Channel == channel && x.Author.Id == ctx.User.Id);

                    rrStuff.EmojiName = result.Result.Content.Replace(" ", "");
                    rr.ReactionIds.Add(rrStuff);

                    rrStuff = new ReactionRole.ReactionRoleCombination();
                    await msg.ModifyAsync("Please enter the next role ID or enter 'next' to set text");
                    await result.Result.DeleteAsync();
                    result = await inter.WaitForMessageAsync(x => x.Channel == channel && x.Author.Id == ctx.User.Id);
                }
            }
            await msg.ModifyAsync("Please enter the message that should be send now");
            result = await inter.WaitForMessageAsync(x => x.Channel == channel && x.Author.Id == ctx.User.Id);
            if (result.TimedOut) return;

            var resMsg = await channel.SendMessageAsync(result.Result.Content);

            Regex EmoteRegex = new Regex(@"^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

            foreach (var item in rr.ReactionIds)
            {
                DiscordEmoji em = default;
                try
                {
                    em = DiscordEmoji.FromUnicode(ctx.Client, item.EmojiName);
                }
                catch { } 

                var m = EmoteRegex.Match(item.EmojiName);
                if (m.Success)
                {
                    var sid = m.Groups["id"].Value;
                    var name = m.Groups["name"].Value;
                    var anim = m.Groups["animated"].Success;

                    if (!ulong.TryParse(sid, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                        return;

                    try
                    {
                        em = DiscordEmoji.FromGuildEmote(ctx.Client, id);
                    }
                    catch { return; }
                }
                if (em == default)
                    return;
                await resMsg.CreateReactionAsync(em);

                rr.MessageId = resMsg.Id;
                var txt = JsonSerializer.Serialize(rr);
                await File.WriteAllTextAsync($@"ReactionRoleMessages\\{resMsg.Id}.json", txt);
            }

        }
    }
}
