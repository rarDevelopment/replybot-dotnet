global using Discord;
global using Discord.Interactions;
global using Discord.WebSocket;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
using DiscordDotNetUtilities;
using DiscordDotNetUtilities.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Replybot;
using Serilog;
using System.Reflection;
using Fortnite_API;
using Replybot.BusinessLayer;
using Replybot.DataLayer;
using Replybot.Models;
using Replybot.NotificationHandlers;
using Replybot.ReactionCommands;
using Replybot.ServiceLayer;
using Replybot.TextCommands;
using Replybot.TextCommands.Models;
using TMDbLib.Client;
using FortniteApi = Replybot.ServiceLayer.FortniteApi;

var builder = new HostBuilder();

builder.ConfigureAppConfiguration(options
    => options.AddJsonFile("appsettings.json")
        .AddUserSecrets(Assembly.GetEntryAssembly()!, true)
        .AddEnvironmentVariables())
    .ConfigureHostConfiguration(configHost =>
    {
        configHost.AddEnvironmentVariables(prefix: "DOTNET_");
    });

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"logs/log-{DateTime.Now:dd.MM.yy_HH.mm}.log")
    .CreateLogger();

builder.ConfigureServices((host, services) =>
{
    services.AddLogging(options => options.AddSerilog(loggerConfig, dispose: true));
    services.AddSingleton(new DiscordSocketClient(
        new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMembers |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.GuildMessageReactions |
                             GatewayIntents.MessageContent |
                             GatewayIntents.GuildBans |
                             GatewayIntents.DirectMessages,
            FormatUsersInBidirectionalUnicode = false,
            AlwaysDownloadUsers = true,
            LogGatewayIntentWarnings = false,
            MessageCacheSize = 50
        }));

    var versionSettings = new VersionSettings(host.Configuration["Version:VersionNumber"]!);

    var botSettings = new BotSettings(regexTimeoutMilliseconds: Convert.ToInt32(host.Configuration["Bot:RegexTimeoutMilliseconds"]));

    var discordSettings = new DiscordSettings(botToken: host.Configuration["Discord:BotToken"]!,
        avatarBaseUrl: host.Configuration["Discord:AvatarBaseUrl"]!,
        maxCharacters: Convert.ToInt32(host.Configuration["Discord:MaxCharacters"]));

    var databaseSettings = new DatabaseSettings(
        host.Configuration["Database:Cluster"]!,
        host.Configuration["Database:User"]!,
        host.Configuration["Database:Password"]!,
        host.Configuration["Database:Name"]!
        );

    var howLongToBeatSettings = new HowLongToBeatSettings(host.Configuration["HowLongToBeat:BaseUrl"]!,
        host.Configuration["HowLongToBeat:Referer"]!);

    var theMovieDbSettings = new TheMovieDbSettings(host.Configuration["TheMovieDB:ImdbBaseUrl"]!);

    var dictionarySettings = new DictionarySettings(host.Configuration["FreeDictionary:BaseUrl"]!);
    var blueskySettings = new BlueskySettings(host.Configuration["Bluesky:BaseUrl"]!);

    services.AddSingleton(versionSettings);
    services.AddSingleton(botSettings);
    services.AddSingleton(discordSettings);
    services.AddSingleton(databaseSettings);
    services.AddSingleton(howLongToBeatSettings);
    services.AddSingleton(theMovieDbSettings);
    services.AddSingleton(blueskySettings);

    services.AddScoped<IDiscordFormatter, DiscordFormatter>();
    services.AddScoped<IReplyBusinessLayer, ReplyBusinessLayer>();
    services.AddScoped<IGuildConfigurationBusinessLayer, GuildConfigurationBusinessLayer>();
    services.AddScoped<IReplyDataLayer, ReplyDataLayer>();

    services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
    services.AddSingleton<InteractionHandler>();

    services.AddSingleton<SystemChannelPoster>();
    services.AddSingleton<LogChannelPoster>();
    services.AddSingleton<ExistingMessageEmbedBuilder>();

    services.AddSingleton<MessageReceivedNotificationHandler>();
    services.AddSingleton<UserUpdatedNotificationHandler>();
    services.AddSingleton<GuildMemberUpdatedNotificationHandler>();
    services.AddSingleton<GuildUpdatedNotificationHandler>();

    services.AddSingleton<ITextCommand, HowLongToBeatCommand>();
    services.AddSingleton<ITextCommand, HowLongIsMovieCommand>();
    services.AddSingleton<ITextCommand, DefineWordCommand>();
    services.AddSingleton<ITextCommand, PollCommand>();
    services.AddSingleton<ITextCommand, GetFortniteShopInformationCommand>();
    services.AddSingleton<ITextCommand, GetMemberCountCommand>();
    services.AddSingleton<ITextCommand, GetChannelAgeCommand>();
    services.AddSingleton<ITextCommand, CanIStreamCommand>();
    services.AddSingleton<ITextCommand, GetGuildBannerCommand>();
    services.AddSingleton<ITextCommand, GetAvatarCommand>();
    services.AddSingleton<ITextCommand, GetGuildIconCommand>();
    services.AddSingleton<ITextCommand, YesOrNoCommand>();
    services.AddSingleton<ITextCommand, FlipACoinCommand>();
    services.AddSingleton<ITextCommand, Magic8BallCommand>();
    services.AddSingleton<ITextCommand, SongLinkCommand>();
    services.AddSingleton<ITextCommand, SearchCommand>();
    services.AddSingleton<ITextCommand, VersionCommand>();

    services.AddSingleton<IReactionCommand, FixTwitterCommand>();
    services.AddSingleton<IReactionCommand, FixInstagramCommand>();
    services.AddSingleton<IReactionCommand, FixBlueskyCommand>();

    services.AddSingleton<FixTwitterCommand>();
    services.AddSingleton<FixInstagramCommand>();
    services.AddSingleton<FixBlueskyCommand>();

    services.AddSingleton<HowLongToBeatApi>();
    services.AddSingleton<FreeDictionaryApi>();
    services.AddSingleton<BlueskyApi>();

    services.AddSingleton<FortniteApi>();
    services.AddSingleton(_ => new FortniteApiClient(host.Configuration["FortniteApi:ApiKey"]));

    services.AddSingleton<TheMovieDbApi>();
    services.AddSingleton(_ => new TMDbClient(host.Configuration["TheMovieDB:ApiKey"]));

    services.AddScoped<RoleHelper>();

    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(DiscordBot).GetTypeInfo().Assembly));

    services.AddHostedService<DiscordBot>();

    services.AddHttpClient(HttpClients.HowLongToBeat.ToString(), config =>
    {
        config.BaseAddress = new Uri(howLongToBeatSettings.BaseUrl);
        config.DefaultRequestHeaders.Add("Referer", howLongToBeatSettings.Referer);
        config.DefaultRequestHeaders.Add("Connection", "keep-alive");
        config.DefaultRequestHeaders.Add("Accept", "*/*");
        config.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.29.2");
    });

    services.AddHttpClient(HttpClients.Dictionary.ToString(), config =>
    {
        config.BaseAddress = new Uri(dictionarySettings.BaseUrl);
    });

    services.AddHttpClient(HttpClients.Bluesky.ToString(), config =>
    {
        config.BaseAddress = new Uri(blueskySettings.BaseUrl);
    });
});

var app = builder.Build();

await app.RunAsync();