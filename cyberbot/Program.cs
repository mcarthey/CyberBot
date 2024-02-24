using System.Net.NetworkInformation;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class Program
{
    public static IConfiguration Configuration { get; set; }

    private static readonly string BotToken = Configuration["BotToken"];
    private static readonly ulong WelcomeChannelId = ulong.Parse(Configuration["WelcomeChannelId"]);
    private static readonly ulong RoleId = ulong.Parse(Configuration["RoleId"]);

    private static DiscordClient discord;

    public class UserStats
    {
        public int Correct { get; set; }
        public int Incorrect { get; set; }
    }

    private static Dictionary<string, UserStats> userStats = new();

    private static string lastQuestion = string.Empty;
    private static string lastAnswer = string.Empty;
    private static int tries;
    public static int similarityThreshold = 73;
    private static readonly object statsLock = new object();

    public static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();

        MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static async Task MainAsync(string[] args)
    {
        discord = new DiscordClient(new DiscordConfiguration
        {
            Token = BotToken,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        });

        LoadStats();
        SetupEventHandlers();

        Console.WriteLine("Connecting to Discord...");
        await discord.ConnectAsync();
        Console.WriteLine("Connected to Discord");
        await Task.Delay(-1);
    }

    private static async Task MessageCreatedHandler(DiscordClient s, MessageCreateEventArgs e)
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

    private static async Task HandleStatsCommand(MessageCreateEventArgs e)
    {
        if (userStats.ContainsKey(e.Author.Username))
        {
            var stats = userStats[e.Author.Username];
            int totalQuestions = stats.Correct + stats.Incorrect;
            double percentageCorrect = Math.Round((double)stats.Correct / totalQuestions * 100, 2);

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
                Color = DiscordColor.Blue,
            };

            embed.AddField("Correct Answers", stats.Correct.ToString(), true);
            embed.AddField("Incorrect Answers", stats.Incorrect.ToString(), true);
            embed.AddField("Success Rate", $"{percentageCorrect}%", true);
            embed.AddField("Title", title, false);

            await e.Channel.SendMessageAsync(embed: embed);
        }
        else
        {
            await e.Channel.SendMessageAsync($"{e.Author.Username}, you have not answered any questions yet.");
        }
    }


    private static async Task HandleTriviaCommand(MessageCreateEventArgs e)
    {
        Console.WriteLine("Trivia command received");
        await e.Message.RespondAsync("Starting trivia game...");
        await StartTrivia(e.Channel);
        await e.Message.DeleteAsync(); // Delete the command message
    }

    private static async Task HandleDisconnectCommand(MessageCreateEventArgs e)
    {
        Console.WriteLine("Disconnect command received");
        await e.Message.RespondAsync("Disconnecting...");
        await discord.DisconnectAsync();
        await e.Message.DeleteAsync(); // Delete the command message
    }

    private static Task ReadyHandler(DiscordClient s, ReadyEventArgs e)
    {
        Console.WriteLine("Bot is ready");
        return Task.CompletedTask;
    }

    private static void SetupEventHandlers()
    {
        discord.MessageCreated += MessageCreatedHandler;
        discord.Ready += ReadyHandler;
        discord.SocketErrored += SocketErroredHandler;
    }

    private static Task SocketErroredHandler(DiscordClient s, SocketErrorEventArgs e)
    {
        Console.WriteLine($"Socket error: {e.Exception.Message}");
        return Task.CompletedTask;
    }

    private static async Task StartTrivia(DiscordChannel channel)
    {
        var random = new Random();
        lastQuestion = TriviaQuestions.GetRandomQuestion(random);
        lastAnswer = TriviaQuestions.GetAnswer(lastQuestion);

        Console.WriteLine($"Sending trivia question: {lastQuestion}");

        var embed = new DiscordEmbedBuilder
        {
            Title = "Pirate Trivia Question",
            Description = lastQuestion,
            Color = DiscordColor.Blue,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "To answer, type `!answer <your answer>`" }
        };

        await channel.SendMessageAsync(embed: embed);

        async Task CheckAnswer(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (!e.Author.IsBot && e.Channel == channel)
            {
                if (e.Message.Content.StartsWith("!answer"))
                {
                    var userAnswer = e.Message.Content.Substring(8); // Remove "!answer " from the message

                    Console.WriteLine($"Received answer from {e.Author.Username}: {userAnswer}");

                    DiscordMessage responseMessage;

                    // Calculate the similarity between the user's answer and the correct answer
                    int similarity = ComputeSimilarity(userAnswer, lastAnswer);

                    // Output the similarity and the threshold to the console
                    Console.WriteLine($"Similarity: {similarity}");
                    Console.WriteLine($"Threshold: {similarityThreshold}");

                    // If the similarity is greater than or equal to similarityThreshold, consider the answer to be correct
                    if (similarity >= similarityThreshold)
                    {
                        Console.WriteLine("Correct answer received");
                        responseMessage = await channel.SendMessageAsync($"Correct! The answer to '{lastQuestion}' is {lastAnswer}");
                        discord.MessageCreated -= CheckAnswer; // Unsubscribe from the event
                        tries = 0; // Reset the number of tries

                        // Update user stats
                        if (userStats.ContainsKey(e.Author.Username))
                        {
                            userStats[e.Author.Username].Correct++;
                        }
                        else
                        {
                            userStats[e.Author.Username] = new UserStats { Correct = 1, Incorrect = 0 };
                        }
                    }
                    else
                    {
                        Console.WriteLine("Incorrect answer received");
                        tries++; // Increment the number of tries

                        // Update user stats
                        if (userStats.ContainsKey(e.Author.Username))
                        {
                            userStats[e.Author.Username].Incorrect++;
                        }
                        else
                        {
                            userStats[e.Author.Username] = new UserStats { Correct = 0, Incorrect = 1 };
                        }

                        if (tries >= 3)
                        {
                            responseMessage = await channel.SendMessageAsync($"Sorry, the correct answer to '{lastQuestion}' is {lastAnswer}");
                            discord.MessageCreated -= CheckAnswer; // Unsubscribe from the event
                            tries = 0; // Reset the number of tries
                        }
                        else
                        {
                            responseMessage = await channel.SendMessageAsync($"Sorry, that's incorrect. Try again!");

                            // Delete the response after a delay
                            await Task.Delay(TimeSpan.FromSeconds(10));
                            await responseMessage.DeleteAsync();
                        }
                    }

                    SaveStats();
                }
            }
        }

        discord.MessageCreated += CheckAnswer;
    }

    public static int ComputeSimilarity(string s, string t)
    {
        return FuzzySharp.Fuzz.PartialRatio(s.ToLower(), t.ToLower());
    }

    public static void SaveStats()
    {
        lock (statsLock)
        {
            var json = JsonConvert.SerializeObject(userStats, Formatting.Indented);
            File.WriteAllText("stats.json", json);
        }
    }

    public static void LoadStats()
    {
        if (File.Exists("stats.json"))
        {
            var json = File.ReadAllText("stats.json");
            userStats = JsonConvert.DeserializeObject<Dictionary<string, UserStats>>(json);
        }
    }

    public static void UpdateStats(string username, int correct, int incorrect)
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
                userStats[username] = new UserStats { Correct = correct, Incorrect = incorrect };
            }

            SaveStats();
        }
    }
}
