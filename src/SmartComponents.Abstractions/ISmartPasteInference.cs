using Microsoft.Extensions.AI;

namespace SmartComponents.Abstractions;

/// <summary>
/// Provides inference capabilities for smart paste functionality.
/// </summary>
public interface ISmartPasteInference
{
    /// <summary>
    /// Gets form completions asynchronously based on the provided request data.
    /// </summary>
    /// <param name="chatClient">The chat client to use for inference.</param>
    /// <param name="requestData">The data containing form fields and clipboard contents.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the smart paste response data.</returns>
    Task<SmartPasteResponseData> GetFormCompletionsAsync(IChatClient chatClient, SmartPasteRequestData requestData);
}
