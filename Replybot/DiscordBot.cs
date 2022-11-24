using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Replybot.Events;
using Replybot.Models;
using Replybot.Notifications;

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
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _loopWaitTime = TimeSpan.FromSeconds(15);
    private readonly CancellationToken _cancellationToken;

    public DiscordBot(DiscordSocketClient client,
        InteractionService interactionService,
        IServiceScopeFactory serviceScopeFactory,
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
        _serviceScopeFactory = serviceScopeFactory;
        _cancellationToken = new CancellationTokenSource().Token;
    }

    private IMediator Mediator
    {
        get
        {
            var scope = _serviceScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IMediator>();
        }
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
        _client.MessageReceived += OnMessageReceivedEvent;
        _client.GuildMemberUpdated += OnGuildMemberUpdatedEvent;
        _client.GuildUpdated += OnGuildUpdatedEvent;
        _client.UserUpdated += OnUserUpdatedEvent;
    }
    private Task OnMessageReceivedEvent(SocketMessage msg)
    {
        return Mediator.Publish(new MessageReceivedNotification(msg), _cancellationToken);
    }
    private Task OnGuildMemberUpdatedEvent(Cacheable<SocketGuildUser, ulong> cachedOldUser, SocketGuildUser newUser)
    {
        return Mediator.Publish(new GuildMemberUpdatedNotification(cachedOldUser, newUser), _cancellationToken);
    }
    private Task OnGuildUpdatedEvent(SocketGuild oldGuild, SocketGuild newGuild)
    {
        return Mediator.Publish(new GuildUpdatedNotification(oldGuild, newGuild), _cancellationToken);
    }
    private Task OnUserUpdatedEvent(SocketUser oldUser, SocketUser newUser)
    {
        return Mediator.Publish(new UserUpdatedNotification(oldUser, newUser), _cancellationToken);
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