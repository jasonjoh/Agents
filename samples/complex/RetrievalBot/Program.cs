// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Storage;
using Microsoft.SemanticKernel;
using RetrievalBot;

var builder = WebApplication.CreateBuilder(args);

// Pretty sure this is done for you in CreateBuilder.
// TODO: Test to confirm.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Register Semantic Kernel
builder.Services.AddKernel();

// Register the AI service of your choice. AzureOpenAI and OpenAI are demonstrated...
if (builder.Configuration.GetSection("AIServices").GetValue<bool>("UseAzureOpenAI"))
{
    var azureOpenAiSettings = builder.Configuration.GetSection("AIServices:AzureOpenAI");
    var deploymentName = azureOpenAiSettings.GetValue<string>("DeploymentName") ??
        throw new ArgumentException("DeploymentName is not set in the configuration.");
    var endpoint = azureOpenAiSettings.GetValue<string>("Endpoint") ??
        throw new ArgumentException("Endpoint is not set in the configuration.");
    var apiKey = azureOpenAiSettings.GetValue<string>("ApiKey") ??
        throw new ArgumentException("ApiKey is not set in the configuration.");

    builder.Services.AddAzureOpenAIChatCompletion(
        deploymentName: deploymentName,
        endpoint: endpoint,
        apiKey: apiKey);
}
else
{
    var openAiSettings = builder.Configuration.GetSection("AIServices:OpenAI");
    var modelId = openAiSettings.GetValue<string>("ModelId") ??
        throw new ArgumentException("ModelId is not set in the configuration.");

    var openAiApiKey = openAiSettings.GetValue<string>("ApiKey") ??
        throw new ArgumentException("ApiKey is not set in the configuration.");

    builder.Services.AddOpenAIChatCompletion(
        modelId: modelId,
        apiKey: openAiApiKey);
}

// Add AspNet token validation
builder.Services.AddAgentAspNetAuthentication(builder.Configuration);

// Add AgentApplicationOptions from config.
builder.AddAgentApplicationOptions();

// Add basic bot functionality
builder.AddAgent<Retrieval>();

builder.Services.AddSingleton<IStorage>(new MemoryStorage());
builder.Services.AddSingleton<ConversationState>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => "Microsoft Agents SDK Sample");
    app.UseDeveloperExceptionPage();
    app.MapControllers().AllowAnonymous();
}
else
{
    app.MapControllers();
}

app.Run();
