using DSharpPlus;
using DSharpPlus.Entities;
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
            var member = await eventArgs.Interaction.Guild.GetMemberAsync(eventArgs.Interaction.User.Id);
            if (!member.Permissions.HasPermission(Permissions.BanMembers) && !member.Permissions.HasPermission(Permissions.Administrator)) return;

            await InviteTracker.SyncInvites(sender, eventArgs.Interaction.Guild.Id.ToString(), Settings);

            var message = new DiscordInteractionResponseBuilder().WithContent("Synced");
            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
        }
    }
}