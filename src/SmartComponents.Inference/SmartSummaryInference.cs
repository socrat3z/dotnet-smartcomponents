using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using SmartComponents.Abstractions;
using SmartComponents.StaticAssets.Inference;

namespace SmartComponents.Inference;

/// <summary>
/// Provides smart summarization capabilities using an AI backend.
/// </summary>
public class SmartSummaryInference : ISmartSummaryInference
{
    private readonly IPromptTemplateProvider _promptProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartSummaryInference"/> class.
    /// </summary>
    public SmartSummaryInference() : this(new EmbeddedResourcePromptTemplateProvider())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartSummaryInference"/> class with a custom prompt provider.
    /// </summary>
    /// <param name="promptProvider">The prompt provider.</param>
    public SmartSummaryInference(IPromptTemplateProvider promptProvider)
    {
        _promptProvider = promptProvider;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> SummarizeStreamingAsync(IChatClient chatClient, SmartSummaryRequestData requestData, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestData.Text))
        {
            yield break;
        }

        var chatParameters = BuildPrompt(requestData);
        
        await foreach (var update in chatClient.GetStreamingResponseAsync(chatParameters.Messages, chatParameters.Options, cancellationToken))
        {
            if (update.Text is { } text)
            {
                yield return text;
            }
        }
    }

    /// <summary>
    /// Builds the chat parameters (prompt) for the smart summary request.
    /// </summary>
    /// <param name="data">The summarization request data.</param>
    /// <returns>The chat parameters.</returns>
    public ChatParameters BuildPrompt(SmartSummaryRequestData data)
    {
        var systemTemplate = _promptProvider.GetTemplate("SmartSummary.System");
        var userTemplate = _promptProvider.GetTemplate("SmartSummary.User");

        var lengthInstruction = GetLengthInstruction(data.LengthPreference);
        var focusInstruction = GetFocusInstruction(data.FocusArea);

        var systemMessage = systemTemplate
            .Replace("{length_instruction}", lengthInstruction)
            .Replace("{focus_instruction}", focusInstruction ?? string.Empty);

        var prompt = userTemplate
            .Replace("{text}", data.Text);

        return new ChatParameters
        {
            Messages =
            [
                new(ChatRole.System, systemMessage),
                new(ChatRole.User, prompt),
            ],
            Options = new ChatOptions
            {
                Temperature = 0.3f, // Slightly higher for better summarization flow, but still low
            }
        };
    }

    private static string GetLengthInstruction(SummaryLengthPreference preference)
    {
        return preference switch
        {
            SummaryLengthPreference.TlDr => "Provide a 'TL;DR' summary: a single, concise paragraph capturing the essence.",
            SummaryLengthPreference.KeyPoints => "Provide a bulleted list of the key points.",
            SummaryLengthPreference.FullBrief => "Provide a detailed briefing style summary.",
            _ => "Provide a concise summary."
        };
    }

    private static string? GetFocusInstruction(string? focusArea)
    {
        if (string.IsNullOrWhiteSpace(focusArea))
        {
            return null;
        }

        return $"Focus specifically on the following area: {focusArea}";
    }
}
