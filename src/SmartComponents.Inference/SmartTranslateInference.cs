using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using SmartComponents.Abstractions;
using SmartComponents.StaticAssets.Inference;

namespace SmartComponents.Inference;

/// <summary>
/// Provides smart translation capabilities using an AI backend.
/// </summary>
public class SmartTranslateInference : ISmartTranslateInference
{
    private readonly IPromptTemplateProvider _promptProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartTranslateInference"/> class.
    /// </summary>
    public SmartTranslateInference() : this(new EmbeddedResourcePromptTemplateProvider())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartTranslateInference"/> class with a custom prompt provider.
    /// </summary>
    /// <param name="promptProvider">The prompt provider.</param>
    public SmartTranslateInference(IPromptTemplateProvider promptProvider)
    {
        _promptProvider = promptProvider;
    }

    /// <inheritdoc />
    public async Task<SmartTranslateResponseData> TranslateAsync(IChatClient chatClient, SmartTranslateRequestData requestData)
    {
        if (string.IsNullOrWhiteSpace(requestData.OriginalText) || string.IsNullOrWhiteSpace(requestData.TargetLanguage))
        {
            return new SmartTranslateResponseData { BadRequest = true };
        }

        var chatParameters = BuildPrompt(requestData);
        var response = await chatClient.GetResponseAsync(chatParameters.Messages, chatParameters.Options);

        return new SmartTranslateResponseData { TranslatedText = response.Text };
    }

    /// <summary>
    /// Builds the chat parameters (prompt) for the smart translate request.
    /// </summary>
    /// <param name="data">The translation request data.</param>
    /// <returns>The chat parameters.</returns>
    public ChatParameters BuildPrompt(SmartTranslateRequestData data)
    {
        var systemTemplate = _promptProvider.GetTemplate("SmartTranslate.System");
        var userTemplate = _promptProvider.GetTemplate("SmartTranslate.User");

        var glossaryBlock = BuildGlossaryBlock(data.Glossary);
        var contextBlock = BuildContextBlock(data.PageContext);
        var instructionsBlock = BuildInstructionsBlock(data.UserInstructions);

        var systemMessage = systemTemplate
            .Replace("{target_language}", data.TargetLanguage)
            .Replace("{glossary_block}", glossaryBlock ?? string.Empty)
            .Replace("{context_block}", contextBlock ?? string.Empty)
            .Replace("{instructions_block}", instructionsBlock ?? string.Empty);

        var prompt = userTemplate
            .Replace("{original_text}", data.OriginalText);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemMessage)
        };
        
        if (!string.IsNullOrEmpty(data.PreviousTranslation))
        {
             // Refinement flow
             // We include original prompt, previous response, and new trigger.
             messages.Add(new(ChatRole.User, prompt));
             messages.Add(new(ChatRole.Assistant, data.PreviousTranslation));
             messages.Add(new(ChatRole.User, "Please refine the translation above based on the instructions provided."));
        }
        else
        {
             messages.Add(new(ChatRole.User, prompt));
        }

        return new ChatParameters
        {
            Messages = messages,
            Options = new ChatOptions
            {
                Temperature = 0f, 
            }
        };
    }

    private static string? BuildGlossaryBlock(Dictionary<string, string>? glossary)
    {
        if (glossary is null || glossary.Count == 0)
        {
            return null;
        }

        var sb = new StringBuilder();
        sb.AppendLine("You MUST use the following terminology:");
        foreach (var kvp in glossary)
        {
            sb.AppendLine($"- {kvp.Key}: {kvp.Value}");
        }
        return sb.ToString();
    }

    private static string? BuildContextBlock(string? context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return null;
        }

        return $"The text appears in the following context:\n{context}";
    }

    private static string? BuildInstructionsBlock(string? instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions))
        {
            return null;
        }

        return $"Additional instructions:\n{instructions}";
    }
}
