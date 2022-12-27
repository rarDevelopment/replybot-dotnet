namespace Replybot;

public class SystemChannelPoster
{
    private readonly ILogger<DiscordBot> _logger;

    public SystemChannelPoster(ILogger<DiscordBot> logger)
    {
        _logger = logger;
    }

    public async Task PostToGuildSystemChannel(SocketGuild guild, string message, string errorMessage, Type callerType)
    {
        try
        {
            await guild.SystemChannel.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, $"Error Posting to System Channel in {callerType}: {errorMessage} -- {ex.Message}");
        }
    }
}