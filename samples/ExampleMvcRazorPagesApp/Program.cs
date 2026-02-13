using Microsoft.Extensions.AI;
using OpenAI;
using SmartComponents.Inference;
using System.Linq;
using System.Numerics.Tensors;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddRepoSharedConfig();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSmartComponents()
    .WithAntiforgeryValidation();

// Note: the StartupKey value is just there so the app will start up. 
builder.Services.AddSingleton(new OpenAIClient(builder.Configuration["AI:OpenAI:Key"] ?? "StartupKey"));
builder.Services.AddChatClient(services =>
{
    var chatClient = new SmartComponentsChatClient(services.GetRequiredService<OpenAIClient>()
        .GetChatClient(builder.Configuration["AI:OpenAI:Chat:ModelId"] ?? "gpt-4o-mini").AsIChatClient());
    return chatClient;
});
builder.Services.AddEmbeddingGenerator(services =>
    services.GetRequiredService<OpenAIClient>().GetEmbeddingClient(builder.Configuration["AI:OpenAI:Embedding:ModelId"] ?? "text-embedding-3-small").AsIEmbeddingGenerator());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Prepare a list of expense categories and corresponding embeddings
var generator = app.Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
var expenseCategories = await GenerateEmbeddingsAsync(generator,
    ["Groceries", "Utilities", "Rent", "Mortgage", "Car Payment", "Car Insurance", "Health Insurance", "Life Insurance", "Home Insurance", "Gas", "Public Transportation", "Dining Out", "Entertainment", "Travel", "Clothing", "Electronics", "Home Improvement", "Gifts", "Charity", "Education", "Childcare", "Pet Care", "Other"]);

app.MapSmartComboBox("/api/suggestions/accounting-categories",
    async (SmartComboBoxRequest request) =>
    {
        var query = request.Query.SearchText;
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<string>();
        var queryEmbedding = (await generator.GenerateAsync(query));
        return FindClosest(queryEmbedding.Vector, expenseCategories);
    });

app.Run();

static async Task<(string Item, ReadOnlyMemory<float> Vector)[]> GenerateEmbeddingsAsync(IEmbeddingGenerator<string, Embedding<float>> generator, string[] items)
{
    try 
    {
        var embeddings = await generator.GenerateAsync(items);
        return items.Zip(embeddings, (item, embedding) => (item, embedding.Vector)).ToArray();  
    }
    catch (Exception)
    {
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
