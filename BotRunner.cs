using System.Reflection;
using Discord;
using Discord.Commands;
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
            GatewayIntents =
                GatewayIntents.GuildMessageReactions |
                GatewayIntents.GuildMembers |
                GatewayIntents.GuildPresences |
                GatewayIntents.Guilds |
                GatewayIntents.GuildMessages | GatewayIntents.MessageContent
        };

        var collection = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<Settings>()
            .AddSingleton<Events>()
            .AddSingleton<Miscellaneous>();

        Console.WriteLine(collection.BuildServiceProvider());
        return collection.BuildServiceProvider();
    }

    private DiscordSocketClient _client;
    private CommandService _commands;

    private Events _events;

    public async Task Run()
    {
        _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        _commands = _serviceProvider.GetRequiredService<CommandService>();
        var token = _serviceProvider.GetRequiredService<Settings>().GetSettings().Token;

        _events = _serviceProvider.GetRequiredService<Events>();

        _client.Log += _events.Log;
        _client.Ready += _events.OnReady;
        _client.MessageReceived += _events.OnMessageReceived;
        _client.UserJoined += _events.OnUserJoined;

        _client.ReactionAdded += _events.GetReactionMethod(true);
        _client.ReactionRemoved += _events.GetReactionMethod(false);
        
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _serviceProvider);

        await Task.Delay(-1);
    }
}