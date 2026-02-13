namespace SmartComponents.Abstractions;

/// <summary>
/// Data required for a smart paste request.
/// </summary>
public class SmartPasteRequestData
{
    /// <summary>
    /// Gets or sets the fields in the form.
    /// </summary>
    public FormField[]? FormFields { get; set; }

    /// <summary>
    /// Gets or sets the contents of the clipboard.
    /// </summary>
    public string? ClipboardContents { get; set; }
}

/// <summary>
/// Represents a field in a form.
/// </summary>
public class FormField
{
    /// <summary>
    /// Gets or sets the identifier for the field.
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// Gets or sets the description of the field.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the allowed values for the field, if applicable.
    /// </summary>
    public string?[]? AllowedValues { get; set; }

    /// <summary>
    /// Gets or sets the type of the field.
    /// </summary>
    public string? Type { get; set; }
}

/// <summary>
/// Represents the response data from a smart paste request.
/// </summary>
public readonly struct SmartPasteResponseData
{
    /// <summary>
    /// Gets a value indicating whether the request was bad.
    /// </summary>
    public bool BadRequest { get; init; }

    /// <summary>
    /// Gets the response content.
    /// </summary>
    public string? Response { get; init; }
}
