namespace SmartComponents.Abstractions;

public enum SmartImageSafetyThreshold
{
    Low,
    Medium,
    High
}

public class SmartImageRequestData
{
    public string? ImageUrl { get; set; }
    public string? Base64Image { get; set; }
    public string? ModelOverride { get; set; }
    public bool EnableSafetyCheck { get; set; }
    public SmartImageSafetyThreshold SafetyThreshold { get; set; } = SmartImageSafetyThreshold.Medium;
}

public class SmartImageResponseData
{
    public string? AltText { get; set; }
    public bool IsSafe { get; set; }
    public SmartImageFocalPoint? FocalPoint { get; set; }
}

public class SmartImageFocalPoint
{
    public float X { get; set; }
    public float Y { get; set; }
}
