using DSharpPlus;
using DSharpPlus.EventArgs;

namespace InviteTracker.Commands;

public class ForceSync: Command
{
    public ForceSync(BotSettings settings): base(settings, PrepareCommand()) { }

    private static ApiCommand PrepareCommand()
    {
        return new ApiCommand()
        {
            name = "force_sync",
            description = "Force the syncing of invites. Useful after bot downtime",
            type = CommandType.ChatInput
        };
    }
    
    public override async Task Handle(DiscordClient sender, InteractionCreateEventArgs eventArgs)
    {
        if (eventArgs.Interaction.Data.Name == Name)
        {
            await InviteTracker.SyncInvites(sender, eventArgs.Interaction.Guild.Id.ToString(), Settings);
        }
    }
}