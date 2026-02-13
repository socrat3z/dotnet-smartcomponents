using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SmartComponents.Inference;

/// <summary>
/// Provides access to prompt templates.
/// </summary>
public interface IPromptTemplateProvider
{
    /// <summary>
    /// Gets a prompt template by name.
    /// </summary>
    /// <param name="templateName">The name of the template.</param>
    /// <returns>The template content.</returns>
    string GetTemplate(string templateName);

    /// <summary>
    /// Gets a prompt template by name asynchronously.
    /// </summary>
    /// <param name="templateName">The name of the template.</param>
    /// <returns>A task containing the template content.</returns>
    Task<string> GetTemplateAsync(string templateName);
}

/// <summary>
/// Provides access to prompt templates stored as embedded resources.
/// </summary>
public class EmbeddedResourcePromptTemplateProvider : IPromptTemplateProvider
{
    private readonly Assembly _assembly;
    private readonly string _baseNamespace;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedResourcePromptTemplateProvider"/> class.
    /// </summary>
    public EmbeddedResourcePromptTemplateProvider()
    {
        _assembly = typeof(EmbeddedResourcePromptTemplateProvider).Assembly;
        _baseNamespace = "SmartComponents.Inference.Prompts";
    }

    /// <inheritdoc />
    public string GetTemplate(string templateName)
    {
        var resourceNameBase = $"{_baseNamespace}.{templateName}";
        var stream = _assembly.GetManifestResourceStream($"{resourceNameBase}.md") 
            ?? _assembly.GetManifestResourceStream($"{resourceNameBase}.txt");

        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceNameBase}.md' or '.txt' not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <inheritdoc />
    public async Task<string> GetTemplateAsync(string templateName)
    {
        var resourceNameBase = $"{_baseNamespace}.{templateName}";
        var stream = _assembly.GetManifestResourceStream($"{resourceNameBase}.md") 
            ?? _assembly.GetManifestResourceStream($"{resourceNameBase}.txt");

        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceNameBase}.md' or '.txt' not found.");
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
