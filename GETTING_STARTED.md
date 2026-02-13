# Getting Started with SmartComponents

This guide will help you set up and run the SmartComponents example applications.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for building TypeScript assets)
- An OpenRouter or OpenAI API key

## Quick Start

### 1. Clone and Build

```bash
# Build TypeScript assets
mise run build-ts

# Build the solution
mise run build
```

### 2. Configure Your AI Backend

The easiest way to get started is with **OpenRouter**, which gives you access to multiple AI models at competitive prices.

#### Option A: Using OpenRouter (Recommended)

1. Get an API key from [OpenRouter](https://openrouter.ai/keys)
2. Copy the sample environment file:
   ```bash
   cp .env.sample .env
   ```
3. Edit `.env` and add your OpenRouter API key:
   ```bash
   AI__OpenRouter__Key=your-openrouter-api-key-here
   AI__OpenRouter__Chat__ModelId=meta-llama/llama-3.3-70b-instruct
   AI__OpenRouter__Embedding__ModelId=openai/text-embedding-3-small
   ```

#### Option B: Using RepoSharedConfig.json

Alternatively, create a file named `RepoSharedConfig.json` at the repository root:

```json
{
  "AI": {
    "OpenRouter": {
      "Key": "your-openrouter-api-key-here",
      "Chat": {
        "ModelId": "meta-llama/llama-3.3-70b-instruct"
      },
      "Embedding": {
        "ModelId": "openai/text-embedding-3-small"
      }
    }
  }
}
```

#### Option C: Using OpenAI Directly

If you prefer OpenAI, edit `.env`:

```bash
AI__OpenAI__Key=your-openai-api-key-here
AI__OpenAI__Chat__ModelId=gpt-4o-mini
AI__OpenAI__Embedding__ModelId=text-embedding-3-small
```

### 3. Run the Example App

```bash
# Run the Blazor example (includes SmartMic demo)
mise run demo

# Or use the full command
mise run run-blazor
```

The app will start and display the URL (e.g., `https://localhost:7001`).

## Features to Try

### 🎤 SmartMic (NEW!)
Voice input for forms - perfect for mobile or hands-free scenarios.

1. Navigate to any form in the example app
2. Click the microphone button
3. Speak naturally (e.g., "Unit 4B has critical status, pump vibrating, needs immediate maintenance")
4. Review and edit the transcript
5. Click "Apply" - the form fills automatically!

**Requirements:**
- HTTPS or localhost (for browser microphone access)
- Chrome, Edge, or Safari (Web Speech API support)

### 📋 SmartPaste
Paste unstructured text and watch it fill out forms intelligently.

### ✨ SmartTextArea
Get AI-powered suggestions as you type in text areas.

### 🔍 SmartComboBox
Semantic search in dropdown menus using vector embeddings.

## Model Recommendations

### For Chat (SmartPaste, SmartMic, SmartTextArea)

| Model | Provider | Speed | Quality | Cost |
|-------|----------|-------|---------|------|
| `meta-llama/llama-3.3-70b-instruct` | Meta | Fast | High | Low |
| `anthropic/claude-3.5-sonnet` | Anthropic | Medium | Highest | High |
| `openai/gpt-4o-mini` | OpenAI | Fast | High | Medium |
| `google/gemini-flash-1.5` | Google | Very Fast | Good | Very Low |

### For Embeddings (SmartComboBox)

| Model | Quality | Dimensions | Cost |
|-------|---------|------------|------|
| `openai/text-embedding-3-small` | Good | 1536 | Low |
| `openai/text-embedding-3-large` | Best | 3072 | Medium |

See [OpenRouter Models](https://openrouter.ai/models) for current pricing.

## Project Structure

```
dotnet-smartcomponents/
├── src/
│   ├── SmartComponents.AspNetCore/        # Core server-side components
│   ├── SmartComponents.AspNetCore.Components/  # Blazor components
│   └── SmartComponents.StaticAssets/      # TypeScript/JS implementations
├── samples/
│   ├── ExampleBlazorApp/                  # Blazor demo app
│   └── ExampleMvcRazorPagesApp/          # MVC/Razor Pages demo
└── docs/
    ├── smart-mic.md                       # SmartMic documentation
    └── smart-mic-implementation-plan.md   # Technical details
```

## Troubleshooting

### "Microphone access denied"
- Ensure you're using HTTPS or localhost
- Check browser permissions for microphone access
- Try Chrome or Edge (best Web Speech API support)

### "The app starts but SmartComponents don't work"
- Verify your API key is correctly configured
- Check the console for error messages
- Ensure you have internet connectivity (AI APIs require network access)

### "Build errors about missing XML comments"
- Run `mise run build` instead of `dotnet build` directly
- Or disable XML documentation warnings in your IDE

## Development

```bash
# Build everything
mise run build

# Build only TypeScript
mise run build-ts

# Run tests
mise run test

# Create NuGet packages
mise run pack
```

## Learn More

- [SmartMic Documentation](docs/smart-mic.md)
- [SmartPaste Documentation](docs/smart-paste.md)
- [OpenRouter Documentation](https://openrouter.ai/docs)
- [Microsoft.Extensions.AI](https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-ai-preview/)

## License

Licensed under the MIT License. See LICENSE for details.
