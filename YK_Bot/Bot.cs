using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using Microsoft.Extensions.Logging;
using DSharpPlus.Interactivity.Extensions;
using System.IO;
using System.Text.Json;
using YK_Bot.Entities;
using System.Collections.Concurrent;
using YK_Bot.Entities.ReactionRoleIO;
using DSharpPlus.Entities;
using YK_Bot.Commands;

namespace YK_Bot
{
    public class Bot
    {
        const ulong HakutoFanRoleId = 761394698745020468;
        private DiscordClient _client { get; set; }
        private CommandsNextExtension _cNext { get; set; }
        private InteractivityExtension _iNext { get; set; }

        private ConcurrentQueue<ReactionRoleOperation> _reactionRoleOperations { get; set; } =
            new ConcurrentQueue<ReactionRoleOperation>();

        public Bot()
        {
            if (!Directory.Exists(@"ReactionRoleMessages"))
                Directory.CreateDirectory(@"ReactionRoleMessages");

            _client = new DiscordClient(new DiscordConfiguration
            {
                MinimumLogLevel = LogLevel.Debug,
                Token = "" //removed
            });
            _cNext = _client.UseCommandsNext(new CommandsNextConfiguration
            {
                EnableDefaultHelp = false,
                StringPrefixes = new[] { "!" }
            });
            _iNext = _client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteEmojis,
                PollBehaviour = PollBehaviour.DeleteEmojis
            });

            _cNext.RegisterCommands<Mod>();

            _cNext.CommandErrored += CommandError;

            _client.GuildMemberAdded += AutoRole;
            _client.MessageReactionAdded += HandleAddReactionRole;
            _client.MessageReactionRemoved += HandleRemoveReactionRole;
            _client.Ready += ReadyHandle;
        }

        private Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            Console.WriteLine(e.Exception);
            return Task.CompletedTask;
        }

        public async Task RunBot()
        {
            await _client.ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task ReadyHandle(DiscordClient sender, ReadyEventArgs e)
        {
            _ = Task.Run(HandleQueue);
            await Task.Delay(1); //await sender.UpdateStatusAsync(new DiscordActivity("Test", ActivityType.Watching));
        }

        private async Task HandleRemoveReactionRole(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            if (!File.Exists(@$"ReactionRoleMessages\\{e.Message.Id}.json")) return;
            var text = await File.ReadAllTextAsync(@$"ReactionRoleMessages\\{e.Message.Id}.json");
            var rrData = JsonSerializer.Deserialize<ReactionRole>(text);
            if (!rrData.ReactionIds.Any(x => x.EmojiName == e.Emoji.ToString())) return;

            var roleId = rrData.ReactionIds.First(x => x.EmojiName == e.Emoji.ToString()).RoleId;
            var role = e.Guild.GetRole(roleId);

            if (!_reactionRoleOperations.Any(x => x is RemoveRole && x._member.Id == e.User.Id && x._role.Id == role.Id))
                _reactionRoleOperations.Enqueue(new RemoveRole((DiscordMember)e.User, role));
        }

        private async Task HandleAddReactionRole(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            if (!File.Exists(@$"ReactionRoleMessages\\{e.Message.Id}.json")) return;
            var text = await File.ReadAllTextAsync(@$"ReactionRoleMessages\\{e.Message.Id}.json");
            var rrData = JsonSerializer.Deserialize<ReactionRole>(text);
            if (!rrData.ReactionIds.Any(x => x.EmojiName == e.Emoji.ToString())) return;

            var roleId = rrData.ReactionIds.First(x => x.EmojiName == e.Emoji.ToString()).RoleId;
            var role = e.Guild.GetRole(roleId);

            if (!_reactionRoleOperations.Any(x => x is AddRole && x._member.Id == e.User.Id && x._role.Id == role.Id))
                _reactionRoleOperations.Enqueue(new AddRole((DiscordMember)e.User, role));
        }

        private async Task HandleQueue()
        {
            while (true)
            {
                await Task.Delay(500);
                if (_reactionRoleOperations.IsEmpty) continue;
                var yes = _reactionRoleOperations.TryDequeue(out var item);
                if (yes) await item.DoOperationAsync();
            }
        }

        private async Task AutoRole(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            var role = e.Guild.GetRole(HakutoFanRoleId);
            await e.Member.GrantRoleAsync(role);
        }
    }
}
