using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Fall2025_Project3_dchanthirath.Services
{
    public class OpenAiService
    {
        private readonly IConfiguration _configuration;
        private readonly ChatClient _chatClient;

        public OpenAiService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Read keys from secrets.json or Azure configuration
            string? apiKey = _configuration["AzureAI:ApiKey"] ?? throw new ArgumentNullException("AzureAI:ApiKey not found.");
            string? apiEndpoint = _configuration["AzureAI:Endpoint"] ?? throw new ArgumentNullException("AzureAI:Endpoint not found.");
            string? deploymentName = _configuration["AzureAI:DeploymentName"] ?? throw new ArgumentNullException("AzureAI:DeploymentName not found.");

            // Set up the official Azure OpenAI client
            Uri apiEndpointUri = new Uri(apiEndpoint);
            ApiKeyCredential apiCredential = new ApiKeyCredential(apiKey);

            _chatClient = new AzureOpenAIClient(apiEndpointUri, apiCredential).GetChatClient(deploymentName);
        }

        private async Task<string> GetAiResponseAsync(string systemPrompt, string userPrompt)
        {
            var messages = new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            // Set options to request a JSON response
            var chatCompletionOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatFormat.JsonObject,
                MaxTokens = 1500,
                Temperature = 0.7f // 0.7 is a good balance of creative and predictable
            };

            // Get the response from the AI
            ClientResult<ChatCompletion> result = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions);

            // Extract the JSON content string from the response
            if (result.Value.Content.FirstOrDefault()?.Text is string content)
            {
                return content;
            }

            throw new Exception("Failed to get a valid JSON response from the AI.");
        }

        public async Task<List<string>> GetMovieReviewsAsync(string movieTitle)
        {
            var systemPrompt = "You are a movie critic. Generate a list of concise, one-sentence reviews for a movie. Your response must be a single JSON object with one key: 'reviews', which contains an array of 10 strings.";
            var userPrompt = $"Generate 10 unique, one-sentence movie reviews for the movie: {movieTitle}.";

            try
            {
                // 1. Get the JSON string from the AI
                var aiJsonContent = await GetAiResponseAsync(systemPrompt, userPrompt);

                // 2. Parse the JSON string
                var reviewsNode = JsonNode.Parse(aiJsonContent);
                var reviews = reviewsNode?["reviews"]?.AsArray();

                return reviews?.Select(r => r.GetValue<string>()).ToList() ?? new List<string> { "No reviews found in AI JSON response." };
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error generating reviews: {ex.Message}" };
            }
        }

        public async Task<List<string>> GetActorTweetsAsync(string actorName)
        {
            var systemPrompt = "You are a social media analyst. Generate a list of 20 realistic, short tweets (like from Twitter) about an actor. Your response must be a single JSON object with one key: 'tweets', which contains an array of 20 strings.";
            var userPrompt = $"Generate 20 unique, short tweets (140-280 characters) about the actor: {actorName}.";

            try
            {
                // 1. Get the JSON string from the AI
                var aiJsonContent = await GetAiResponseAsync(systemPrompt, userPrompt);

                // 2. Parse the JSON string
                var tweetsNode = JsonNode.Parse(aiJsonContent);
                var tweets = tweetsNode?["tweets"]?.AsArray();

                return tweets?.Select(t => t.GetValue<string>()).ToList() ?? new List<string> { "No tweets found in AI JSON response." };
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error generating tweets: {ex.Message}" };
            }
        }
    }
}