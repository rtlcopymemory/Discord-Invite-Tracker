using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace InviteTracker.Commands;

public abstract class Command
{
    private readonly ApiCommand _command;
    private readonly AuthenticationHeaderValue? _authHeader;
    private readonly string? _url;

    public string? Name => _command.name;

    protected Command(BotSettings settings, ApiCommand command)
    {
        _command = command;
        _authHeader = new AuthenticationHeaderValue("Bot", settings.Token);
        _url = $"https://discord.com/api/v9/applications/{settings.ApplicationId}";
    }
    
    public async Task Register()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_url}/commands");
        await MakeRegisterRequest(request);
    }

    public async Task RegisterToServer(string serverId)
    {
        
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_url}/guilds/{serverId}/commands");
        await MakeRegisterRequest(request);
    }

    private async Task MakeRegisterRequest(HttpRequestMessage request)
    {
        var client = new HttpClient();
        
        var body = new StringContent(JsonConvert.SerializeObject(_command, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");

        request.Headers.Authorization = _authHeader;
        request.Content = body;
        var response = await client.SendAsync(request);

        var code = response.StatusCode;
        if (code != HttpStatusCode.Created && code != HttpStatusCode.OK)
        {
            throw new WebException($"Command registration returned code different than OK\n{code}\n{response.Content}");
        }
    }
}