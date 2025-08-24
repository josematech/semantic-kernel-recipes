using Microsoft.SemanticKernel;

namespace FunctionCallingBasics.Filters;

public class SecurityFilter : IFunctionInvocationFilter
{
    private readonly string[] _blockedCurrencies = { "BTC", "ETH", "DOGE", "XRP" };
    private readonly string[] _restrictedCountries = { "NORTH_KOREA", "IRAN", "SYRIA" };

    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"🔒 [{DateTime.Now:HH:mm:ss.fff}] Security filter checking: {context.Function.Name}");

        ValidateCryptocurrencyConversion(context);
        ValidateWeatherAccess(context);
        LogLargeConversions(context);

        Console.WriteLine($"✅ [{DateTime.Now:HH:mm:ss.fff}] Security filter passed: {context.Function.Name}");
        
        await next(context);
    }

    private void ValidateCryptocurrencyConversion(FunctionInvocationContext context)
    {
        if (!context.Function.Name.Contains("Convert")) return;

        if (context.Arguments.TryGetValue("fromCurrency", out var fromCurrency) &&
            _blockedCurrencies.Contains(fromCurrency?.ToString()?.ToUpper()))
        {
            Console.WriteLine($"❌ [{DateTime.Now:HH:mm:ss.fff}] BLOCKED: Cryptocurrency conversion from {fromCurrency} is not allowed");
            throw new UnauthorizedAccessException($"Cryptocurrency conversion from {fromCurrency} is blocked for security reasons.");
        }

        if (context.Arguments.TryGetValue("toCurrency", out var toCurrency) &&
            _blockedCurrencies.Contains(toCurrency?.ToString()?.ToUpper()))
        {
            Console.WriteLine($"❌ [{DateTime.Now:HH:mm:ss.fff}] BLOCKED: Cryptocurrency conversion to {toCurrency} is not allowed");
            throw new UnauthorizedAccessException($"Cryptocurrency conversion to {toCurrency} is blocked for security reasons.");
        }
    }

    private void ValidateWeatherAccess(FunctionInvocationContext context)
    {
        if (!context.Function.Name.Contains("Weather")) return;

        if (context.Arguments.TryGetValue("city", out var city))
        {
            var cityName = city?.ToString()?.ToUpper();
            if (cityName != null && (_restrictedCountries.Any(country => cityName.Contains(country)) ||
                                   cityName.Contains("PYONGYANG") || cityName.Contains("TEHRAN")))
            {
                Console.WriteLine($"❌ [{DateTime.Now:HH:mm:ss.fff}] BLOCKED: Weather access for {city} is restricted");
                throw new UnauthorizedAccessException($"Weather information for {city} is restricted due to security policies.");
            }
        }
    }

    private void LogLargeConversions(FunctionInvocationContext context)
    {
        if (context.Function.Name.Contains("Convert") &&
            context.Arguments.TryGetValue("amount", out var amount) &&
            decimal.TryParse(amount?.ToString(), out var amountValue) &&
            amountValue > 100000)
        {
            Console.WriteLine($"⚠️ [{DateTime.Now:HH:mm:ss.fff}] ALERT: Large conversion detected: {amountValue:C}");
        }
    }
}
