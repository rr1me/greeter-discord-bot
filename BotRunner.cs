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
                GatewayIntents.GuildMembers |
                GatewayIntents.GuildPresences |
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
        _client.Ready += OnReady;
        // _client.UserJoined += OnUserJoined;

        _client.ReactionAdded += GetReactionMethod(true);
        _client.ReactionRemoved += GetReactionMethod(false);
        
        var token = settings.Token;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        
        await Task.Delay(-1);
    }

    private async Task OnReady()
    {
        var chnl = _client.GetChannel(settings.MentionChannelId) as IMessageChannel;
        var message = await chnl.GetMessageAsync(settings.MessageId);
        
        
        // var chnl = _client.GetChannel(925140676482580540) as IMessageChannel;
        // var message = await chnl.GetMessageAsync(1044320604024733796);
        
        // Console.WriteLine(message.Reactions);
        // Console.WriteLine(message.Reactions);

        var messageText = settings.MessageText;
        
        if (message.Content != messageText)
        {
            message = await chnl.SendMessageAsync(messageText);
        
            settings.MessageId = message.Id;
            var settingsService = _serviceProvider.GetRequiredService<Settings>();
            settingsService.SaveSettings(settings);
        }   
        
        var reactions = message.Reactions;
        await CheckReactions(message, reactions);
    }

    private async Task CheckReactions(IMessage message, IReadOnlyDictionary<IEmote, ReactionMetadata> reactions)
    {
        foreach (var emoteUnicode in settings.EmoteAndRole.Keys)
        {
            // if (reactions.Count == 0 || !reactions.Keys.Select(x=>x.Name).Contains(emoteUnicode))
            //     await message.AddReactionAsync(Emote.Parse(emoteUnicode));
        }
    }
    
    // "🤔": 1044049914965012520,
    // "🖕": 1044049961416917022,

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
            
            var emoteId = socketReaction.Emote.ToString();
            
            var user = socketReaction.User.Value as IGuildUser;

            var emoteAndRole = settings.EmoteAndRole;
            if (isAdded)
                user.AddRoleAsync(emoteAndRole[emoteId]);
            else
                user.RemoveRoleAsync(emoteAndRole[emoteId]);

            return Task.CompletedTask; 
        };

    private Task Log(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
}