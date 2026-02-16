using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace SmartComponents.Abstractions;

/// <summary>
/// Provides inference capabilities for smart image analysis functionality.
/// </summary>
public interface ISmartImageInference
{
    /// <summary>
    /// Analyzes an image based on the provided request data asynchronously.
    /// </summary>
    /// <param name="chatClient">The chat client to use for inference.</param>
    /// <param name="requestData">The data containing the image and analysis parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the image analysis response data.</returns>
    Task<SmartImageResponseData> AnalyzeImageAsync(IChatClient chatClient, SmartImageRequestData requestData);
}
