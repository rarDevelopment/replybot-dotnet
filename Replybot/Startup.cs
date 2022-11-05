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
using Replybot.BusinessLayer;
using Replybot.Commands;
using Replybot.DataLayer;
using Replybot.Events;
using Replybot.Models;
using Replybot.ServiceLayer;


var builder = new HostBuilder();

builder.ConfigureAppConfiguration(options
    => options.AddJsonFile("appsettings.json")
        .AddUserSecrets(Assembly.GetEntryAssembly(), true)
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
            GatewayIntents = GatewayIntents.All,
            FormatUsersInBidirectionalUnicode = false,
            AlwaysDownloadUsers = true,
            LogGatewayIntentWarnings = false
        }));

    var discordSettings = new DiscordSettings
    {
        BotToken = host.Configuration["Discord:BotToken"],
        AvatarBaseUrl = host.Configuration["Discord:AvatarBaseUrl"]
    };

    var databaseSettings = new DatabaseSettings
    {
        Cluster = host.Configuration["Database:Cluster"],
        User = host.Configuration["Database:User"],
        Password = host.Configuration["Database:Password"],
        Name = host.Configuration["Database:Name"],
    };

    var howLongToBeatSettings = new HowLongToBeatSettings
    {
        BaseUrl = host.Configuration["HowLongToBeat:BaseUrl"],
        Referer = host.Configuration["HowLongToBeat:Referer"]
    };

    services.AddSingleton(discordSettings);
    services.AddSingleton(databaseSettings);
    services.AddSingleton(howLongToBeatSettings);

    services.AddScoped<IDiscordFormatter, DiscordFormatter>();
    services.AddScoped<IResponseBusinessLayer, ResponseBusinessLayer>();
    services.AddScoped<IGuildConfigurationBusinessLayer, GuildConfigurationBusinessLayer>();
    services.AddScoped<IResponseDataLayer, ResponseDataLayer>();

    services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
    services.AddSingleton<InteractionHandler>();
    
    services.AddSingleton<KeywordHandler>();

    services.AddSingleton<MessageReceivedEventHandler>();
    services.AddSingleton<UserUpdatedEventHandler>();
    services.AddSingleton<GuildMemberUpdatedEventHandler>();

    services.AddSingleton<HowLongToBeatCommand>();
    services.AddSingleton<HowLongToBeatApi>();

    services.AddHostedService<DiscordBot>();

    services.AddHttpClient(HttpClients.HowLongToBeat.ToString(), config =>
    {
        config.BaseAddress = new Uri(howLongToBeatSettings.BaseUrl);
        config.DefaultRequestHeaders.Add("Referer", howLongToBeatSettings.Referer);
        config.DefaultRequestHeaders.Add("Connection", "keep-alive");
        config.DefaultRequestHeaders.Add("Accept", "*/*");
        config.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.29.2");
    });
});

var app = builder.Build();

await app.RunAsync();