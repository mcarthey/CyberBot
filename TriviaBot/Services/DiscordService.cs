using DSharpPlus;
using DSharpPlus.EventArgs;

namespace TriviaBot.Services;

public class DiscordService
{
    private readonly IMessageHandler _messageHandler;

    public DiscordClient DiscordClient { get; }

    public DiscordService(DiscordClient discordClient, IMessageHandler messageHandler)
    {
        DiscordClient = discordClient;
        _messageHandler = messageHandler;

        SetupEventHandlers();
    }

    public async Task StartAsync()
    {
        Console.WriteLine("Connecting to Discord...");
        await DiscordClient.ConnectAsync();
        Console.WriteLine("Connected to Discord");
    }

    private Task ReadyHandler(DiscordClient s, ReadyEventArgs e)
    {
        Console.WriteLine("Bot is ready");
        return Task.CompletedTask;
    }

    private void SetupEventHandlers()
    {
        DiscordClient.MessageCreated += _messageHandler.MessageCreatedHandler;
        DiscordClient.Ready += ReadyHandler;
        DiscordClient.SocketErrored += SocketErroredHandler;
    }

    private Task SocketErroredHandler(DiscordClient s, SocketErrorEventArgs e)
    {
        Console.WriteLine($"Socket error: {e.Exception.Message}");
        return Task.CompletedTask;
    }
}
