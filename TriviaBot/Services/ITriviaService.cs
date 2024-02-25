using DSharpPlus.Entities;

namespace TriviaBot.Services;

public interface ITriviaService
{
    Task StartTrivia(DiscordChannel channel);
}