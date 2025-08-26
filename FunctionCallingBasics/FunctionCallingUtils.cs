using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using FunctionCallingBasics.Plugins;
using FunctionCallingBasics.Filters;
using Microsoft.SemanticKernel.Plugins.Core;

namespace FunctionCallingBasics;

public static class FunctionCallingUtils
{
    public static async Task WeatherChatDemo(string modelName)
    {
        var kernel = CreateKernel(modelName);
        kernel.ImportPluginFromType<WeatherPlugin>();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        ChatHistory chatHistory = [];
        OpenAIPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        string? response = string.Empty;
        while (response != "quit")
        {
            Console.WriteLine("Enter your message:");
            response = Console.ReadLine();
            if (response != null) chatHistory.AddUserMessage(response);
            var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            Console.WriteLine(assistantMessage);
            chatHistory.Add(assistantMessage);
        }
    }

    public static async Task ParallelismDemoComparison(string modelName)
    {
        Console.WriteLine("\n=== PARALLELISM COMPARISON DEMO ===\n");
        
        // sequential demo
        await SequentialFunctionCallsDemo(modelName);
        
        Console.WriteLine("\n" + new string('-', 60) + "\n");
        
        // Parallel demo
        await ParallelFunctionCallsDemo(modelName);
    }

    private static async Task SequentialFunctionCallsDemo(string modelName)
    {
        var kernel = CreateKernel(modelName);
        kernel.ImportPluginFromType<CurrencyPlugin>();
        kernel.ImportPluginFromType<WeatherPlugin>();

        Console.WriteLine("--- SEQUENTIAL FUNCTION CALLS DEMO ---");
        Console.WriteLine("Forcing sequential execution by asking one thing at a time...\n");

        var startTime = DateTime.Now;
        
        // Sequential queries
        var query1 = "Convert 1000 USD to EUR";
        var query2 = "Convert 1000 USD to GBP"; 
        var query3 = "Convert 1000 USD to JPY";
        var query4 = "Get weather for London";
        var query5 = "Get weather for Paris";

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        var arguments = new KernelArguments(settings);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting sequential calls...");
        
        var result1 = await kernel.InvokePromptAsync(query1, arguments);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Completed: {query1}");
        Console.WriteLine($"Result: {result1}\n");
        
        var result2 = await kernel.InvokePromptAsync(query2, arguments);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Completed: {query2}");
        Console.WriteLine($"Result: {result2}\n");
        
        var result3 = await kernel.InvokePromptAsync(query3, arguments);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Completed: {query3}");
        Console.WriteLine($"Result: {result3}\n");
        
        var result4 = await kernel.InvokePromptAsync(query4, arguments);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Completed: {query4}");
        Console.WriteLine($"Result: {result4}\n");
        
        var result5 = await kernel.InvokePromptAsync(query5, arguments);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Completed: {query5}");
        Console.WriteLine($"Result: {result5}\n");

        var endTime = DateTime.Now;
        var duration = endTime - startTime;

        Console.WriteLine($"📊 SEQUENTIAL EXECUTION TIME: {duration.TotalSeconds:F2} seconds");
    }

    private static async Task ParallelFunctionCallsDemo(string modelName)
    {
        var kernel = CreateKernel(modelName);
        kernel.ImportPluginFromType<CurrencyPlugin>();
        kernel.ImportPluginFromType<WeatherPlugin>();

        Console.WriteLine("--- PARALLEL FUNCTION CALLS DEMO ---");
        Console.WriteLine("Asking for multiple operations in a single prompt...\n");

        var startTime = DateTime.Now;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting parallel function calls...");

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var arguments = new KernelArguments(settings);

        var parallelQuery = """
        Please perform these operations simultaneously:
        1. Convert 1000 USD to EUR
        2. Convert 1000 USD to GBP  
        3. Convert 1000 USD to JPY
        4. Get current weather for London
        5. Get current weather for Paris
        
        Execute all these tasks and provide a summary of the results.
        """;

        var result = await kernel.InvokePromptAsync(parallelQuery, arguments);
        
        var endTime = DateTime.Now;
        var duration = endTime - startTime;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] All parallel calls completed!");

