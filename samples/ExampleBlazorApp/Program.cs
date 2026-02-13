using ExampleBlazorApp.Components;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Linq;
using System.Numerics.Tensors;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddRepoSharedConfig();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSmartComponents()
    .WithAntiforgeryValidation();

// Configure AI backend - supports both OpenRouter and OpenAI
var openRouterKey = builder.Configuration["AI:OpenRouter:Key"];
var openAiKey = builder.Configuration["AI:OpenAI:Key"];

Console.WriteLine("===== AI Configuration Debug =====");
Console.WriteLine($"OpenRouter Key: {(string.IsNullOrEmpty(openRouterKey) ? "NOT SET" : $"***{openRouterKey.Substring(Math.Max(0, openRouterKey.Length - 4))}")}");
Console.WriteLine($"OpenAI Key: {(string.IsNullOrEmpty(openAiKey) ? "NOT SET" : $"***{openAiKey.Substring(Math.Max(0, openAiKey.Length - 4))}")}");

if (!string.IsNullOrEmpty(openRouterKey))
{
    // Use OpenRouter configuration
    var chatModelId = builder.Configuration["AI:OpenRouter:Chat:ModelId"] ?? "meta-llama/llama-3.3-70b-instruct";
    var embeddingModelId = builder.Configuration["AI:OpenRouter:Embedding:ModelId"] ?? "openai/text-embedding-3-small";
    var siteUrl = builder.Configuration["AI:OpenRouter:SiteUrl"] ?? "http://localhost";
    var siteTitle = builder.Configuration["AI:OpenRouter:SiteTitle"] ?? "SmartComponents Demo";

    Console.WriteLine($"Using OpenRouter with chat model: {chatModelId}");
    Console.WriteLine($"Embedding model: {embeddingModelId}");
    Console.WriteLine($"Site: {siteTitle} ({siteUrl})");

    var openRouterOptions = new OpenAIClientOptions
    {
        Endpoint = new Uri("https://openrouter.ai/api/v1")
    };

    // Add OpenRouter-specific headers (required by OpenRouter)
    openRouterOptions.AddPolicy(new OpenRouterHeaderPolicy(siteUrl, siteTitle), System.ClientModel.Primitives.PipelinePosition.PerCall);

    var openRouterClient = new OpenAIClient(new ApiKeyCredential(openRouterKey), openRouterOptions);
    
    builder.Services.AddChatClient(services =>
        new SmartComponents.Inference.SmartComponentsChatClient(
            openRouterClient.GetChatClient(chatModelId).AsIChatClient()));
    
    builder.Services.AddEmbeddingGenerator(services =>
        openRouterClient.GetEmbeddingClient(embeddingModelId).AsIEmbeddingGenerator());
}
else if (!string.IsNullOrEmpty(openAiKey))
{
    // Use OpenAI configuration
    var chatModelId = builder.Configuration["AI:OpenAI:Chat:ModelId"] ?? "gpt-4o-mini";
    var embeddingModelId = builder.Configuration["AI:OpenAI:Embedding:ModelId"] ?? "text-embedding-3-small";
    
    var openAiClient = new OpenAIClient(new ApiKeyCredential(openAiKey));
    
    builder.Services.AddChatClient(services =>
        new SmartComponents.Inference.SmartComponentsChatClient(
            openAiClient.GetChatClient(chatModelId).AsIChatClient()));
    
    builder.Services.AddEmbeddingGenerator(services =>
        openAiClient.GetEmbeddingClient(embeddingModelId).AsIEmbeddingGenerator());
}
else
{
    // No API key configured - use a dummy client that will fail with a helpful error
    Console.WriteLine("WARNING: No AI API key configured. SmartComponents features will not work.");
    Console.WriteLine("Please set AI:OpenRouter:Key or AI:OpenAI:Key in your configuration.");
    Console.WriteLine("See .env.sample or GETTING_STARTED.md for setup instructions.");
    
    var dummyClient = new OpenAIClient(new ApiKeyCredential("CONFIGURE_YOUR_API_KEY"));
    builder.Services.AddChatClient(services =>
        new SmartComponents.Inference.SmartComponentsChatClient(
            dummyClient.GetChatClient("gpt-4o-mini").AsIChatClient()));
    
    builder.Services.AddEmbeddingGenerator(services =>
        dummyClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator());
}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Precompute embeddings
var generator = app.Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
var expenseCategories = await GenerateEmbeddingsAsync(generator, 
    ["Groceries", "Utilities", "Rent", "Mortgage", "Car Payment", "Car Insurance", "Health Insurance", "Life Insurance", "Home Insurance", "Gas", "Public Transportation", "Dining Out", "Entertainment", "Travel", "Clothing", "Electronics", "Home Improvement", "Gifts", "Charity", "Education", "Childcare", "Pet Care", "Other"]);
