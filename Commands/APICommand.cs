namespace InviteTracker.Commands;

// ReSharper disable InconsistentNaming
public class ApiCommand
{
    public string? name;
    public CommandType? type;
    public string? description;
    public List<ApiCommandOption>? options;
}

public class ApiCommandOption
{
    public string? name;
    public string? description;
    public OptionType type;
    public bool required;
    public List<ApiOptionChoice>? choices;
}

public class ApiOptionChoice
{
    public string? name;
    public string? value;
}