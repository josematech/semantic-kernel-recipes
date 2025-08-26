using System.ComponentModel;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace FunctionCallingBasics.Plugins;

public class CurrencyPlugin : IDisposable
{
    private readonly HttpClient _httpClient = new();
    private readonly Dictionary<string, decimal> _fallbackRates = new()
    {
        {"USD_EUR", 0.85m},
        {"USD_GBP", 0.73m},
        {"USD_JPY", 110.0m},
        {"EUR_USD", 1.18m},
        {"EUR_GBP", 0.86m},
        {"EUR_JPY", 129.0m},
        {"GBP_USD", 1.37m},
        {"GBP_EUR", 1.16m},
        {"GBP_JPY", 150.0m},
        {"JPY_USD", 0.0091m},
        {"JPY_EUR", 0.0077m},
        {"JPY_GBP", 0.0067m}
    };

    [KernelFunction, Description("Converts an amount from one currency to another using real-time exchange rates")]
    public async Task<string> ConvertCurrencyAsync(
        [Description("The amount to convert")] decimal amount,
        [Description("The source currency code (e.g., USD, EUR, GBP, JPY)")] string fromCurrency,
        [Description("The target currency code (e.g., USD, EUR, GBP, JPY)")] string toCurrency)
    {
        Console.WriteLine($"💱 [{DateTime.Now:HH:mm:ss.fff}] Converting {amount} {fromCurrency} to {toCurrency}...");
        
        try
        {
            var rate = await GetRealTimeExchangeRate(fromCurrency.ToUpper(), toCurrency.ToUpper());
            var convertedAmount = Math.Round(amount * rate, 2);
            var result = $"{amount} {fromCurrency.ToUpper()} = {convertedAmount} {toCurrency.ToUpper()} (Real-time rate: {rate})";
            
            Console.WriteLine($"✅ [{DateTime.Now:HH:mm:ss.fff}] Conversion complete: {fromCurrency} to {toCurrency}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [{DateTime.Now:HH:mm:ss.fff}] Conversion failed: {fromCurrency} to {toCurrency}, using fallback");
            // Fallback to static rates if API fails
            var key = $"{fromCurrency.ToUpper()}_{toCurrency.ToUpper()}";
            if (_fallbackRates.TryGetValue(key, out var fallbackRate))
            {
                var convertedAmount = Math.Round(amount * fallbackRate, 2);
                return $"{amount} {fromCurrency.ToUpper()} = {convertedAmount} {toCurrency.ToUpper()} (Fallback rate: {fallbackRate}) - API Error: {ex.Message}";
            }
            return $"Unable to convert {fromCurrency} to {toCurrency}: {ex.Message}";
        }
    }

    [KernelFunction, Description("Gets the current real-time exchange rate between two currencies")]
    public async Task<string> GetExchangeRateAsync(
        [Description("The base currency code")] string baseCurrency,
        [Description("The target currency code")] string targetCurrency)
    {
        Console.WriteLine($"📊 [{DateTime.Now:HH:mm:ss.fff}] Getting exchange rate: {baseCurrency} to {targetCurrency}...");
        
        try
        {
            var rate = await GetRealTimeExchangeRate(baseCurrency.ToUpper(), targetCurrency.ToUpper());
            var result = $"1 {baseCurrency.ToUpper()} = {rate} {targetCurrency.ToUpper()} (Real-time)";
            
            Console.WriteLine($"✅ [{DateTime.Now:HH:mm:ss.fff}] Exchange rate retrieved: {baseCurrency} to {targetCurrency}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [{DateTime.Now:HH:mm:ss.fff}] Exchange rate failed: {baseCurrency} to {targetCurrency}, using fallback");
            var key = $"{baseCurrency.ToUpper()}_{targetCurrency.ToUpper()}";
            if (_fallbackRates.TryGetValue(key, out var fallbackRate))
            {
                return $"1 {baseCurrency.ToUpper()} = {fallbackRate} {targetCurrency.ToUpper()} (Fallback) - API Error: {ex.Message}";
            }
            return $"Exchange rate not available for {baseCurrency} to {targetCurrency}: {ex.Message}";
        }
    }

    [KernelFunction, Description("Gets information about a specific currency")]
    public async Task<string> GetCurrencyInfoAsync(
        [Description("The currency code to get information about")] string currencyCode)
    {
        var currencyInfo = currencyCode.ToUpper() switch
        {
            "USD" => "United States Dollar - The world's primary reserve currency",
            "EUR" => "Euro - The official currency of the Eurozone",
            "GBP" => "British Pound Sterling - The currency of the United Kingdom",
            "JPY" => "Japanese Yen - The official currency of Japan",
            _ => $"Information not available for currency: {currencyCode}"
        };

        return currencyInfo;
    }

    private async Task<decimal> GetRealTimeExchangeRate(string fromCurrency, string toCurrency)
    {
        // Using exchangerate-api.com (free tier: 1500 requests/month)
        var url = $"https://api.exchangerate-api.com/v4/latest/{fromCurrency}";
        
        var response = await _httpClient.GetStringAsync(url);
        var jsonDocument = JsonDocument.Parse(response);
        var rates = jsonDocument.RootElement.GetProperty("rates");
        
        if (rates.TryGetProperty(toCurrency, out var rateElement))
        {
            return rateElement.GetDecimal();
        }
        
        throw new Exception($"Currency {toCurrency} not found in exchange rates");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
