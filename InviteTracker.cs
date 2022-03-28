// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using DSharpPlus;

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

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static BotSettings ReadSettings()
        {
            var sr = new StreamReader("appsettings.json");
            var jsonContent = sr.ReadToEnd();
            return JsonSerializer.Deserialize<BotSettings>(jsonContent) ?? throw new InvalidOperationException();
        }

        private class BotSettings
        {
            public string? Token;
        }
    }
}
