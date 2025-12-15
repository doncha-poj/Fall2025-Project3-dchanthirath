using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace Fall2025_Project3_dchanthirath.Services
{
    public class OpenAiService
    {
        private readonly ChatClient _client;

        public OpenAiService(IConfiguration config)
        {
            var endpoint = config["AzureAI:Endpoint"];
            var apiKey = config["AzureAI:ApiKey"];
            var deploymentName = config["AzureAI:DeploymentName"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("AzureAI Endpoint or API Key is missing in configuration.");
            }

            var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            _client = azureClient.GetChatClient(deploymentName);
        }

        public async Task<List<string>> GetMovieReviewsAsync(string movieTitle)
        {
            var systemPrompt = "You are a movie critic assistant. " +
                               "Provide a JSON response containing a list of 10 fictional reviews for the specified movie. " +
                               "Return ONLY a JSON object with a single property 'reviews' which is an array of strings. " +
                               "Example: { \"reviews\": [\"Great movie!\", \"Did not like it.\"] }";

            var userPrompt = $"Generate 10 reviews for the movie '{movieTitle}'.";

            var options = new ChatCompletionOptions
            {
                // We use simple JSON mode to avoid "schema" errors
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            try
            {
                ChatCompletion completion = await _client.CompleteChatAsync(
                    [
                        new SystemChatMessage(systemPrompt),
                        new UserChatMessage(userPrompt)
                    ],
                    options);

                var jsonResponse = completion.Content[0].Text;

                // Parse the simple JSON object
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                if (root.TryGetProperty("reviews", out var reviewsElement) && reviewsElement.ValueKind == JsonValueKind.Array)
                {
                    return reviewsElement.Deserialize<List<string>>() ?? new List<string>();
                }

                return new List<string> { "Error parsing AI response." };
            }
            catch (Exception ex)
            {
                // Fallback in case of API errors
                return new List<string> { $"Error retrieving reviews: {ex.Message}" };
            }
        }

        public async Task<List<string>> GetActorTweetsAsync(string actorName)
        {
            var systemPrompt = "You are a social media simulator. " +
                               "Provide a JSON response containing a list of 20 fictional tweets about the specified actor. " +
                               "Return ONLY a JSON object with a single property 'tweets' which is an array of strings. " +
                               "Example: { \"tweets\": [\"Love this actor! #fan\", \"Can't wait for the next movie.\"] }";

            var userPrompt = $"Generate 20 tweets about '{actorName}'.";

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            try
            {
                ChatCompletion completion = await _client.CompleteChatAsync(
                    [
                        new SystemChatMessage(systemPrompt),
                        new UserChatMessage(userPrompt)
                    ],
                    options);

                var jsonResponse = completion.Content[0].Text;

                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                if (root.TryGetProperty("tweets", out var tweetsElement) && tweetsElement.ValueKind == JsonValueKind.Array)
                {
                    return tweetsElement.Deserialize<List<string>>() ?? new List<string>();
                }

                return new List<string> { "Error parsing AI response." };
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error retrieving tweets: {ex.Message}" };
            }
        }
    }
}