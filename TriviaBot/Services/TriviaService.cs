using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FuzzySharp;

namespace TriviaBot.Services
{
    public class TriviaService : ITriviaService
    {
        private static string _lastQuestion = string.Empty;
        private static string _lastAnswer = string.Empty;
        public static int SimilarityThreshold = 73;
        private static int _tries;
        private readonly Lazy<DiscordService> _discordService;
        private readonly IStatsService _statsService;

        public TriviaService(IStatsService statsService, Lazy<DiscordService> discordService)
        {
            _statsService = statsService;
            _discordService = discordService;
            _statsService.LoadStats(); // Load the stats when the TriviaService is created
        }

        public async Task StartTrivia(DiscordChannel channel)
        {
            var random = new Random();
            _lastQuestion = TriviaQuestions.GetRandomQuestion(random);
            _lastAnswer = TriviaQuestions.GetAnswer(_lastQuestion);

            Console.WriteLine($"Sending trivia question: {_lastQuestion}");

            var embed = new DiscordEmbedBuilder
            {
                Title = "Pirate Trivia Question",
                Description = _lastQuestion,
                Color = DiscordColor.Blue,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "To answer, type `!answer <your answer>`" }
            };

            await channel.SendMessageAsync(embed);

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
                        var similarity = ComputeSimilarity(userAnswer, _lastAnswer);

                        // Output the similarity and the threshold to the console
                        Console.WriteLine($"Similarity: {similarity}");
                        Console.WriteLine($"Threshold: {SimilarityThreshold}");

                        // If the similarity is greater than or equal to SimilarityThreshold, consider the answer to be correct
                        if (similarity >= SimilarityThreshold)
                        {
                            Console.WriteLine("Correct answer received");
                            responseMessage = await channel.SendMessageAsync($"Correct! The answer to '{_lastQuestion}' is {_lastAnswer}");
                            _discordService.Value.DiscordClient.MessageCreated -= CheckAnswer; // Unsubscribe from the event
                            _tries = 0; // Reset the number of tries

                            // Update user stats
                            _statsService.UpdateStats(e.Author.Username, 1, 0);
                        }
                        else
                        {
                            Console.WriteLine("Incorrect answer received");
                            _tries++; // Increment the number of tries

                            // Update user stats
                            _statsService.UpdateStats(e.Author.Username, 0, 1);

                            if (_tries >= 3)
                            {
                                responseMessage = await channel.SendMessageAsync($"Sorry, the correct answer to '{_lastQuestion}' is {_lastAnswer}");
                                _discordService.Value.DiscordClient.MessageCreated -= CheckAnswer; // Unsubscribe from the event
                                _tries = 0; // Reset the number of tries
                            }
                            else
                            {
                                responseMessage = await channel.SendMessageAsync("Sorry, that's incorrect. Try again!");

                                // Delete the response after a delay
                                await Task.Delay(TimeSpan.FromSeconds(10));
                                await responseMessage.DeleteAsync();
                            }
                        }

                        _statsService.SaveStats();
                    }
                }
            }

            _discordService.Value.DiscordClient.MessageCreated += CheckAnswer;
        }

        public static int ComputeSimilarity(string s, string t)
        {
            return Fuzz.PartialRatio(s.ToLower(), t.ToLower());
        }
    }
}
