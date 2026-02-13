// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SmartComponents.StaticAssets.Inference;
using Microsoft.Extensions.AI;
using SmartComponents.Abstractions;

namespace SmartComponents.Inference;

/// <summary>
/// Default implementation of <see cref="ISmartPasteInference"/>.
/// </summary>
public class SmartPasteInference : ISmartPasteInference
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Gets form completions asynchronously based on the provided request data in JSON format.
    /// </summary>
    /// <param name="inferenceBackend">The chat client to use for inference.</param>
    /// <param name="dataJson">The data containing form fields and clipboard contents in JSON format.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the smart paste response data.</returns>
    public Task<SmartPasteResponseData> GetFormCompletionsAsync(IChatClient inferenceBackend, string dataJson)
        => GetFormCompletionsAsync(inferenceBackend, JsonSerializer.Deserialize<SmartPasteRequestData>(dataJson, jsonSerializerOptions)!);

    private readonly IPromptTemplateProvider _promptProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartPasteInference"/> class.
    /// </summary>
    public SmartPasteInference() : this(new EmbeddedResourcePromptTemplateProvider())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartPasteInference"/> class with a custom prompt provider.
    /// </summary>
    /// <param name="promptProvider">The prompt provider.</param>
    public SmartPasteInference(IPromptTemplateProvider promptProvider)
    {
        _promptProvider = promptProvider;
    }

    /// <summary>
    /// Builds the chat parameters (prompt) for the smart paste request.
    /// </summary>
    /// <param name="data">The data containing form fields and clipboard contents.</param>
    /// <returns>The chat parameters.</returns>
    public virtual ChatParameters BuildPrompt(SmartPasteRequestData data)
    {
        var systemTemplate = _promptProvider.GetTemplate("SmartPaste.System");
        var userTemplate = _promptProvider.GetTemplate("SmartPaste.User");

        var systemMessage = systemTemplate
            .Replace("{current_date}", DateTime.Today.ToString("D", CultureInfo.InvariantCulture))
            .Replace("{field_output_examples}", ToFieldOutputExamples(data.FormFields!));

        var prompt = userTemplate
            .Replace("{user_data}", data.ClipboardContents);

        return new ChatParameters
        {
            Messages = [
                new (ChatRole.System, systemMessage),
                new (ChatRole.User, prompt),
            ],
            Options = new ChatOptions
            {
                Temperature = 0,
                TopP = 1,
                MaxOutputTokens = 2000,
                FrequencyPenalty = 0.1f,
                PresencePenalty = 0,
                ResponseFormat = ChatResponseFormat.Json,
            },
        };
    }

    /// <inheritdoc />
    public virtual async Task<SmartPasteResponseData> GetFormCompletionsAsync(IChatClient inferenceBackend, SmartPasteRequestData requestData)
    {
        if (requestData.FormFields is null || requestData.FormFields.Length == 0 || string.IsNullOrEmpty(requestData.ClipboardContents))
        {
            return new SmartPasteResponseData { BadRequest = true };
        }

        var chatParameters = BuildPrompt(requestData);
        var completionsResponse = await inferenceBackend.GetResponseAsync(chatParameters.Messages, chatParameters.Options);
        return new SmartPasteResponseData { Response = completionsResponse.Text };
    }

    private static string ToFieldOutputExamples(FormField[] fields)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");

        var firstField = true;
        foreach (var field in fields)
        {
            if (firstField)
            {
                firstField = false;
            }
            else
            {
                sb.AppendLine(",");
            }

            sb.Append($"  \"{field.Identifier}\": /* ");

            if (!string.IsNullOrEmpty(field.Description))
            {
                sb.Append($"The {field.Description}");
            }

            if (field.AllowedValues is { Length: > 0 })
            {
                sb.Append($" (multiple choice, with allowed values: ");
                var first = true;
                foreach (var value in field.AllowedValues)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append($"\"{value}\"");
                }
                sb.Append(")");
            }
            else
            {
                sb.Append($" of type {field.Type}");
            }

            sb.Append(" */");
        }

        sb.AppendLine();
        sb.AppendLine("}");
        return sb.ToString();
    }
}