        Console.WriteLine($"\n🚀 PARALLEL EXECUTION TIME: {duration.TotalSeconds:F2} seconds");
        Console.WriteLine($"\n--- PARALLEL RESULTS ---");
        Console.WriteLine($"{result}");
    }

    public static async Task CurrencyConversionDemo(string modelName)
    {
        var kernel = CreateKernel(modelName);
        kernel.ImportPluginFromType<CurrencyPlugin>();

        Console.WriteLine("\n--- Currency Conversion with Parallel Function Calls Demo ---");
        Console.WriteLine("Note: Using real-time exchange rates with automatic parallel calls");

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var arguments = new KernelArguments(settings);

        var conversionQuery = """
        Convert 500 USD to EUR, GBP, and JPY using current exchange rates. 
        Also get the current exchange rates for each conversion.
        Present the results in a clear format and mention if real-time or fallback rates are used.
        """;

        Console.WriteLine("Converting currencies with parallel function calls...\n");

        var result = await kernel.InvokePromptAsync(conversionQuery, arguments);

        Console.WriteLine(result);
    }

    public static async Task SecurityFilterDemo(string modelName)
    {
        Console.WriteLine("\n=== SECURITY FILTER DEMO ===\n");
        
        var kernel = CreateKernel(modelName);
        kernel.ImportPluginFromType<CurrencyPlugin>();
        kernel.ImportPluginFromType<WeatherPlugin>();
        
        // Add security filter to the kernel
        kernel.FunctionInvocationFilters.Add(new SecurityFilter());

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        var arguments = new KernelArguments(settings);

        Console.WriteLine("Testing allowed operations...\n");
        
        try
        {
            var allowedQuery = "Convert 500 USD to EUR";
            Console.WriteLine($"Query: {allowedQuery}");
            var result1 = await kernel.InvokePromptAsync(allowedQuery, arguments);
            Console.WriteLine($"Result: {result1}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}\n");
        }

        try
        {
            var weatherQuery = "Get weather for Madrid";
            Console.WriteLine($"Query: {weatherQuery}");
            var result2 = await kernel.InvokePromptAsync(weatherQuery, arguments);
            Console.WriteLine($"Result: {result2}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}\n");
        }

        Console.WriteLine("Testing BLOCKED operations...\n");

        try
        {
            var cryptoQuery = "Convert 1000 USD to BTC";
            Console.WriteLine($"Query: {cryptoQuery}");
            var result3 = await kernel.InvokePromptAsync(cryptoQuery, arguments);
            Console.WriteLine($"Result: {result3}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚫 BLOCKED: {ex.Message}\n");
        }

        try
        {
            var restrictedWeatherQuery = "Get weather for Pyongyang";
            Console.WriteLine($"Query: {restrictedWeatherQuery}");
            var result4 = await kernel.InvokePromptAsync(restrictedWeatherQuery, arguments);
            Console.WriteLine($"Result: {result4}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚫 BLOCKED: {ex.Message}\n");
        }

        try
        {
            var largeAmountQuery = "Convert 500000 USD to EUR";
            Console.WriteLine($"Query: {largeAmountQuery}");
            var result5 = await kernel.InvokePromptAsync(largeAmountQuery, arguments);
            Console.WriteLine($"Result: {result5}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}\n");
        }

        Console.WriteLine("=== Security Filter Demo Complete ===");
    }

    public static async Task ComplexSecurityDemo(string modelName)
    {
        Console.WriteLine("\n=== COMPLEX SECURITY SCENARIO DEMO ===\n");
        
        var kernel = CreateKernel(modelName);
        kernel.ImportPluginFromType<CurrencyPlugin>();
        kernel.ImportPluginFromType<WeatherPlugin>();
        kernel.FunctionInvocationFilters.Add(new SecurityFilter());

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        var arguments = new KernelArguments(settings);

        var complexQuery = """
        I need help with the following financial operations:
        1. Convert 1000 USD to EUR
        2. Convert 500 USD to BTC (this should be blocked)
        3. Convert 2000 EUR to GBP
        4. Get weather for London
        5. Get weather for Tehran (this should be blocked)
        6. Convert 750000 USD to JPY (this should trigger an alert)
        
        Please process all these requests.
        """;

        Console.WriteLine($"Complex Query:\n{complexQuery}\n");
        Console.WriteLine("Processing...\n");

        try
        {
            var result = await kernel.InvokePromptAsync(complexQuery, arguments);
            Console.WriteLine($"Final Result:\n{result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Operation stopped due to security violation: {ex.Message}");
        }

        Console.WriteLine("\n=== Complex Security Demo Complete ===");
    }

    private static Kernel CreateKernel(string modelName)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
        return Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: modelName, apiKey: apiKey)
            .Build();
    }
}
