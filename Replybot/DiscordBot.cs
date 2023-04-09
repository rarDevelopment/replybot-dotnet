using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _loopWaitTime = TimeSpan.FromSeconds(15);
    private readonly CancellationToken _cancellationToken;

    public DiscordBot(DiscordSocketClient client,
        InteractionService interactionService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DiscordBot> logger,
        InteractionHandler interactionHandler,
        DiscordSettings discordSettings)
    {
        _client = client;
        _interactionService = interactionService;
        _logger = logger;
        _interactionHandler = interactionHandler;
        _discordSettings = discordSettings;
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
            if (_client.ConnectionState != ConnectionState.Disconnected)
            {
                continue;
            }
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

    private async Task ClientReady()
    {
        _logger.LogInformation($"Logged as {_client.CurrentUser}");

        await _interactionService.RegisterCommandsGloballyAsync();
    }

    public void SetEvents()
    {
        _client.MessageReceived += msg => Publish(new MessageReceivedNotification(msg));
        _client.GuildMemberUpdated += (cachedOldUser, newUser) => Publish(new GuildMemberUpdatedNotification(cachedOldUser, newUser));
        _client.JoinedGuild += (SocketGuild socketGuild) => Publish(new JoinedGuildNotification(socketGuild));
        _client.GuildUpdated += (oldGuild, newGuild) => Publish(new GuildUpdatedNotification(oldGuild, newGuild));
        _client.LeftGuild += (SocketGuild socketGuild) => Publish(new LeftGuildNotification(socketGuild));
        _client.UserUpdated += (oldUser, newUser) => Publish(new UserUpdatedNotification(oldUser, newUser));
        _client.MessageUpdated += (oldMessage, newMessage, channel) => Publish(new MessageUpdatedNotification(oldMessage, newMessage, channel));
        _client.MessageDeleted += (message, channel) => Publish(new MessageDeletedNotification(message, channel));
        _client.UserLeft += (guild, userWhoLeft) => Publish(new UserLeftNotification(guild, userWhoLeft));
        _client.UserBanned += (userWhoWasBanned, guild) => Publish(new UserBannedNotification(userWhoWasBanned, guild));
        _client.UserUnbanned += (userWhoWasUnbanned, guild) => Publish(new UserUnbannedNotification(userWhoWasUnbanned, guild));
        _client.UserJoined += user => Publish(new UserJoinedNotification(user));
    }

    private Task Publish<TEvent>(TEvent @event) where TEvent : INotification
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Mediator.Publish(@event, _cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in {Event}:  {ExceptionMessage}", @event.GetType().Name, ex.Message);
            }
        }, _cancellationToken);
        return Task.CompletedTask;
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