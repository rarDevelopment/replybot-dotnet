using Microsoft.Extensions.Hosting;

namespace Replybot
{
    public class DiscordBot : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly ILogger _logger;
        private readonly InteractionHandler _interactionHandler;
        private readonly DiscordSettings _discordSettings;

        public DiscordBot(DiscordSocketClient client, InteractionService interactions, ILogger<DiscordBot> logger,
            InteractionHandler interactionHandler, DiscordSettings discordSettings)
        {
            _client = client;
            _interactions = interactions;
            _logger = logger;
            _interactionHandler = interactionHandler;
            _discordSettings = discordSettings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Ready += ClientReady;

            _client.Log += LogAsync;
            _interactions.Log += LogAsync;

            await _interactionHandler.InitializeAsync();

            await _client.LoginAsync(TokenType.Bot, _discordSettings.BotToken);

            await _client.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();
        }

        private async Task ClientReady()
        {
            _logger.LogInformation($"Logged as {_client.CurrentUser}");

            await _interactions.RegisterCommandsGloballyAsync();
        }

        public async Task LogAsync(LogMessage msg)
        {
            var severity = msg.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Trace,
                LogSeverity.Debug => LogLevel.Debug,
                _ => LogLevel.Information
            };

            _logger.Log(severity, msg.Exception, msg.Message);

            await Task.CompletedTask;
        }
    }
}