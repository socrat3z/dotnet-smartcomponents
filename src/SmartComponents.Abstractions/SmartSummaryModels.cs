namespace SmartComponents.Abstractions;

/// <summary>
/// Specifies the preferred length or format of the summary.
/// </summary>
public enum SummaryLengthPreference
{
    /// <summary>
    /// A very short "Too Long; Didn't Read" summary.
    /// </summary>
    TlDr,

    /// <summary>
    /// A bulleted list of key points.
    /// </summary>
    KeyPoints,

    /// <summary>
    /// A full paragraph summary.
    /// </summary>
    FullBrief
}

/// <summary>
/// Represents the data required to generate a smart summary.
/// </summary>
public class SmartSummaryRequestData
{
    /// <summary>
    /// Gets or sets the text to summarize.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the preferred length of the summary.
    /// </summary>
    public SummaryLengthPreference LengthPreference { get; set; }

    /// <summary>
    /// Gets or sets a specific area of focus for the summary.
    /// </summary>
    public string? FocusArea { get; set; }
}

// For streaming, we might not need a response data structure if we yield strings.
// But for non-streaming or metadata, we might.
// Keeping it simple for now.
