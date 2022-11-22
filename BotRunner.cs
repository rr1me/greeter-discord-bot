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
                // GatewayIntents.GuildMessageReactions |
                // GatewayIntents.GuildMembers |
                // GatewayIntents.GuildPresences |
                // GatewayIntents.Guilds |
                // GatewayIntents.GuildMessages | GatewayIntents.MessageContent | 
                GatewayIntents.All
        };

        var collection = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<Settings>()
            .AddSingleton(this);

        Console.WriteLine(collection.BuildServiceProvider());
        return collection.BuildServiceProvider();
    }

    private DiscordSocketClient _client;
    private CommandService _commands;
    private SettingsEntity settings;

    public async Task Run()
    {
        _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        _commands = _serviceProvider.GetRequiredService<CommandService>();
        settings = _serviceProvider.GetRequiredService<Settings>().GetSettings();

        _client.Log += Log;
        _client.Ready += OnReady;
        _client.MessageReceived += OnMessageReceived;
        _client.UserJoined += OnUserJoined;

        _client.ReactionAdded += GetReactionMethod(true);
        _client.ReactionRemoved += GetReactionMethod(false);
        
        var token = settings.Token;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _serviceProvider);

        await Task.Delay(-1);
    }

    private async Task OnMessageReceived(SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;
        
        Console.WriteLine(!(message.HasCharPrefix('!', ref argPos) || 
                            message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || 
                          message.Author.IsBot);
        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if (!(message.HasCharPrefix('!', ref argPos) || 
              message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;

        // Create a WebSocket-based command context based on the message
        var context = new SocketCommandContext(_client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await _commands.ExecuteAsync(
            context: context, 
            argPos: argPos,
            services: _serviceProvider);
    }

    public async Task UpdateSettings()
    {
        settings = _serviceProvider.GetRequiredService<Settings>().GetSettings();
        CompareSettings();
    }

    private async Task OnReady() => CompareSettings();

    private async Task CompareSettings()
    {
        var chnl = _client.GetChannel(settings.MentionChannelId) as IMessageChannel;
        var message = await chnl.GetMessageAsync(settings.MessageId);

        var messageText = settings.MessageText;
        if (message.Content != messageText)
        {
            message = await chnl.SendMessageAsync(messageText);
            
            settings.MessageId = message.Id;
            var settingsService = _serviceProvider.GetRequiredService<Settings>();
            settingsService.SaveSettings(settings);
        }
            
        var reactions = message.Reactions;
        CheckReactions(message, reactions);
    }

    private async Task CheckReactions(IMessage message, IReadOnlyDictionary<IEmote, ReactionMetadata> reactions)
    {

        var formalReactions = settings.EmoteAndRole.Keys;
        var actualReactions = reactions.Keys.Select(x => x.ToString());
        
        foreach (var emoteUnicode in formalReactions)
        {
            if (reactions.Count == 0 || !actualReactions.Contains(emoteUnicode))
                message.AddReactionAsync(Emote.Parse(emoteUnicode));
        }
    }

    private Task OnUserJoined(SocketGuildUser user)
    {
        _ = Task.Run(async () =>
        {
            var channel = _client.GetChannel(settings.MentionChannelId) as IMessageChannel;
            var msg = await channel.SendMessageAsync(user.Mention);
    
            msg.DeleteAsync();
        });
    
        return Task.CompletedTask;
    }

    private Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task> GetReactionMethod(bool isAdded) 
        => (_, _, socketReaction) =>
        {
            if (socketReaction.MessageId != settings.MessageId || socketReaction.User.Value.IsBot)
                return Task.CompletedTask;

            var user = socketReaction.User.Value as IGuildUser;
            var emoteId = socketReaction.Emote.ToString();

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