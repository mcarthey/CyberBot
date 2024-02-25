using Newtonsoft.Json;
using TriviaBot.Models;

namespace TriviaBot.Services;

public class StatsService : IStatsService
{
    private static Dictionary<string, UserStats> userStats = new();
    private static readonly object statsLock = new();

    private readonly Lazy<DiscordService> _discordService;

    public StatsService(Lazy<DiscordService> discordService)
    {
        _discordService = discordService;
    }

    public UserStats GetUserStats(string username)
    {
        lock (statsLock)
        {
            if (userStats.ContainsKey(username))
            {
                return userStats[username];
            }

            return null;
        }
    }

    public void LoadStats()
    {
        if (File.Exists("stats.json"))
        {
            var json = File.ReadAllText("stats.json");
            userStats = JsonConvert.DeserializeObject<Dictionary<string, UserStats>>(json);
        }
    }

    public void SaveStats()
    {
        lock (statsLock)
        {
            var json = JsonConvert.SerializeObject(userStats, Formatting.Indented);
            File.WriteAllText("stats.json", json);
        }
    }

    public void UpdateStats(string username, int correct, int incorrect)
    {
        lock (statsLock)
        {
            if (userStats.ContainsKey(username))
            {
                userStats[username].Correct += correct;
                userStats[username].Incorrect += incorrect;
            }
            else
            {
                userStats[username] = new UserStats {Correct = correct, Incorrect = incorrect};
            }

            SaveStats();
        }
    }
}
