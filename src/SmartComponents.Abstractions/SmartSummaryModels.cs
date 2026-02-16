namespace SmartComponents.Abstractions;

public enum SummaryLengthPreference
{
    TlDr,
    KeyPoints,
    FullBrief
}

public class SmartSummaryRequestData
{
    public string? Text { get; set; }
    public SummaryLengthPreference LengthPreference { get; set; }
    public string? FocusArea { get; set; }
}

// For streaming, we might not need a response data structure if we yield strings.
// But for non-streaming or metadata, we might.
// Keeping it simple for now.
