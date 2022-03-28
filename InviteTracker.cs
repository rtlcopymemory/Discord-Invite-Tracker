// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InviteTracker.Commands;
using LiteDB;

namespace InviteTracker
{
    internal static class InviteTracker
    {
        public static async Task Main(string[] args)
        {
            var settings = ReadSettings();
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = settings.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });

            var commands = new List<Command>()
            {
                new SetLogChannel(settings),
                new ForceSync(settings)
            };

            foreach (var command in commands)
            {
                await command.Register();
                // test server
                await command.RegisterToServer("764229893042733097");
            }

            discord.InteractionCreated += async (sender, eventArgs) =>
            {
                foreach (var command in commands)
                {
                    await command.Handle(sender, eventArgs);
                }
            };

            discord.InviteCreated += async (sender, eventArgs) =>
            {
                HandleInviteCreate(settings, sender, eventArgs);
            };

            discord.GuildMemberAdded += async (sender, eventArgs) =>
            {
                await HandleMemberJoin(settings, sender, eventArgs);
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static BotSettings ReadSettings()
        {
            var jsonContent = File.ReadAllText("appsettings.json");
            return JsonConvert.DeserializeObject<BotSettings>(jsonContent) ?? throw new InvalidOperationException();
        }

        private static void HandleInviteCreate(BotSettings settings, DiscordClient sender, InviteCreateEventArgs eventArgs)
        {
            using var db = new LiteDatabase(settings.DbPath);
            var col = db.GetCollection<Invite>("invites");
            var evInvite = eventArgs.Invite;
                
            var invite = new Invite()
            {
                InviteCode = evInvite.Code,
                Uses = evInvite.Uses,
                ExprireDate = evInvite.ExpiresAt,
                InviterId = evInvite.Inviter.Id.ToString(),
                MaxUses = evInvite.MaxUses,
                ServerId = eventArgs.Guild.Id.ToString()
            };
                
            var exists = col.FindOne(x => x.InviteCode == evInvite.Code);
            if (exists != null)
            {
                // If it exists. (uh???)
                exists.Uses = invite.Uses;
                exists.ExprireDate = invite.ExprireDate;
                exists.InviteCode = invite.InviteCode;
                exists.InviterId = invite.InviterId;
                exists.MaxUses = invite.MaxUses;
                exists.ServerId = invite.ServerId;
                    
                col.Update(exists);
                return;
            }
                
            col.Insert(invite);
        }

        private static async Task HandleMemberJoin(BotSettings settings, DiscordClient sender,
            GuildMemberAddEventArgs eventArgs)
        {
            var now = DateTimeOffset.Now;
            var invites = await eventArgs.Guild.GetInvitesAsync();
            var serverId = eventArgs.Guild.Id.ToString();
            using var db = new LiteDatabase(settings.DbPath);

            var col = db.GetCollection<Invite>("invites");
            var serverCol = db.GetCollection<Server>("servers");
            var channel = await sender.GetChannelAsync(ulong.Parse(serverCol.FindOne(x => x.ServerId == serverId).ChannelId));

            Invite? usedInvite = null;
            var toUpdate = false;
            foreach (var invite in invites)
            {
                var saved = col.Query()
                    .Where(x => x.ServerId == serverId && x.InviteCode == invite.Code).ToList();

                
                if (saved.Count < 1)
                {
                    // This is a new invite || vanity url
                    // Maybe created while the bot was down
                    toUpdate = true;
                }

                var foundInDb = saved.First();

                if (foundInDb.ExprireDate < now)
                {
                    // It is expired, remove
                    col.Delete(foundInDb.Id);
                    continue;
                }

                if (foundInDb.Uses >= invite.Uses) continue;
                
                // The uses increased, it's likely this.
                usedInvite = new Invite()
                {
                    Id = foundInDb.Id,
                    Uses = invite.Uses,
                    ExprireDate = invite.ExpiresAt,
                    InviteCode = invite.Code,
                    InviterId = invite.Inviter.Id.ToString(),
                    MaxUses = invite.MaxUses,
                    ServerId = serverId
                };
                    
                col.Update(usedInvite);
                break;
            }
            
            var embed = new DiscordEmbedBuilder();
            if (usedInvite is null)
            {
                embed.Title = "New Join from vanity/unknown invite";
                embed.Description = "Couldn't find the invite in the saved DB";
                
                await channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));
                return;
            }

            embed.Title = "New Join from invite";
            embed.Description = "Remember that this bot cannot determine with 100% accuracy if the invite is correct";
            embed.AddField("Inviter ID", usedInvite.InviterId, true);
            embed.AddField("Inviter Mention", $"<@{usedInvite.InviterId}>", true);
            embed.AddField("Joiner", eventArgs.Member.Id.ToString(), true);
            embed.AddField("Joiner Mention", $"<@eventArgs.Member.Id.ToString()>", true);
            
            var message = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder());
            await channel.SendMessageAsync(message);

            if (toUpdate) await SyncInvites(sender, serverId, settings);
        }

        public static async Task SyncInvites(DiscordClient client, string serverId, BotSettings settings)
        {
            using var db = new LiteDatabase(settings.DbPath);
            var guild = await client.GetGuildAsync(ulong.Parse(serverId));
            var invites = await guild.GetInvitesAsync();

            foreach (var invite in invites)
            {
                var col = db.GetCollection<Invite>("invites");

                var exists = col.FindOne(x => x.InviteCode == invite.Code);
                if (exists != null)
                {
                    exists.Uses = invite.Uses;
                    exists.ExprireDate = invite.ExpiresAt;
                    exists.InviteCode = invite.Code;
                    exists.InviterId = invite.Inviter.Id.ToString();
                    exists.MaxUses = invite.MaxUses;
                    exists.ServerId = serverId;
                    
                    col.Update(exists);
                    continue;
                }

                var newInvite = new Invite()
                {
                    Uses = invite.Uses,
                    ExprireDate = invite.ExpiresAt,
                    InviteCode = invite.Code,
                    InviterId = invite.Inviter.Id.ToString(),
                    MaxUses = invite.MaxUses,
                    ServerId = serverId
                };
                col.Insert(newInvite);
            }
        }
    }
}
