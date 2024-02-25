namespace TriviaBot.Services;

public interface IConfigurationService
{
    string BotToken { get; }
    ulong RoleId { get; }
    ulong WelcomeChannelId { get; }
}