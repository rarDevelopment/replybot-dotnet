namespace Replybot;

public class SystemChannelPoster(ILogger<DiscordBot> logger)
{
    public async Task PostMessageToGuildSystemChannel(SocketGuild guild, string message, string errorMessage, Type callerType)
    {
        try
        {
            await guild.SystemChannel.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, $"Error Posting Message to System Channel in {callerType}: {errorMessage} -- {ex.Message}");
        }
    }

    public async Task PostEmbedToGuildSystemChannel(SocketGuild guild, Embed embed, string errorMessage, Type callerType)
    {
        try
        {
            await guild.SystemChannel.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, $"Error Posting Embed to System Channel in {callerType}: {errorMessage} -- {ex.Message}");
        }
    }
}