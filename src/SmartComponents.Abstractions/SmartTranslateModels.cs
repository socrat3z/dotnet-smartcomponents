using System.Collections.Generic;

namespace SmartComponents.Abstractions;

public class SmartTranslateRequestData
{
    public string? OriginalText { get; set; }
    public string? TargetLanguage { get; set; } // Required
    public string? UserInstructions { get; set; }
    public string? PageContext { get; set; }
    public Dictionary<string, string>? Glossary { get; set; }
    public string? PreviousTranslation { get; set; }
}

public readonly struct SmartTranslateResponseData
{
    public bool BadRequest { get; init; }
    public string? TranslatedText { get; init; }
}
