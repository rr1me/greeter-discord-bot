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
        var chnl = _client.GetChannel(_settingsEntity.MentionChannelId) as IMessageChannel;
        var message = await chnl.GetMessageAsync(_settingsEntity.MessageId);

        var messageText = _settingsEntity.MessageText;
        if (message.Content != messageText)
        {
            message = await chnl.SendMessageAsync(messageText);
            
            _settingsEntity.MessageId = message.Id;
            _settings.SaveSettings(_settingsEntity);
        }
            
        var reactions = message.Reactions;
        CheckReactions(message, reactions);
    }
    
    public async Task CheckReactions(IMessage message, IReadOnlyDictionary<IEmote, ReactionMetadata> reactions)
    {
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