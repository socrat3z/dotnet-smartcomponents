using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace SmartComponents.Abstractions;

/// <summary>
/// Provides inference capabilities for smart translation functionality.
/// </summary>
public interface ISmartTranslateInference
{
    /// <summary>
    /// Translates text based on the provided request data asynchronously.
    /// </summary>
    /// <param name="chatClient">The chat client to use for inference.</param>
    /// <param name="requestData">The data containing the text and translation parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the translation response data.</returns>
    Task<SmartTranslateResponseData> TranslateAsync(IChatClient chatClient, SmartTranslateRequestData requestData);
}