var issueLabels = await GenerateEmbeddingsAsync(generator,
    ["Bug", "Docs", "Enhancement", "Question", "UI (Android)", "UI (iOS)", "UI (Windows)", "UI (Mac)", "Performance", "Security", "Authentication", "Accessibility"]);

app.MapSmartComboBox("/api/suggestions/expense-category",
    async (SmartComboBoxRequest request) =>
    {
        var query = request.Query.SearchText;
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<string>();
        var queryEmbedding = (await generator.GenerateAsync(query));
        return FindClosest(queryEmbedding.Vector, expenseCategories);
    });

app.MapSmartComboBox("/api/suggestions/issue-label",
    async (SmartComboBoxRequest request) =>
    {
        var query = request.Query.SearchText;
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<string>();
        var queryEmbedding = (await generator.GenerateAsync(query));
        return FindClosest(queryEmbedding.Vector, issueLabels);
    });

app.Run();

static async Task<(string Item, ReadOnlyMemory<float> Vector)[]> GenerateEmbeddingsAsync(IEmbeddingGenerator<string, Embedding<float>> generator, string[] items)
{
    // In a real app, you might want to cache these or load from a DB
    try
    {
        var embeddings = await generator.GenerateAsync(items);
        return items.Zip(embeddings, (item, embedding) => (item, embedding.Vector)).ToArray();
    }
    catch (Exception)
    {
        // Fallback for demo if OpenAI is not configured or fails
        return [];
    }
}

static string[] FindClosest(ReadOnlyMemory<float> queryVector, (string Item, ReadOnlyMemory<float> Vector)[] candidates)
{
    if (candidates.Length == 0) return [];
    
    return candidates
        .Select(c => (c.Item, Similarity: TensorPrimitives.CosineSimilarity(c.Vector.Span, queryVector.Span)))
        .OrderByDescending(x => x.Similarity)
        .Take(5)
        .Select(x => x.Item)
        .ToArray();
}

// OpenRouter requires HTTP-Referer and X-Title headers
sealed class OpenRouterHeaderPolicy(string? siteUrl, string? siteTitle) : System.ClientModel.Primitives.PipelinePolicy
{
    public override void Process(System.ClientModel.Primitives.PipelineMessage message, IReadOnlyList<System.ClientModel.Primitives.PipelinePolicy> pipeline, int currentIndex)
    {
        AddHeaders(message);
        ProcessNext(message, pipeline, currentIndex);
    }

    public override async ValueTask ProcessAsync(System.ClientModel.Primitives.PipelineMessage message, IReadOnlyList<System.ClientModel.Primitives.PipelinePolicy> pipeline, int currentIndex)
    {
        AddHeaders(message);
        await ProcessNextAsync(message, pipeline, currentIndex).ConfigureAwait(false);
    }

    private void AddHeaders(System.ClientModel.Primitives.PipelineMessage message)
    {
        Console.WriteLine($"[OpenRouter] Adding headers - Referer: {siteUrl}, Title: {siteTitle}");
        if (!string.IsNullOrEmpty(siteUrl)) message.Request.Headers.Set("HTTP-Referer", siteUrl);
        if (!string.IsNullOrEmpty(siteTitle)) message.Request.Headers.Set("X-Title", siteTitle);
    }
}

