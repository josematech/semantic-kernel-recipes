using Microsoft.Extensions.Configuration;
using SKFundamentals;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

var modelName = config["modelName"];
//await AiPromptUtils.BasicPromptInteraction(modelName);
//await AiPromptUtils.ChatWithHistory(modelName);
//await AiPromptUtils.TravelStreamingDemo(modelName);
//await AiPromptUtils.StartupIdeaSettingsDemo(modelName);
#pragma warning disable SKEXP0001
var imageModelName = config["imageModelName"];
await AiPromptUtils.CreateTravelLoungeImage(imageModelName);
#pragma warning restore SKEXP0001
return;
