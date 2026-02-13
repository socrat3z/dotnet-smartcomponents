// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartComponents.Inference;
using SmartComponents.Abstractions;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for mapping smart combo box endpoints.
/// </summary>
public static class SmartComboBoxEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps a smart combo box endpoint.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <param name="url">The URL for the endpoint.</param>
    /// <param name="suggestions">A function that returns suggestions based on the request.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapSmartComboBox(this IEndpointRouteBuilder builder, string url, Func<SmartComboBoxRequest, IEnumerable<string>> suggestions)
        => MapSmartComboBoxCore(builder, url, req => Task.FromResult(suggestions(req)));

    /// <summary>
    /// Maps a smart combo box endpoint asynchronously.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <param name="url">The URL for the endpoint.</param>
    /// <param name="suggestions">A function that returns suggestions asynchronously based on the request.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapSmartComboBox(this IEndpointRouteBuilder builder, string url, Func<SmartComboBoxRequest, Task<IEnumerable<string>>> suggestions)
        => MapSmartComboBoxCore(builder, url, req => suggestions(req));

    private static IEndpointRouteBuilder MapSmartComboBoxCore(this IEndpointRouteBuilder builder, string url, Func<SmartComboBoxRequest, Task<IEnumerable<string>>> suggestions)
    {
        var validateAntiforgery = DefaultSmartComponentsBuilder.HasEnabledAntiForgeryValidation(builder.ServiceProvider);

        var endpoint = builder.MapPost(url, async (HttpContext httpContext,
            [FromServices] IAntiforgery antiforgery) =>
        {
            // See comment in SmartComponentsServiceCollectionExtensions to explain the antiforgery handling
            if (validateAntiforgery)
            {
                await antiforgery.ValidateRequestAsync(httpContext);
            }

            var form = httpContext.Request.Form;
            if (!(form.TryGetValue("inputValue", out var inputValue) && !string.IsNullOrEmpty(inputValue))
                || !(form.TryGetValue("maxResults", out var maxResultsString) && int.TryParse(maxResultsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxResults))
                || !(form.TryGetValue("similarityThreshold", out var similarityThresholdString) && float.TryParse(similarityThresholdString, NumberStyles.Float, CultureInfo.InvariantCulture, out var similarityThreshold)))
            {
                return Results.BadRequest("inputValue, maxResults, and similarityThreshold are required");
            }

            if (maxResults < 1 || maxResults > 100)
            {
                return Results.BadRequest("maxResults must be less than or equal to 100");
            }

            var suggestionsList = await suggestions(new SmartComboBoxRequest
            {
                HttpContext = httpContext,
                Query = new SimilarityQuery
                {
                    SearchText = inputValue.ToString(),
                    MaxResults = maxResults,
                    MinSimilarity = similarityThreshold,
                }
            });

            return Results.Ok(suggestionsList);
        });

        endpoint.DisableAntiforgery();

        return builder;
    }
}
