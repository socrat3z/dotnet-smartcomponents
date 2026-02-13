# SmartComponents Modernization Plan

## 1. Executive Summary & Findings
**Current Status**: The library has partially adopted modern .NET AI abstractions (`Microsoft.Extensions.AI`), but retains significant technical debt from its experimental "local-first" origins. The current architecture couples the UI components too tightly to specific implementation details (like ONNX-based local embeddings and hardcoded prompts), limiting its flexibility with broader ecosystems like OpenRouter or hosted LLMs.

**Strategic Goal**: Transform the library into a **Universal AI UI Kit** for .NET. It should be model-agnostic, provider-agnostic (OpenRouter, OpenAI, Ollama, Azure), and focused purely on the *UI interaction patterns*, delegating the "Intelligence" entirely to `Microsoft.Extensions.AI` abstractions.

### Key Findings
1.  **Hardcoded Prompts**: Prompts are embedded directly in C# code (e.g., `SmartPasteInference.cs`), making them impossible to modify without recompilation and hard to adapt for different models (e.g., Llama 3 vs. GPT-4).
2.  **Legacy "LocalEmbeddings"**: The `SmartComponents.LocalEmbeddings` project imposes a heavy dependency on `BertOnnx` and rigid file-system expectations, which contradicts the goal of using generic API-based embeddings (OpenAI/OpenRouter).
3.  **Tight Coupling**: The inference logic mixes *prompt engineering* with *client execution*.
4.  **Missing Configuration**: There is no modernized configuration layer to easily swap between OpenRouter, Azure, or Local endpoints without code changes.

---

## 2. Modernization Roadmap

### Phase 1: Architectural Cleanup (The "Purge")
**Objective**: Remove rigid local-only dependencies and standardize on abstractions.

1.  **Deprecate/Remove `SmartComponents.LocalEmbeddings`**:
    *   **Action**: Remove the `SmartComponents.LocalEmbeddings` project entirely or move it to a separate "contrib" repository.
    *   **Replacement**: The core library will depend *only* on `Microsoft.Extensions.AI.Abstractions.IEmbeddingGenerator<string, Embedding<float>>`. This allows users to plug in *any* embedding provider (OpenAI, local Ollama, etc.) via dependency injection.

2.  **Standardize on `IChatClient`**:
    *   Ensure all inference services (`SmartPasteInference`, `SmartTextAreaInference`) accept `IChatClient` via DI.
    *   Remove any assumption that the backend is running locally.

### Phase 2: Prompt Management & Engineering
**Objective**: Decouple prompts from code to allow easier updates and multi-model support.

1.  **Introduce `IPromptTemplateProvider`**:
    *   Create an interface to load prompts.
    *   **Implementation**: Load prompts from embedded resources (`.md` or `.txt` files) or configuration.
    *   **Benefit**: Users can override default prompts in `appsettings.json` if a specific model needs different instructions.

2.  **Externalize Prompts**:
    *   Move the hardcoded strings in `SmartPasteInference.cs` and `SmartTextAreaInference.cs` into:
        *   `src/SmartComponents.Inference/Prompts/SmartPaste/System.md`
        *   `src/SmartComponents.Inference/Prompts/SmartPaste/User.md`

3.  **Structured Output Handling**:
    *   The current `SmartPaste` relies on the model returning raw JSON "on faith".
    *   **Upgrade**: Update the prompt strategy to use robust JSON schema definition or `response_format` features where supported, or robust "json repair" utilities for smaller models.

### Phase 3: OpenRouter & Universal Compatibility
**Objective**: Ensure "Zero Config" compatibility with OpenRouter.

1.  **Configuration Helpers**:
    *   Add extension methods for easy OpenRouter setup.
    *   *Note*: OpenRouter is just an OpenAI-compatible endpoint. We don't need a custom SDK, just proper configuration documentation for `Microsoft.Extensions.AI.OpenAI`.

2.  **Model Agnostic Headers**:
    *   Ensure the `IChatClient` pipeline can easily inject `HTTP Referer` and `X-Title` headers (required/recommended by OpenRouter) without dirtying the component logic.

---

## 3. Proposed Project Structure

This structure separates concerns: **Core** (Contracts/Abstractions), **Inference** (Logic + Prompts), and **UI** (Blazor/MVC).

```text
/src
  /SmartComponents.Abstractions
    - ISmartPasteInference.cs
    - ISmartTextAreaInference.cs
    - /Models (Requests/Responses)

  /SmartComponents.Inference
    - /Prompts (Markdown files/Resources)
      - SmartPaste.system.md
      - SmartTextArea.user.md
    - /Services
      - SmartPasteService.cs (Implements logic using IChatClient)
      - SmartTextAreaService.cs
    - PromptTemplateProvider.cs

  /SmartComponents.AspNetCore
    - /Components (Blazor UI)
    - /TagHelpers (MVC/Razor UI)
    - ServiceCollectionExtensions.cs (DI Setup)

  /SmartComponents.Extensions.OpenRouter (Optional/Doc-only)
    - Setup helpers for OpenRouter specifics
```

## 4. Implementation Details

### Step 1: Defines Prompts as Resources
Instead of C# strings, use a structured prompt loader.

```csharp
public class SmartPasteInference(IChatClient chatClient, IPromptTemplateProvider prompts)
{
    public async Task<SmartPasteResponseData> GetCompletionsAsync(...)
    {
        var systemPrompt = await prompts.GetTemplateAsync("SmartPaste.System");
        var userPrompt = await prompts.GetTemplateAsync("SmartPaste.User", new { Data = clipboard });
        
        var response = await chatClient.CompleteAsync(
            [new ChatMessage(ChatRole.System, systemPrompt), new ChatMessage(ChatRole.User, userPrompt)]
        );
        // ...
    }
}
```

### Step 2: OpenRouter Compatibility (Example Usage)
Modern usage should look like this in `Program.cs`:

```csharp
// User installs Microsoft.Extensions.AI.OpenAI
builder.Services.AddChatClient(new OpenAIClient(
    new Uri("https://openrouter.ai/api/v1"), 
    new ApiKeyCredential("sk-or-...")
).AsChatClient("google/gemini-pro-1.5"));

// SmartComponents just works
builder.Services.AddSmartComponents();
```

## 5. Next Actions (Prioritized)

1.  **Delete**: `src/SmartComponents.LocalEmbeddings` project.
2.  **Refactor**: Extract `SmartPasteInference` prompts to a `Prompts` class or resource file.
3.  **Refactor**: Update `SmartComponents.AspNetCore` to remove references to `LocalEmbedder`.
4.  **Verify**: creating a test ensuring `AddSmartComponents` works with a mocked `IChatClient`.
