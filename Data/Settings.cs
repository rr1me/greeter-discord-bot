using Newtonsoft.Json;

namespace myGreeterBot.Data;

public class Settings
{
    public dynamic GetSettings()
    {
        string jsonAsString;
        using (StreamReader streamReader = new StreamReader(Environment.CurrentDirectory + @"\Data\Settings.json")) 
            jsonAsString = streamReader.ReadToEnd();
        
        return JsonConvert.DeserializeObject<SettingsEntity>(jsonAsString);
    }
}

public class SettingsEntity
{
    public string Token;
    public ulong MessageId;
    public ulong MentionChannelId;
    public Dictionary<string, ulong> EmoteAndRole;
} 