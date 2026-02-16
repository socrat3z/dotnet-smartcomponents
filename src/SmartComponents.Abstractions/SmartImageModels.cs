namespace SmartComponents.Abstractions;

/// <summary>
/// Specifies the threshold for safety checks.
/// </summary>
public enum SmartImageSafetyThreshold
{
    /// <summary>
    /// Low safety threshold.
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium safety threshold.
    /// </summary>
    Medium,
    
    /// <summary>
    /// High safety threshold.
    /// </summary>
    High
}

/// <summary>
/// Represents the data required for smart image analysis.
/// </summary>
public class SmartImageRequestData
{
    /// <summary>
    /// Gets or sets the URL of the image to analyze.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the Base64 representation of the image.
    /// </summary>
    public string? Base64Image { get; set; }

    /// <summary>
    /// Gets or sets an optional model override.
    /// </summary>
    public string? ModelOverride { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable safety checks.
    /// </summary>
    public bool EnableSafetyCheck { get; set; }

    /// <summary>
    /// Gets or sets the safety check threshold.
    /// </summary>
    public SmartImageSafetyThreshold SafetyThreshold { get; set; } = SmartImageSafetyThreshold.Medium;
}

/// <summary>
/// Represents the response data from smart image analysis.
/// </summary>
public class SmartImageResponseData
{
    /// <summary>
    /// Gets or sets the generated alternative text for the image.
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the image is considered safe.
    /// </summary>
    public bool IsSafe { get; set; }

    /// <summary>
    /// Gets or sets the detected focal point of the image.
    /// </summary>
    public SmartImageFocalPoint? FocalPoint { get; set; }
}

/// <summary>
/// Represents a focal point within an image (normalized coordinates 0-1).
/// </summary>
public class SmartImageFocalPoint
{
    /// <summary>
    /// Gets or sets the X coordinate (0.0 to 1.0).
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate (0.0 to 1.0).
    /// </summary>
    public float Y { get; set; }
}
