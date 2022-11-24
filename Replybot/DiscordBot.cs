using Microsoft.Extensions.Hosting;
using Replybot.Events;
using Replybot.Models;

namespace Replybot;

public class DiscordBot : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly ILogger _logger;
    private readonly InteractionHandler _interactionHandler;
    private readonly DiscordSettings _discordSettings;
    private readonly MessageReceivedEventHandler _messageReceivedEventHandler;
    private readonly UserUpdatedEventHandler _userUpdatedEventHandler;
    private readonly GuildMemberUpdatedEventHandler _guildMemberUpdatedEventHandler;
    private readonly GuildUpdatedEventHandler _guildUpdatedEventHandler;
    private readonly TimeSpan _loopWaitTime = TimeSpan.FromSeconds(15);

    public DiscordBot(DiscordSocketClient client,
        InteractionService interactionService,
        ILogger<DiscordBot> logger,
        InteractionHandler interactionHandler,
        DiscordSettings discordSettings,
        MessageReceivedEventHandler messageReceivedEventHandler,
        UserUpdatedEventHandler userUpdatedEventHandler,
        GuildMemberUpdatedEventHandler guildMemberUpdatedEventHandler,
        GuildUpdatedEventHandler guildUpdatedEventHandler)
    {
        _client = client;
        _interactionService = interactionService;
        _logger = logger;
        _interactionHandler = interactionHandler;
        _discordSettings = discordSettings;
        _messageReceivedEventHandler = messageReceivedEventHandler;
        _userUpdatedEventHandler = userUpdatedEventHandler;
        _guildMemberUpdatedEventHandler = guildMemberUpdatedEventHandler;
        _guildUpdatedEventHandler = guildUpdatedEventHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Ready += ClientReady;

        _client.Log += LogAsync;
        _interactionService.Log += LogAsync;

        await _interactionHandler.InitializeAsync();

        SetEvents();

        await _client.LoginAsync(TokenType.Bot, _discordSettings.BotToken);

        await _client.SetActivityAsync(new Game("everything you say", ActivityType.Watching));

        await _client.StartAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_loopWaitTime, stoppingToken);
            if (_client.ConnectionState == ConnectionState.Disconnected)
            {
                await LogAsync(new LogMessage(LogSeverity.Error, "ExecuteAsync", "Attempting to restart bot"));
                await _client.StopAsync();
                try
                {
                    await _client.StartAsync();
                }
                catch (Exception ex)
                {
                    await LogAsync(new LogMessage(LogSeverity.Critical, "ExecuteAsync", "Could not restart bot", ex));
                }
            }
        }
    }

    private async Task ClientReady()
    {
        _logger.LogInformation($"Logged as {_client.CurrentUser}");

        await _interactionService.RegisterCommandsGloballyAsync();
    }

    public void SetEvents()
    {
        _client.MessageReceived += _messageReceivedEventHandler.HandleEvent;
        _client.UserUpdated += _userUpdatedEventHandler.HandleEvent;
        _client.GuildMemberUpdated += _guildMemberUpdatedEventHandler.HandleEvent;
        _client.GuildUpdated += _guildUpdatedEventHandler.HandleEvent;
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