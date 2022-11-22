using Discord.Commands;

namespace myGreeterBot.Commands;

public class Commands : ModuleBase<SocketCommandContext>
{
    private readonly BotRunner _botRunner;
    
    public Commands(BotRunner botRunner)
    {
        _botRunner = botRunner;
    }

    [Command("updateSettings")]
    public async Task UpdateSettings()
    {
        await _botRunner.UpdateSettings();
        ReplyAsync("+");
    }
}