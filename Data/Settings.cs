using Newtonsoft.Json;

namespace myGreeterBot.Data;

public class Settings
{

    private readonly string JsonPath = Environment.CurrentDirectory + @"\Data\Settings.json";
    
    public dynamic GetSettings()
    {
        string jsonAsString;
        using (StreamReader streamReader = new StreamReader(JsonPath)) 
            jsonAsString = streamReader.ReadToEnd();
        
        return JsonConvert.DeserializeObject<SettingsEntity>(jsonAsString);
    }

    public void SaveSettings(SettingsEntity settings)
    {
        using (StreamWriter streamWriter = File.CreateText(JsonPath))
        {
            var jsonSerializer = JsonSerializer.Create();
            jsonSerializer.Formatting = Formatting.Indented;
            
            jsonSerializer.Serialize(streamWriter, settings);
        }
    }
}

public class SettingsEntity
{
    public string Token;
    public ulong MessageId;
    public string MessageText;
    public ulong MentionChannelId;
    public Dictionary<string, ulong> EmoteAndRole;
}

// {
// "token": "MTA0MjEwMzY5NDYwODU3NjUxMg.G2fNn5.vHYxNGsxpKYtb5tQf03IhnNb2hKc0YRO_qbdzM",
// "messageId": 1044045860889170000,
// "mentionChannelId": 1044044881468850186,
// "emoteAndRole": {
//     "\uD83E\uDD14": 1044049914965012520,
//     "\uD83D\uDD95": 1044049961416917022
// }
// }
