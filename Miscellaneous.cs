using Discord;
using Discord.WebSocket;
using myGreeterBot.Data;

namespace myGreeterBot;

public class Miscellaneous
{

    private readonly DiscordSocketClient _client;
    private readonly Settings _settings;
    private SettingsEntity _settingsEntity;

    public Miscellaneous(DiscordSocketClient client, Settings settings)
    {
        _client = client;
        _settings = settings;
        _settingsEntity = settings._settingsEntity;
    }

    public async Task UpdateSettings()
    {
        _settingsEntity = _settings.GetSettings();
        CompareSettings();
    }
    
    public async Task CompareSettings()
    {
        var channel = _client.GetChannel(_settingsEntity.MentionChannelId) as IMessageChannel;
        var messageTask = channel.GetMessageAsync(_settingsEntity.MessageId);

        IMessage message;
        if (messageTask.IsFaulted)
            message = await SendNewMessage(channel);
        else
        {
            try
            {
                message = await CheckMessage(await messageTask);
            }
            catch
            {
                message = await SendNewMessage(channel);
            }
        }

        CheckReactions(message);
    }

    private async Task<IMessage> CheckMessage(IMessage message)
    {
        var channel = message.Channel;
        var botId = _client.CurrentUser.Id;

        if (message.Author.Id != botId)
            return await SendNewMessage(channel);

        var messageText = _settingsEntity.MessageText;
        if (message.Content != messageText)
            return await channel.ModifyMessageAsync(message.Id, properties => properties.Content = messageText);

        return message;
    }
    
    private async Task<IMessage> SendNewMessage(IMessageChannel channel)
    {
        var messageText = _settingsEntity.MessageText;
        var message = await channel.SendMessageAsync(messageText);
            
        _settingsEntity.MessageId = message.Id;
        _settings.SaveSettings(_settingsEntity);
        return message;
    }
    
    private async Task CheckReactions(IMessage message)
    {
        var reactions = message.Reactions;
        var formalReactions = _settingsEntity.EmoteAndRole.Keys;

        if (reactions.Count > formalReactions.Count)
        {
            foreach (var actualReaction in reactions.Keys)
            {
                if (!formalReactions.Contains(actualReaction.ToString()))
                    await message.RemoveAllReactionsForEmoteAsync(actualReaction);
            }
        }
        foreach (var emoteUnicode in formalReactions)
        {
            var reactionKeys = reactions.Keys.Select(x=>x.ToString());
            
            if (reactions.Count == 0 || !reactionKeys.Contains(emoteUnicode))
                await message.AddReactionAsync(Emote.Parse(emoteUnicode));
        }
    }
}