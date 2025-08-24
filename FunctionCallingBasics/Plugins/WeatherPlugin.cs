using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace FunctionCallingBasics.Plugins;

public class WeatherPlugin
{
    private readonly HttpClient _http = new();

    [KernelFunction, Description("Gets the current weather for a city")]
    public async Task<string> GetWeatherAsync(
        [Description("City name")] string city)
    {
        Console.WriteLine($"🌤️ [{DateTime.Now:HH:mm:ss.fff}] Getting weather for {city}...");
        
        var url = $"https://wttr.in/{city}?format=3";
        var result = await _http.GetStringAsync(url);
        
        Console.WriteLine($"✅ [{DateTime.Now:HH:mm:ss.fff}] Weather retrieved for {city}");
        return result;
    }
}