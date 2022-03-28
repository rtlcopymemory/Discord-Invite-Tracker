// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using DSharpPlus;
using DSharpPlus.Entities;
using InviteTracker.Commands;

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

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static BotSettings ReadSettings()
        {
            var jsonContent = File.ReadAllText("appsettings.json");
            return JsonConvert.DeserializeObject<BotSettings>(jsonContent) ?? throw new InvalidOperationException();
        }
    }
    
    public class BotSettings
    {
        public string Token;
        public string ApplicationId;
        public string DbPath;
    }
}
