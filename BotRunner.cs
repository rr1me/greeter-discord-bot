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
            GatewayIntents = 
                GatewayIntents.GuildMessageReactions | 
                             // GatewayIntents.DirectMessages | GatewayIntents.GuildBans | 
                             // GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations |
                             // GatewayIntents.GuildInvites | 
                             GatewayIntents.GuildMembers | 
                             // GatewayIntents.GuildMessages | 
                             GatewayIntents.GuildPresences | 
            //                  GatewayIntents.GuildWebhooks | GatewayIntents.MessageContent | GatewayIntents.DirectMessageReactions |
            //                  GatewayIntents.DirectMessageTyping | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessageTyping |
            //                  GatewayIntents.GuildScheduledEvents | 
                             // GatewayIntents.GuildVoiceStates | 
                             GatewayIntents.Guilds
        };

        var collection = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<Settings>();

        return collection.BuildServiceProvider();
    }

    private DiscordSocketClient _client;
    private SettingsEntity settings;

    public async Task Run()
    {
        _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        settings = _serviceProvider.GetRequiredService<Settings>().GetSettings();

        _client.Log += Log;
        _client.UserJoined += OnUserJoined;

        _client.ReactionAdded += GetReactionMethod(true);
        _client.ReactionRemoved += GetReactionMethod(false);
        
        var token = settings.Token;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        
        await Task.Delay(-1);
    }

    private async Task OnUserJoined(SocketGuildUser user)
    {
        var channel = _client.GetChannel(settings.MentionChannelId) as IMessageChannel;
        var msg = await channel.SendMessageAsync(user.Mention);

        msg.DeleteAsync();
    }

    private Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task> GetReactionMethod(bool isAdded) 
        => (_, _, socketReaction) =>
        {
            if (socketReaction.MessageId != settings.MessageId)
                return Task.CompletedTask;
            
            var emoteName = socketReaction.Emote.Name;
            var user = socketReaction.User.Value as IGuildUser;

            var emoteAndRole = settings.EmoteAndRole;
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