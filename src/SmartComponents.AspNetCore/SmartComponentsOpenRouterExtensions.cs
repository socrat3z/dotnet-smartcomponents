using System.Collections.Generic;
using Microsoft.Extensions.AI;
using OpenAI;
using System;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for configuring OpenRouter as the inference backend.
/// </summary>
public static class SmartComponentsOpenRouterExtensions
{
    /// <summary>
    /// Configures OpenRouter as the inference backend.
    /// </summary>
    /// <param name="builder">The smart components builder.</param>
    /// <param name="apiKey">The API key for OpenRouter.</param>
    /// <param name="modelId">The model ID to use.</param>
    /// <param name="siteUrl">The site URL.</param>
    /// <param name="siteTitle">The site title.</param>
    /// <returns>The smart components builder.</returns>
    public static ISmartComponentsBuilder WithOpenRouter(
        this ISmartComponentsBuilder builder,
        string apiKey,
        string modelId,
        string? siteUrl = null,
        string? siteTitle = null)
    {
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://openrouter.ai/api/v1")
        };

        if (!string.IsNullOrEmpty(siteUrl) || !string.IsNullOrEmpty(siteTitle))
        {
            options.AddPolicy(new OpenRouterHeaderPolicy(siteUrl, siteTitle), PipelinePosition.PerCall);
        }

        var client = new OpenAIClient(new ApiKeyCredential(apiKey), options);
        var chatClient = client.GetChatClient(modelId).AsIChatClient();

        return builder.WithInferenceBackend(chatClient);
    }

    private sealed class OpenRouterHeaderPolicy(string? siteUrl, string? siteTitle) : PipelinePolicy
    {
        public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            AddHeaders(message);
            ProcessNext(message, pipeline, currentIndex);
        }

        public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            AddHeaders(message);
            await ProcessNextAsync(message, pipeline, currentIndex).ConfigureAwait(false);
        }

        private void AddHeaders(PipelineMessage message)
        {
            if (!string.IsNullOrEmpty(siteUrl)) message.Request.Headers.Set("HTTP-Referer", siteUrl);
            if (!string.IsNullOrEmpty(siteTitle)) message.Request.Headers.Set("X-Title", siteTitle);
        }
    }
}
