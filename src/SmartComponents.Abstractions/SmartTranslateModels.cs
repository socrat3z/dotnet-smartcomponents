using System.Collections.Generic;

namespace SmartComponents.Abstractions;

/// <summary>
/// Represents the data required for a smart translation request.
/// </summary>
public class SmartTranslateRequestData
{
    /// <summary>
    /// Gets or sets the text to be translated.
    /// </summary>
    public string? OriginalText { get; set; }

    /// <summary>
    /// Gets or sets the target language code (e.g., "es", "fr").
    /// </summary>
    public string? TargetLanguage { get; set; } // Required

    /// <summary>
    /// Gets or sets additional user instructions for the translation.
    /// </summary>
    public string? UserInstructions { get; set; }

    /// <summary>
    /// Gets or sets the page context to help with translation nuances.
    /// </summary>
    public string? PageContext { get; set; }

    /// <summary>
    /// Gets or sets a glossary of specific terms and their translations.
    /// </summary>
    public Dictionary<string, string>? Glossary { get; set; }

    /// <summary>
    /// Gets or sets the previous translation for refinement purposes.
    /// </summary>
    public string? PreviousTranslation { get; set; }
}

/// <summary>
/// Represents the response data from a smart translation request.
/// </summary>
public readonly struct SmartTranslateResponseData
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was invalid.
    /// </summary>
    public bool BadRequest { get; init; }

    /// <summary>
    /// Gets or sets the translated text.
    /// </summary>
    public string? TranslatedText { get; init; }
}
