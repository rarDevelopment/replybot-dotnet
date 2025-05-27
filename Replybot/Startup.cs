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
using IGDB;
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
using NodaTime;

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
    .Filter.ByExcluding(logEvent =>
        logEvent.Properties.TryGetValue("SourceContext", out var sourceContext) &&
        sourceContext.ToString().Contains("System.Net.Http.HttpClient") &&
        logEvent.Level == Serilog.Events.LogEventLevel.Information)
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
                             GatewayIntents.DirectMessages |
                             GatewayIntents.GuildVoiceStates,
            FormatUsersInBidirectionalUnicode = false,
            AlwaysDownloadUsers = true,
            LogGatewayIntentWarnings = false,
            MessageCacheSize = 50
        }));

    var versionSettings = new VersionSettings(host.Configuration["Version:VersionNumber"]!);

    var botSettings = new BotSettings(Convert.ToInt32(host.Configuration["Bot:RegexTimeoutTicks"]));

    var discordSettings = new DiscordSettings(host.Configuration["Discord:BotToken"]!,
        host.Configuration["Discord:AvatarBaseUrl"]!,
        Convert.ToInt32(host.Configuration["Discord:MaxCharacters"]),
        host.Configuration["Discord:BaseUrl"]!);

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
    var siteIgnoreListSettings = new SiteIgnoreListSettings(host.Configuration["SiteIgnoreList:Url"]!);
    var countryConfigListSettings = new CountryConfigListSettings(host.Configuration["CountryConfigList:Url"]!);
    var defaultRepliesSettings = new DefaultRepliesSettings(host.Configuration["DefaultReplies:Url"]!);
    var internetGameDatabaseSettings = new InternetGameDatabaseSettings(host.Configuration["InternetGameDatabase:ClientId"]!,
        host.Configuration["InternetGameDatabase:ClientSecret"]!);
    var websiteApiSettings = new WebsiteApiSettings(host.Configuration["WebsiteApi:BaseUrl"]!);
    var applicationEmojiSettings = new ApplicationEmojiSettings(
        host.Configuration["ApplicationEmojis:FixTweet"]!,
        host.Configuration["ApplicationEmojis:FixInstagram"]!,
        host.Configuration["ApplicationEmojis:FixTikTok"]!,
        host.Configuration["ApplicationEmojis:FixBluesky"]!,
        host.Configuration["ApplicationEmojis:FixReddit"]!,
        host.Configuration["ApplicationEmojis:FixThreads"]!,
        host.Configuration["ApplicationEmojis:Slowpoke"]!
        );

    IClock clock = SystemClock.Instance;
    services.AddTransient(_ => clock);

    services.AddSingleton(versionSettings);
    services.AddSingleton(botSettings);
    services.AddSingleton(discordSettings);
    services.AddSingleton(databaseSettings);
    services.AddSingleton(howLongToBeatSettings);
    services.AddSingleton(theMovieDbSettings);
    services.AddSingleton(blueskySettings);
    services.AddSingleton(siteIgnoreListSettings);
    services.AddSingleton(internetGameDatabaseSettings);
    services.AddSingleton(countryConfigListSettings);
    services.AddSingleton(defaultRepliesSettings);
    services.AddSingleton(applicationEmojiSettings);
    services.AddSingleton(websiteApiSettings);

    services.AddSingleton<IDiscordFormatter, DiscordFormatter>();
    services.AddSingleton<IReplyBusinessLayer, ReplyBusinessLayer>();
    services.AddSingleton<IGuildConfigurationBusinessLayer, GuildConfigurationBusinessLayer>();
    services.AddSingleton<IReplyDataLayer, ReplyDataLayer>();
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
    services.AddSingleton<ITextCommand, GetFortniteShopInformationCommand>();
    services.AddSingleton<ITextCommand, GetFortniteMapLocationCommand>();
    services.AddSingleton<ITextCommand, GetFortniteStatsCommand>();
    services.AddSingleton<ITextCommand, GetMemberCountCommand>();
    services.AddSingleton<ITextCommand, GetChannelAgeCommand>();
    services.AddSingleton<ITextCommand, EmoteCommand>();
    services.AddSingleton<ITextCommand, GetStickerCommand>();
    services.AddSingleton<ITextCommand, CanIStreamCommand>();
    services.AddSingleton<ITextCommand, GetGuildBannerCommand>();
    services.AddSingleton<ITextCommand, GetAvatarCommand>();
    services.AddSingleton<ITextCommand, GetGuildIconCommand>();
    services.AddSingleton<ITextCommand, YesOrNoCommand>();
    services.AddSingleton<ITextCommand, FlipACoinCommand>();
    services.AddSingleton<ITextCommand, Magic8BallCommand>();
    services.AddSingleton<ITextCommand, SongLinkCommand>();
    services.AddSingleton<ITextCommand, ChooseCommand>();
    services.AddSingleton<ITextCommand, GameSearchCommand>();
    services.AddSingleton<ITextCommand, SearchCommand>();
    services.AddSingleton<ITextCommand, VersionCommand>();

    services.AddSingleton<IReactionCommand, FixTwitterCommand>();
    services.AddSingleton<IReactionCommand, FixInstagramCommand>();
    services.AddSingleton<IReactionCommand, FixBlueskyCommand>();
    services.AddSingleton<IReactionCommand, FixTikTokCommand>();
    services.AddSingleton<IReactionCommand, FixRedditCommand>();
    services.AddSingleton<IReactionCommand, FixThreadsCommand>();

    services.AddSingleton<HowLongToBeatApi>();
    services.AddSingleton<FreeDictionaryApi>();
    services.AddSingleton<BlueskyApi>();
    services.AddSingleton<SiteIgnoreService>();
    services.AddSingleton<DefaultRepliesService>();
    services.AddSingleton<CountryConfigService>();
    services.AddSingleton<InternetGameDatabaseApi>();
    services.AddSingleton<DefaultRepliesService>();

    services.AddSingleton<FortniteApi>();
    services.AddTransient(_ => new FortniteApiClient(host.Configuration["FortniteApi:ApiKey"]));

    services.AddTransient<TheMovieDbApi>();
    services.AddTransient(_ => new TMDbClient(host.Configuration["TheMovieDB:ApiKey"]));

    services.AddTransient(_ => new IGDBClient(internetGameDatabaseSettings.ClientId, internetGameDatabaseSettings.ClientSecret));

    services.AddSingleton<RoleHelper>();

    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(DiscordBot).GetTypeInfo().Assembly));

    services.AddHostedService<DiscordBot>();

    services.AddHttpClient(HttpClients.HowLongToBeat.ToString(), config =>
    {
        config.BaseAddress = new Uri(howLongToBeatSettings.BaseUrl);
        config.DefaultRequestHeaders.Add("Referer", howLongToBeatSettings.Referer);
        config.DefaultRequestHeaders.Add("Connection", "keep-alive");
        config.DefaultRequestHeaders.Add("Accept", "*/*");
        config.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");
    });

    services.AddHttpClient(HttpClients.Dictionary.ToString(), config =>
    {
        config.BaseAddress = new Uri(dictionarySettings.BaseUrl);
    });

    services.AddHttpClient(HttpClients.Bluesky.ToString(), config =>
    {
        config.BaseAddress = new Uri(blueskySettings.BaseUrl);
    });

    services.AddHttpClient(HttpClients.SiteIgnoreList.ToString(), config =>
    {
        config.BaseAddress = new Uri(siteIgnoreListSettings.Url);
    });

    services.AddHttpClient(HttpClients.CountryConfigList.ToString(), config =>
    {
        config.BaseAddress = new Uri(countryConfigListSettings.Url);
    });

    services.AddHttpClient(HttpClients.DefaultReplies.ToString(), config =>
    {
        config.BaseAddress = new Uri(defaultRepliesSettings.Url);
    });

    services.AddHttpClient(HttpClients.WebsiteApi.ToString(), config =>
    {
        config.BaseAddress = new Uri(websiteApiSettings.BaseUrl);
    });
});

var app = builder.Build();

await app.RunAsync();
