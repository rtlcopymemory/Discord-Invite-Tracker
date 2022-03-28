namespace InviteTracker.Commands;

public class SetLogChannel: Command
{
    public SetLogChannel(BotSettings settings): base(settings, PrepareCommand()) { }

    private static ApiCommand PrepareCommand()
    {
        return new ApiCommand()
        {
            name = "set_channel",
            description = "Set Channel where to post logs",
            type = CommandType.ChatInput,
            options = new List<ApiCommandOption>(new []
            {
                new ApiCommandOption()
                {
                    name = "channel",
                    description = "The channel",
                    required = true,
                    type = OptionType.Channel
                }
            })
        };
    }
}