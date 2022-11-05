using Microsoft.Extensions.Hosting;
using Replybot.Events;
using Replybot.Models;

namespace Replybot;

public class DiscordBot : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly ILogger _logger;
    private readonly InteractionHandler _interactionHandler;
    private readonly DiscordSettings _discordSettings;
    private readonly MessageReceivedEventHandler _messageReceivedEventHandler;
    private readonly UserUpdatedEventHandler _userUpdatedEventHandler;
    private readonly GuildMemberUpdatedEventHandler _guildMemberUpdatedEventHandler;

    public DiscordBot(DiscordSocketClient client, 
        InteractionService interactionService,
        ILogger<DiscordBot> logger,
        InteractionHandler interactionHandler,
        DiscordSettings discordSettings,
        MessageReceivedEventHandler messageReceivedEventHandler,
        UserUpdatedEventHandler userUpdatedEventHandler,
        GuildMemberUpdatedEventHandler guildMemberUpdatedEventHandler)
    {
        _client = client;
        _interactionService = interactionService;
        _logger = logger;
        _interactionHandler = interactionHandler;
        _discordSettings = discordSettings;
        _messageReceivedEventHandler = messageReceivedEventHandler;
        _userUpdatedEventHandler = userUpdatedEventHandler;
        _guildMemberUpdatedEventHandler = guildMemberUpdatedEventHandler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Ready += ClientReady;

        _client.Log += LogAsync;
        _interactionService.Log += LogAsync;

        await _interactionHandler.InitializeAsync();

        SetEvents();

        await _client.LoginAsync(TokenType.Bot, _discordSettings.BotToken);

        await _client.SetActivityAsync(new Game("everything you say", ActivityType.Watching));

        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
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