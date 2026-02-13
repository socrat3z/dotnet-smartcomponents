using Microsoft.Extensions.AI;

namespace SmartComponents.Abstractions;

/// <summary>
/// Configuration for the smart text area.
/// </summary>
public struct SmartTextAreaConfig
{
    /// <summary>
    /// Gets or sets configuration parameters.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the user role for context.
    /// </summary>
    public string? UserRole { get; set; }

    /// <summary>
    /// Gets or sets user phrases to guide the inference.
    /// </summary>
    public string[]? UserPhrases { get; set; }
}

/// <summary>
/// Provides inference capabilities for smart text areas.
/// </summary>
public interface ISmartTextAreaInference
{
    /// <summary>
    /// Gets an insertion suggestion asynchronously.
    /// </summary>
    /// <param name="chatClient">The chat client to use for inference.</param>
    /// <param name="config">The configuration for the smart text area.</param>
    /// <param name="textBefore">The text before the cursor.</param>
    /// <param name="textAfter">The text after the cursor.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the suggested text.</returns>
    Task<string> GetInsertionSuggestionAsync(IChatClient chatClient, SmartTextAreaConfig config, string textBefore, string textAfter);
}
