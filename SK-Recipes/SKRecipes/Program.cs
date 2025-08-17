using Microsoft.Extensions.Configuration;
using SKRecipes;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

var modelName = config["modelName"];
await AiPromptUtils.BasicPromptInteraction(modelName);
//await AIPromptUtils.ChatWithHistory(modelName);
//await AIPromptUtils.TravelStreamingDemo(modelName);
//await AIPromptUtils.StartupIdeaSettingsDemo(modelName);
//await AIPromptUtils.CreateTravelLoungeImage(modelName);
return;
