using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("請提供檔案路徑作為參數。");
            return;
        }

        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.WriteLine("檔案不存在：" + filePath);
            return;
        }

        string userPrompt = await File.ReadAllTextAsync(filePath);

        string? apiKey = Environment.GetEnvironmentVariable("AOAI_API_KEY");
        string? endpoint = Environment.GetEnvironmentVariable("AOAI_API_BASE");
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
        {
            Console.WriteLine("請確保所有環境變數都已設定。");
            return;
        }

        AzureKeyCredential credential = new AzureKeyCredential(apiKey);

        // Initialize the AzureOpenAIClient
        AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);

        // Initialize the ChatClient with the specified deployment name
        ChatClient chatClient = azureClient.GetChatClient("gpt-4o-mini");

        // Create a list of chat messages
        var messages = new List<ChatMessage>
          {
              new SystemChatMessage("Translate the following text into English while preserving the original meaning. Only return the translation result without adding any additional content."),
              new UserChatMessage(userPrompt)
          };


        // Create chat completion options
        var options = new ChatCompletionOptions
        {
            Temperature = (float)0.3,
            MaxOutputTokenCount = 800,
        };

        try
        {
            // ChatCompletion completion1 = await chatClient.CompleteChatAsync(messages, options);

            // Create the chat completion request
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);
            stopwatch.Stop();
            // Console.WriteLine($"{stopwatch.ElapsedMilliseconds}");

            // Print the response
            if (completion.Content != null && completion.Content.Count > 0)
            {
                Console.Write($"{completion.Content[0].Text}");
                // Console.WriteLine($"{completion.Content[0].Kind}: {completion.Content[0].Text}");
            }
            else
            {
                // Console.WriteLine("No response received.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
