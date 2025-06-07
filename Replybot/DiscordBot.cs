using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Replybot.Models;
using Replybot.Notifications;

namespace Replybot;

public class DiscordBot(DiscordSocketClient client,
        InteractionService interactionService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DiscordBot> logger,
        InteractionHandler interactionHandler,
        DiscordSettings discordSettings)
    : BackgroundService
{
    private readonly ILogger _logger = logger;
    private readonly TimeSpan _loopWaitTime = TimeSpan.FromSeconds(15);
    private readonly CancellationToken _cancellationToken = new CancellationTokenSource().Token;

    private IMediator Mediator
    {
        get
        {
            var scope = serviceScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IMediator>();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        client.Ready += ClientReady;

        client.Log += LogAsync;
        interactionService.Log += LogAsync;

        await interactionHandler.InitializeAsync();

        SetEvents();

        await client.LoginAsync(TokenType.Bot, discordSettings.BotToken);

        await client.StartAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_loopWaitTime, stoppingToken);
            if (client.ConnectionState != ConnectionState.Disconnected)
            {
                continue;
            }
            await LogAsync(new LogMessage(LogSeverity.Error, "ExecuteAsync", "Attempting to restart bot"));
            await client.StopAsync();
            try
            {
                await client.StartAsync();
            }
            catch (Exception ex)
            {
                await LogAsync(new LogMessage(LogSeverity.Critical, "ExecuteAsync", "Could not restart bot", ex));
            }
        }
    }

    private async Task ClientReady()
    {
        _logger.LogInformation($"Logged as {client.CurrentUser}");

        await interactionService.RegisterCommandsGloballyAsync();
    }

    public void SetEvents()
    {
        client.MessageReceived += msg => Publish(new MessageReceivedNotification(msg));
        client.GuildMemberUpdated += (cachedOldUser, newUser) => Publish(new GuildMemberUpdatedNotification(cachedOldUser, newUser));
        client.JoinedGuild += socketGuild => Publish(new JoinedGuildNotification(socketGuild));
        client.GuildUpdated += (oldGuild, newGuild) => Publish(new GuildUpdatedNotification(oldGuild, newGuild));
        client.LeftGuild += socketGuild => Publish(new LeftGuildNotification(socketGuild));
        client.UserUpdated += (oldUser, newUser) => Publish(new UserUpdatedNotification(oldUser, newUser));
        client.MessageUpdated += (oldMessage, newMessage, channel) => Publish(new MessageUpdatedNotification(oldMessage, newMessage, channel));
        client.MessageDeleted += (message, channel) => Publish(new MessageDeletedNotification(message, channel));
        client.UserLeft += (guild, userWhoLeft) => Publish(new UserLeftNotification(guild, userWhoLeft));
        client.UserBanned += (userWhoWasBanned, guild) => Publish(new UserBannedNotification(userWhoWasBanned, guild));
        client.UserUnbanned += (userWhoWasUnbanned, guild) => Publish(new UserUnbannedNotification(userWhoWasUnbanned, guild));
        client.UserJoined += user => Publish(new UserJoinedNotification(user));
        client.ReactionAdded += (cacheableMessage, cacheableChannel, reaction) => Publish(new ReactionAddedNotification(cacheableMessage, cacheableChannel, reaction));
        client.ChannelUpdated += (oldChannel, newChannel) => Publish(new ChannelUpdatedNotification(oldChannel, newChannel));
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