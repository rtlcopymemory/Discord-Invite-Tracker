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

            var setLogChannel = new SetLogChannel(settings);
            await setLogChannel.Register();
            await setLogChannel.RegisterToServer("764229893042733097");

            discord.InteractionCreated += async (sender, eventArgs) =>
            {
                await setLogChannel.Handle(sender, eventArgs);
            };

            discord.InviteCreated += async (sender, eventArgs) =>
            {
                HandleInviteCreate(settings, sender, eventArgs);
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
                MaxUses = evInvite.MaxUses
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
                    
                col.Update(exists);
                return;
            }
                
            col.Insert(invite);
        }
    }
}
