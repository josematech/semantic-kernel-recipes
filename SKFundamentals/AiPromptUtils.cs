using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToImage;

namespace SKFundamentals
{
    public static class AiPromptUtils
    {
        private static Kernel CreateKernel(string modelName)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
            return Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: modelName, apiKey: apiKey).Build();
        }

        public static async Task BasicPromptInteraction(string modelName)
        {
            var kernel = CreateKernel(modelName);

            var response = string.Empty;

            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                if (response != null) Console.WriteLine(await kernel.InvokePromptAsync(response));
            }
        }

        public static async Task ChatWithHistory(string modelName)
        {
            var kernel = CreateKernel(modelName);

            string? response = string.Empty;

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();

            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }
        }

        public static async Task TravelStreamingDemo(string modelName)
        {
            var kernel = CreateKernel(modelName);

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = new("You are a travel expert who recommends unique destinations and unforgettable experiences.");

            chatHistory.AddAssistantMessage("Hello! I'm your travel assistant. Where would you like to go?");
            var message = chatHistory.Last();
            Console.WriteLine($"{message.Role}: {message.Content}");

            chatHistory.AddUserMessage("I want a different kind of vacation in Europe.");
            message = chatHistory.Last();
            Console.WriteLine($"{message.Role}: {message.Content}");

            await foreach (StreamingChatMessageContent chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                Console.Write(chatUpdate.Content);
            }
        }

        public static async Task StartupIdeaSettingsDemo(string modelName)
        {
            var kernel = CreateKernel(modelName);

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            KernelArguments arguments = new(new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 0 });
            Console.WriteLine("Temperature 0:");
            Console.WriteLine(await kernel.InvokePromptAsync(
                "Suggest an innovative idea for a tech startup focused on improving online education.", arguments));

            arguments = new(new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 1 });
            Console.WriteLine("Temperature 1:");
            Console.WriteLine(await kernel.InvokePromptAsync(
                "Suggest an innovative idea for a tech startup focused on improving online education.", arguments));
        }

        [Experimental("SKEXP0001")]
        public static async Task CreateTravelLoungeImage(string modelName)
        {
            var kernel = CreateKernel(modelName);
            ITextToImageService imageService = kernel.GetRequiredService<ITextToImageService>();

            const string prompt = $@"Imagine a vibrant travel agency lounge inspired by the spirit of global adventure. The space features sleek, modern furniture with pops of color representing different continents, large world maps adorning the walls, and interactive digital displays showcasing breathtaking destinations.
Sunlight streams through panoramic windows, illuminating travel memorabilia, vintage suitcases, and shelves filled with guidebooks and souvenirs. The atmosphere is energetic yet welcoming, encouraging visitors to dream, plan, and embark on their next unforgettable journey.
Cozy nooks with plush seating invite guests to relax and discuss travel ideas, while a central coffee bar offers international treats and beverages. The overall design celebrates exploration, curiosity, and the joy of discovering new places.";

            var image = await imageService.GenerateImageAsync(prompt, 896, 512);
            Console.WriteLine("Image URL: " + image);
        }
    }
}
