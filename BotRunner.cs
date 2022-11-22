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
            
            // .AddSingleton(this)
            .AddSingleton<Events>()
            .AddSingleton<Miscellaneous>();

        Console.WriteLine(collection.BuildServiceProvider());
        return collection.BuildServiceProvider();
    }

    private DiscordSocketClient _client;
    private CommandService _commands;
    // private SettingsEntity settings;

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

    // private async Task OnMessageReceived(SocketMessage messageParam)
    // {
    //     var message = messageParam as SocketUserMessage;
    //     if (message == null) return;
    //     
    //     int argPos = 0;
    //
    //     if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author.IsBot)
    //         return;
    //     
    //     var context = new SocketCommandContext(_client, message);
    //
    //     await _commands.ExecuteAsync(context: context, argPos: argPos, services: _serviceProvider);
    // }
    //
    // public async Task UpdateSettings()
    // {
    //     settings = _serviceProvider.GetRequiredService<Settings>().GetSettings();
    //     CompareSettings();
    // }
    //
    // private async Task OnReady() => CompareSettings();
    //
    // private async Task CompareSettings()
    // {
    //     var chnl = _client.GetChannel(settings.MentionChannelId) as IMessageChannel;
    //     var message = await chnl.GetMessageAsync(settings.MessageId);
    //
    //     var messageText = settings.MessageText;
    //     if (message.Content != messageText)
    //     {
    //         message = await chnl.SendMessageAsync(messageText);
    //         
    //         settings.MessageId = message.Id;
    //         var settingsService = _serviceProvider.GetRequiredService<Settings>();
    //         settingsService.SaveSettings(settings);
    //     }
    //         
    //     var reactions = message.Reactions;
    //     CheckReactions(message, reactions);
    // }
    //
    // private async Task CheckReactions(IMessage message, IReadOnlyDictionary<IEmote, ReactionMetadata> reactions)
    // {
    //     var formalReactions = settings.EmoteAndRole.Keys;
    //
    //     if (reactions.Count > formalReactions.Count)
    //     {
    //         foreach (var actualReaction in reactions.Keys)
    //         {
    //             if (!formalReactions.Contains(actualReaction.ToString()))
    //                 await message.RemoveAllReactionsForEmoteAsync(actualReaction);
    //         }
    //     }
    //     foreach (var emoteUnicode in formalReactions)
    //     {
    //         var reactionKeys = reactions.Keys.Select(x=>x.ToString());
    //         
    //         if (reactions.Count == 0 || !reactionKeys.Contains(emoteUnicode))
    //             await message.AddReactionAsync(Emote.Parse(emoteUnicode));
    //     }
    // }
    //
    // private Task OnUserJoined(SocketGuildUser user) => 
    //     Task.Run(async () => 
    //     {
    //         var channel = _client.GetChannel(settings.MentionChannelId) as IMessageChannel;
    //         var msg = await channel.SendMessageAsync(user.Mention);
    //
    //         msg.DeleteAsync();
    //     });
    //
    // private Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task> GetReactionMethod(bool isAdded) 
    //     => async (_, _, socketReaction) =>
    //     {
    //         var emoteAndRole = settings.EmoteAndRole;
    //         var emoteId = socketReaction.Emote.ToString();
    //
    //         if (socketReaction.MessageId != settings.MessageId || socketReaction.User.Value.IsBot || !emoteAndRole.Keys.Contains(emoteId)) return;
    //
    //         var user = socketReaction.User.Value as IGuildUser;
    //
    //         if (isAdded)
    //             user.AddRoleAsync(emoteAndRole[emoteId]);
    //         else
    //             user.RemoveRoleAsync(emoteAndRole[emoteId]);
    //     };
    //
    // private async Task Log(LogMessage log) => Console.WriteLine(log);
}