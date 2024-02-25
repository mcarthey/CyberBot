using TriviaBot.Models;

namespace TriviaBot.Services;

public interface IStatsService
{
    UserStats GetUserStats(string username);
    void LoadStats();
    void SaveStats();
    void UpdateStats(string username, int correct, int incorrect);
}