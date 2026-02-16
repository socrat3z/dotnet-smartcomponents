using System.Collections.Generic;
using Microsoft.Extensions.AI;

namespace SmartComponents.Abstractions;

/// <summary>
/// Provides inference capabilities for smart summarization functionality.
/// </summary>
public interface ISmartSummaryInference
{
    /// <summary>
    /// Summarizes text based on the provided request data asynchronously, streaming the result.
    /// </summary>
    /// <param name="chatClient">The chat client to use for inference.</param>
    /// <param name="requestData">The data containing the text and summarization preferences.</param>
    /// <returns>An async enumerable of strings representing the streamed summary chunks.</returns>
    IAsyncEnumerable<string> SummarizeStreamingAsync(IChatClient chatClient, SmartSummaryRequestData requestData);
}
