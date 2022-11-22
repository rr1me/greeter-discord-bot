﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using myGreeterBot.Data;

namespace myGreeterBot;

public class Events
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    
    private readonly Miscellaneous _miscellaneous;
    // private readonly Settings _settings;
    private readonly SettingsEntity _settingsEntity;

    private readonly IServiceProvider _serviceProvider;

    public Events(DiscordSocketClient client, CommandService commands, Miscellaneous miscellaneous, Settings settings, IServiceProvider serviceProvider)
    {
        _client = client;
        _commands = commands;
        _miscellaneous = miscellaneous;
        _settingsEntity = settings._settingsEntity;
        _serviceProvider = serviceProvider;
    }

    public async Task OnMessageReceived(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null) return;
        
        int argPos = 0;

        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author.IsBot)
            return;
        
        var context = new SocketCommandContext(_client, message);

        await _commands.ExecuteAsync(context: context, argPos: argPos, services: _serviceProvider);
    }
    
    public async Task OnReady() => _miscellaneous.CompareSettings();

    public Task OnUserJoined(SocketGuildUser user) => 
        Task.Run(async () => 
        {
            var channel = _client.GetChannel(_settingsEntity.MentionChannelId) as IMessageChannel;
            var msg = await channel.SendMessageAsync(user.Mention);
    
            msg.DeleteAsync();
        });
    
    public Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task> GetReactionMethod(bool isAdded) 
        => async (_, _, socketReaction) =>
        {
            var emoteAndRole = _settingsEntity.EmoteAndRole;
            var emoteId = socketReaction.Emote.ToString();

            if (socketReaction.MessageId != _settingsEntity.MessageId || socketReaction.User.Value.IsBot || !emoteAndRole.Keys.Contains(emoteId)) return;

            var user = socketReaction.User.Value as IGuildUser;

            if (isAdded)
                user.AddRoleAsync(emoteAndRole[emoteId]);
            else
                user.RemoveRoleAsync(emoteAndRole[emoteId]);
        };
    
    public async Task Log(LogMessage log) => Console.WriteLine(log);
}