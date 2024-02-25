using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TriviaBot.Services;

namespace TriviaBot;

public class Program
{
    public static IConfiguration Configuration { get; set; }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<DiscordService>();
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddSingleton<IStatsService, StatsService>();
                services.AddSingleton<ITriviaService, TriviaService>();
                services.AddSingleton<IMessageHandler, MessageHandler>();

                services.AddSingleton<DiscordConfiguration>(serviceProvider =>
                {
                    var configService = serviceProvider.GetRequiredService<IConfigurationService>();
                    var config = new DiscordConfiguration
                    {
                        Token = configService.BotToken,
                        TokenType = TokenType.Bot,
                        Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
                    };
                    return config;
                });

                services.AddSingleton<DiscordClient>(serviceProvider =>
                {
                    var config = serviceProvider.GetRequiredService<DiscordConfiguration>();
                    return new DiscordClient(config);
                });

                // Add this line to register a Lazy<DiscordService>
                services.AddTransient(provider => new Lazy<DiscordService>(() => provider.GetRequiredService<DiscordService>()));
            });
    }

    public static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true);

        Configuration = builder.Build();

        MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static async Task MainAsync(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // Get the DiscordService from the DI container and start it
        var discordService = host.Services.GetRequiredService<DiscordService>();
        await discordService.StartAsync();

        // Run the host
        await host.RunAsync();
    }
}