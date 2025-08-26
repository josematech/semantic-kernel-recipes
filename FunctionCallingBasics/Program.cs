using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

var modelName = config["modelName"];

// Other demos (commented out)

await FunctionCallingBasics.FunctionCallingUtils.WeatherChatDemo(modelName);
//await FunctionCallingBasics.FunctionCallingUtils.ParallelismDemoComparison(modelName);
//await FunctionCallingBasics.FunctionCallingUtils.CurrencyConversionDemo(modelName);
//await FunctionCallingBasics.FunctionCallingUtils.SecurityFilterDemo(modelName);
//await FunctionCallingBasics.FunctionCallingUtils.ComplexSecurityDemo(modelName);
