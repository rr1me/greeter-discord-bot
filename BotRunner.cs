using Discord;
using Discord.WebSocket;
using myGreeterBot.Data;

namespace myGreeterBot;

public class BotRunner
{

    private readonly IServiceProvider _serviceProvider;
    
    public BotRunner()
    {
        _serviceProvider = CreateProvider();
    }

    private IServiceProvider CreateProvider()
    {
        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All
        };

        var collection = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>();

        return collection.BuildServiceProvider();
    }

    private DiscordSocketClient _client;

    public async Task Run()
    {
        _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

        _client.Log += Log;

        _client.ReactionAdded += GetReactionMethod(true);
        _client.ReactionRemoved += GetReactionMethod(false);
        
        string token = new Settings().GetSettings().token;
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        
        await Task.Delay(-1);
    }

    private readonly Dictionary<string, ulong> emoteAndRole = new()
    {
        {"🤔", 1044049914965012520},
        {"🖕", 1044049961416917022}
    };

    private Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task> GetReactionMethod(bool isAdded) 
        => (_, _, socketReaction) => 
        {
            var emoteName = socketReaction.Emote.Name;
            var user = socketReaction.User.Value as IGuildUser;
    
            if (isAdded)
                user.AddRoleAsync(emoteAndRole[emoteName]);
            else
                user.RemoveRoleAsync(emoteAndRole[emoteName]);

            return Task.CompletedTask; 
        };

    private Task Log(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
}