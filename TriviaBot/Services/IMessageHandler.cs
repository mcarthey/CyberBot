using DSharpPlus;
using DSharpPlus.EventArgs;

namespace TriviaBot.Services;

public interface IMessageHandler
{
    Task MessageCreatedHandler(DiscordClient s, MessageCreateEventArgs e);
}