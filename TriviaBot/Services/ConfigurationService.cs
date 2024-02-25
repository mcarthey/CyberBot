using Microsoft.Extensions.Configuration;

namespace TriviaBot.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string BotToken => _configuration["BotToken"];
    public ulong RoleId => ulong.Parse(_configuration["RoleId"]);
    public ulong WelcomeChannelId => ulong.Parse(_configuration["WelcomeChannelId"]);
}
