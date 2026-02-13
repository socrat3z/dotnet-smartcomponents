# Using Smart Components with Abstractions

The SmartComponents library has been modernized to be provider-agnostic and more flexible. It now uses `Microsoft.Extensions.AI` abstractions and provides its own interfaces for custom implementations.

## Getting Started

### 1. Register Services

In your `Program.cs` or `Startup.cs`, call `AddSmartComponents()`:

```csharp
builder.Services.AddSmartComponents()
    .WithInferenceBackend(chatClient); // Pass an IChatClient from Microsoft.Extensions.AI
```

### 2. Provider Agnosticism

Since the library uses `IChatClient`, you can use any AI provider that implements this interface. For example, using OpenAI:

```csharp
builder.Services.AddChatClient(services => {
    var client = new OpenAIClient(apiKey);
    return client.GetChatClient("gpt-4o-mini").AsIChatClient();
});

builder.Services.AddSmartComponents(); // Automatically picks up the registered IChatClient
```

### 3. Custom Inference Logic

If you want to customize how prompts are built or how the AI is called, you can provide your own implementation of `ISmartTextAreaInference` or `ISmartPasteInference`.

```csharp
public class MyCustomPasteInference : ISmartPasteInference
{
    public async Task<SmartPasteResponseData> GetFormCompletionsAsync(IChatClient chatClient, SmartPasteRequestData requestData)
    {
        // Your custom logic here
    }
}

// Register it
builder.Services.AddScoped<ISmartPasteInference, MyCustomPasteInference>();
builder.Services.AddSmartComponents(); // Will use your custom implementation
```

### 4. Prompt Customization

Prompts are now loaded via `IPromptTemplateProvider`. The default implementation loads them from embedded resources (now in `.md` format). You can provide a custom `IPromptTemplateProvider` to load prompts from files, database, or other sources.

```csharp
public class FileSystemPromptProvider : IPromptTemplateProvider
{
    public string GetTemplate(string templateName) => File.ReadAllText($"Prompts/{templateName}.md");
    public Task<string> GetTemplateAsync(string templateName) => File.ReadAllTextAsync($"Prompts/{templateName}.md");
}

builder.Services.AddSingleton<IPromptTemplateProvider, FileSystemPromptProvider>();
```

## Smart Components

### SmartTextArea

Provides AI-powered autocomplete suggestions as you type.

**Blazor:**
```razor
<SmartTextArea UserRole="Software Engineer" />
```

**MVC/Razor Pages:**
```html
<smart-textarea user-role="Software Engineer"></smart-textarea>
```

#### Display Options

*   **`data-inline-suggestions`**: Optional boolean (`true` or `false`). By default, Smart TextArea uses inline ghost text on desktop (accepted with Tab) and a floating overlay on touch devices. You can force a specific mode using this attribute on the `<textarea>` element itself.

### SmartPaste

Fills out forms automatically using content from the clipboard. It works by scanning the form for input, select, and textarea elements and identifying them based on labels, names, or IDs.

**Blazor:**
```razor
<form>
    <InputText @bind-Value="name" data-smartpaste-description="Legal full name" />
    <SmartPasteButton />
</form>
```

**MVC/Razor Pages:**
```html
<form id="myForm">
    <input name="Name" data-smartpaste-description="Legal full name" />
    <smart-paste-button></smart-paste-button>
</form>
```

#### Improving Results with Data Attributes

While Smart Paste tries to infer what each field represents automatically, you can improve accuracy using the following attributes:

*   **`data-smartpaste-description`**: Provide a clear, natural-language description of what should go in this field. This is the most effective way to help the AI understand your form. You can also use this on individual radio buttons to describe each option.
*   **Labels**: Smart Paste automatically uses the text from a linked `<label>` as the field description if `data-smartpaste-description` is missing.
*   **Name/ID**: If no explicit description or label is found, the element's `name` or `id` attribute is used as a final fallback.
*   **Excluded Fields**: Fields with `type="hidden"` or those marked as combo boxes (e.g., using `data-autocomplete`) are automatically skipped.

## Advanced Features

### Anti-forgery Validation

To enable anti-forgery validation for the Smart Component endpoints:

```csharp
builder.Services.AddSmartComponents()
    .WithAntiforgeryValidation();
```

Make sure you have `app.UseAntiforgery()` in your request pipeline.
