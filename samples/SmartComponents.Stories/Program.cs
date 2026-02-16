using BlazingStory.Components;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using SmartComponents.Abstractions;
using SmartComponents.Inference;
using SmartComponents.Stories.Components.Pages;
using SmartComponents.Stories.Mocks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register Mock Services for Smart Components Stories
// Check for command line flag to enable Real LLM Mode
var useRealLlm = args.Contains("--use-real-llm");
var openRouterApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

if (useRealLlm && !string.IsNullOrEmpty(openRouterApiKey))
{
    // Real LLM Mode
    // Use the OpenRouter API with the specific model
    var openAIClient = new OpenAI.OpenAIClient(new System.ClientModel.ApiKeyCredential(openRouterApiKey), new OpenAI.OpenAIClientOptions { Endpoint = new Uri("https://openrouter.ai/api/v1") });
    
    // Use AsIChatClient() extension method from Microsoft.Extensions.AI (provided by Microsoft.Extensions.AI.OpenAI package)
    var chatClient = openAIClient.GetChatClient("google/gemini-2.0-flash-001").AsIChatClient();
    builder.Services.AddSingleton<IChatClient>(chatClient);
}
else
{
    if (useRealLlm && string.IsNullOrEmpty(openRouterApiKey))
    {
        Console.WriteLine("WARNING: --use-real-llm flag provided but OPENROUTER_API_KEY is missing. Falling back to specific mocks.");
    }

    // Mock Mode
    builder.Services.TryAddScoped<ISmartTranslateInference, MockSmartTranslateInference>();
    builder.Services.TryAddScoped<ISmartSummaryInference, MockSmartSummaryInference>();
    builder.Services.TryAddScoped<ISmartImageInference, MockSmartImageInference>();

    // Minimal mock ChatClient since it's injected but ignored by mocks
    builder.Services.TryAddScoped<IChatClient, MockChatClient>();
}

// Register real implementations for other services if needed, checks for existing registrations via TryAdd
builder.Services.TryAddScoped<ISmartTextAreaInference, SmartTextAreaInference>(); 
builder.Services.TryAddScoped<ISmartPasteInference, SmartPasteInference>(); 

builder.Services.AddSmartComponents();

// Add localizer if needed? SmartComponents use IStringLocalizer? No, mostly pass strings.

var app = builder.Build();

app.MapSmartComboBox("/api/suggestions/sample", (SmartComboBoxRequest request) =>
{
    var query = request.Query.SearchText;
    if (string.IsNullOrWhiteSpace(query)) return ["Apple", "Banana", "Cherry"];
    return new[] { "Apple", "Banana", "Cherry" }.Where(x => x.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<BlazingStoryServerComponent<IndexPage, IFramePage>>()
    .AddInteractiveServerRenderMode();

app.Run();
