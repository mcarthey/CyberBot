using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace TriviaBot.Services;

public class MessageHandler : IMessageHandler
{
    private readonly Lazy<DiscordService> _discordService;
    private readonly IStatsService _statsService;
    private readonly ITriviaService _triviaService;

    public MessageHandler(IStatsService statsService, ITriviaService triviaService, Lazy<DiscordService> discordService)
    {
        _statsService = statsService;
        _triviaService = triviaService;
        _discordService = discordService;
    }

    public async Task MessageCreatedHandler(DiscordClient s, MessageCreateEventArgs e)
    {
        if (e.Message.Attachments.Count > 0)
        {
            Console.WriteLine($"Message received from {e.Author.Username} with {e.Message.Attachments.Count} attachment(s)");
        }
        else if (!string.IsNullOrEmpty(e.Message.Content))
        {
            Console.WriteLine($"Message received from {e.Author.Username}: {e.Message.Content}");

            if (e.Message.Content.ToLower().StartsWith("!trivia"))
            {
                await HandleTriviaCommand(e);
            }
            else if (e.Message.Content.ToLower().StartsWith("!stats"))
            {
                await HandleStatsCommand(e);
            }
            else if (e.Message.Content.ToLower() == "!disconnect")
            {
                await HandleDisconnectCommand(e);
            }
        }
        else if (e.Message.Embeds.Count > 0)
        {
            // Handle messages with embeds
            foreach (var embed in e.Message.Embeds)
            {
                Console.WriteLine($"Embed received from {e.Author.Username}: {embed.Title}");
            }
        }
    }

    private async Task HandleDisconnectCommand(MessageCreateEventArgs e)
    {
        Console.WriteLine("Disconnect command received");
        await e.Message.RespondAsync("Disconnecting...");
        await _discordService.Value.DiscordClient.DisconnectAsync();
        await e.Message.DeleteAsync(); // Delete the command message
    }

    private async Task HandleStatsCommand(MessageCreateEventArgs e)
    {
        var stats = _statsService.GetUserStats(e.Author.Username);
        if (stats != null)
        {
            var totalQuestions = stats.Correct + stats.Incorrect;
            var percentageCorrect = Math.Round((double) stats.Correct / totalQuestions * 100, 2);

            string title;
            if (percentageCorrect >= 90)
            {
                title = "🏆 Trivia Master";
            }
            else if (percentageCorrect >= 75)
            {
                title = "🥇 Trivia Pro";
            }
            else if (percentageCorrect >= 50)
            {
                title = "🥈 Trivia Enthusiast";
            }
            else if (percentageCorrect >= 25)
            {
                title = "🥉 Trivia Novice";
            }
            else
            {
                title = "🎓 Trivia Newbie";
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{e.Author.Username}'s Trivia Stats",
                Color = DiscordColor.Blue
            };

            embed.AddField("Correct Answers", stats.Correct.ToString(), true);
            embed.AddField("Incorrect Answers", stats.Incorrect.ToString(), true);
            embed.AddField("Success Rate", $"{percentageCorrect}%", true);
            embed.AddField("Title", title);

            await e.Channel.SendMessageAsync(embed);
        }
        else
        {
            await e.Channel.SendMessageAsync($"{e.Author.Username}, you have not answered any questions yet.");
        }
    }

    private async Task HandleTriviaCommand(MessageCreateEventArgs e)
    {
        Console.WriteLine("Trivia command received");
        await e.Message.RespondAsync("Starting trivia game...");
        await _triviaService.StartTrivia(e.Channel);
        await e.Message.DeleteAsync(); // Delete the command message
    }
}
