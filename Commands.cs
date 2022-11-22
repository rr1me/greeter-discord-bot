using Discord;
using Discord.Commands;

namespace myGreeterBot.Commands;

public class Commands : ModuleBase<SocketCommandContext>
{
    private readonly Miscellaneous _miscellaneous;
    
    public Commands(Miscellaneous miscellaneous)
    {
        _miscellaneous = miscellaneous;
    }

    [Command("updateSettings")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task UpdateSettings()
    {
        await _miscellaneous.UpdateSettings();
        ReplyAsync("Настройки обновлены.");
    }
}