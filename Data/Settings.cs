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
