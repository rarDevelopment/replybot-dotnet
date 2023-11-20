namespace Replybot;

public class SystemChannelPoster(ILogger<DiscordBot> logger)
{
    public async Task PostToGuildSystemChannel(SocketGuild guild, string message, string errorMessage, Type callerType)
    {
        try
        {
            await guild.SystemChannel.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, $"Error Posting to System Channel in {callerType}: {errorMessage} -- {ex.Message}");
        }
    }
}