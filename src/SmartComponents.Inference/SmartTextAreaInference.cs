// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using SmartComponents.StaticAssets.Inference;
using SmartComponents.Abstractions;

namespace SmartComponents.Inference;

/// <summary>
/// Default implementation of <see cref="ISmartTextAreaInference"/>.
/// </summary>
public class SmartTextAreaInference : ISmartTextAreaInference
{
    private readonly IPromptTemplateProvider _promptProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartTextAreaInference"/> class.
    /// </summary>
    public SmartTextAreaInference() : this(new EmbeddedResourcePromptTemplateProvider())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartTextAreaInference"/> class with a custom prompt provider.
    /// </summary>
    /// <param name="promptProvider">The prompt provider.</param>
    public SmartTextAreaInference(IPromptTemplateProvider promptProvider)
    {
        _promptProvider = promptProvider;
    }

    /// <summary>
    /// Builds the chat parameters (prompt) for the smart text area request.
    /// </summary>
    /// <param name="config">The configuration for the smart text area.</param>
    /// <param name="textBefore">The text before the cursor.</param>
    /// <param name="textAfter">The text after the cursor.</param>
    /// <returns>The chat parameters.</returns>
    public virtual ChatParameters BuildPrompt(SmartTextAreaConfig config, string textBefore, string textAfter)
    {
        var systemTemplate = _promptProvider.GetTemplate("SmartTextArea.System");
        var stockPhrasesText = "";
        if (config.UserPhrases is { Length: > 0 } stockPhrases)
        {
            var sb = new StringBuilder("\nAlways try to use variations on the following phrases as part of the predictions:\n");
            foreach (var phrase in stockPhrases)
            {
                sb.AppendFormat("- {0}\n", phrase);
            }
            stockPhrasesText = sb.ToString();
        }
        
        var systemMessage = systemTemplate.Replace("{stock_phrases}", stockPhrasesText);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemMessage)
        };

        AddExamples(messages, _promptProvider.GetTemplate("SmartTextArea.Examples"));

        messages.Add(new(ChatRole.User, _promptProvider.GetTemplate("SmartTextArea.User")
                .Replace("{user_role}", config.UserRole)
                .Replace("{text_before}", textBefore)
                .Replace("{text_after}", textAfter)));

        return new ChatParameters
        {
            Messages = messages,
            Options = new ChatOptions
            {
                Temperature = 0,
                MaxOutputTokens = 400,
                StopSequences = ["END_INSERTION", "NEED_INFO"],
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            }
        };
    }

    private static void AddExamples(List<ChatMessage> messages, string examplesText)
    {
        var lines = examplesText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        ChatMessage? currentMessage = null;
        var currentContent = new StringBuilder();

        foreach (var line in lines)
        {
            if (line.StartsWith("USER: ", StringComparison.Ordinal))
            {
                if (currentMessage != null)
                {
                    messages.Add(new ChatMessage(currentMessage.Role, currentContent.ToString().Trim()));
                    currentContent.Clear();
                }
                currentMessage = new ChatMessage(ChatRole.User, "");
                currentContent.AppendLine(line.Substring(6));
            }
            else if (line.StartsWith("ASSISTANT: ", StringComparison.Ordinal))
            {
                if (currentMessage != null)
                {
                    messages.Add(new ChatMessage(currentMessage.Role, currentContent.ToString().Trim()));
                    currentContent.Clear();
                }
                currentMessage = new ChatMessage(ChatRole.Assistant, "");
                currentContent.AppendLine(line.Substring(11));
            }
            else
            {
                currentContent.AppendLine(line);
            }
        }

        if (currentMessage != null)
        {
            messages.Add(new ChatMessage(currentMessage.Role, currentContent.ToString().Trim()));
        }
    }

    /// <inheritdoc />
    public virtual async Task<string> GetInsertionSuggestionAsync(IChatClient inference, SmartTextAreaConfig config, string textBefore, string textAfter)
    {
        var chatParameters = BuildPrompt(config, textBefore, textAfter);
        var response = await inference.GetResponseAsync(chatParameters.Messages, chatParameters.Options);
        var responseText = response.Text;

        if (responseText.Length > 5 && responseText.StartsWith("OK:[", StringComparison.Ordinal))
        {
            // Avoid returning multiple sentences as it's unlikely to avoid inventing some new train of thought.
            var trimAfter = responseText.IndexOfAny(['.', '?', '!']);
            if (trimAfter > 0 && responseText.Length > trimAfter + 1 && responseText[trimAfter + 1] == ' ')
            {
                responseText = responseText.Substring(0, trimAfter + 1);
            }

            // Leave it up to the frontend code to decide whether to add a training space
            var trimmedResponse = responseText.Substring(4).TrimEnd(']', ' ');

            // Don't have a leading space on the suggestion if there's already a space right
            // before the cursor. The language model normally gets this right anyway (distinguishing
            // between starting a new word, vs continuing a partly-typed one) but sometimes it adds
            // an unnecessary extra space.
            if (textBefore.Length > 0 && textBefore[textBefore.Length - 1] == ' ')
            {
                trimmedResponse = trimmedResponse.TrimStart(' ');
            }

            return trimmedResponse;
        }

        return string.Empty;
    }
}
