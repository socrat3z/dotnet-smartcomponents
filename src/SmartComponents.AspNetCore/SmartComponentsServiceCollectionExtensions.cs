// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartComponents.AspNetCore;
using SmartComponents.Inference;
using SmartComponents.Abstractions;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for adding smart components to the service collection.
/// </summary>
public static class SmartComponentsServiceCollectionExtensions
{
    /// <summary>
    /// Adds smart components to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>A builder for configuring smart components.</returns>
    public static ISmartComponentsBuilder AddSmartComponents(this IServiceCollection services)
    {
        // Default inference implementations. Developers can subclass these and register their
        // own implementations if they want to override the prompts or the calls to the backend.
        services.TryAddScoped<ISmartTextAreaInference, SmartTextAreaInference>();
        services.TryAddScoped<ISmartPasteInference, SmartPasteInference>();
        services.TryAddScoped<ISmartTranslateInference, SmartTranslateInference>();

        services.AddTransient<IStartupFilter, AttachSmartComponentsEndpointsStartupFilter>();
        services.AddTransient<ITagHelperComponent, SmartComponentsScriptTagHelperComponent>();
        return new DefaultSmartComponentsBuilder(services);
    }

    private sealed class AttachSmartComponentsEndpointsStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => builder =>
        {
            next(builder);

            var validateAntiforgery = DefaultSmartComponentsBuilder.HasEnabledAntiForgeryValidation(builder.ApplicationServices);

            builder.UseEndpoints(app =>
            {
                var smartPasteEndpoint = app.MapPost("/_smartcomponents/smartpaste", async ([FromServices] IChatClient inference, HttpContext httpContext, [FromServices] IAntiforgery antiforgery, [FromServices] ISmartPasteInference smartPasteInference) =>
                {
                    // The rules about whether antiforgery are enabled by default vary across different
                    // ASP.NET Core versions. To make it consistent, we manually validate it if you've opted in.
                    // Note that antiforgery handling has issues (https://github.com/dotnet/aspnetcore/issues/54533)
                    // so until that's resolved we need this to be off by default.
                    if (validateAntiforgery)
                    {
                        await antiforgery.ValidateRequestAsync(httpContext);
                    }

                    if (!httpContext.Request.Form.TryGetValue("dataJson", out var dataJson))
                    {
                        return Results.BadRequest("dataJson is required");
                    }

                    var requestData = JsonSerializer.Deserialize<SmartPasteRequestData>(dataJson.ToString(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                    var result = await smartPasteInference.GetFormCompletionsAsync(inference, requestData);
                    return result.BadRequest ? Results.BadRequest() : Results.Content(result.Response!);
                });

                var smartTextAreaEndpoint = app.MapPost("/_smartcomponents/smarttextarea", async ([FromServices] IChatClient inference, HttpContext httpContext, [FromServices] IAntiforgery antiforgery, [FromServices] ISmartTextAreaInference smartTextAreaInference) =>
                {
                    if (validateAntiforgery)
                    {
                        // See above for why we validate antiforgery manually
                        await antiforgery.ValidateRequestAsync(httpContext);
                    }

                    var form = httpContext.Request.Form;
                    if (!form.TryGetValue("config", out var config)
                        || !form.TryGetValue("textBefore", out var textBefore)
                        || !form.TryGetValue("textAfter", out var textAfter))
                    {
                        return Results.BadRequest("config, textBefore, and textAfter are required");
                    }

                    var parsedConfig = JsonSerializer.Deserialize<SmartTextAreaConfig>(config.ToString())!;
                    var suggestion = await smartTextAreaInference.GetInsertionSuggestionAsync(inference, parsedConfig, textBefore.ToString(), textAfter.ToString());
                    return Results.Content(suggestion);
                });

                var smartTranslateEndpoint = app.MapPost("/_smartcomponents/smarttranslate", async ([FromServices] IChatClient inference, HttpContext httpContext, [FromServices] IAntiforgery antiforgery, [FromServices] ISmartTranslateInference smartTranslateInference) =>
                {
                    if (validateAntiforgery)
                    {
                        await antiforgery.ValidateRequestAsync(httpContext);
                    }

                    if (!httpContext.Request.Form.TryGetValue("dataJson", out var dataJson))
                    {
                        return Results.BadRequest("dataJson is required");
                    }

                    var requestData = JsonSerializer.Deserialize<SmartTranslateRequestData>(dataJson.ToString(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                    var result = await smartTranslateInference.TranslateAsync(inference, requestData);
                    return result.BadRequest ? Results.BadRequest() : Results.Json(result); 
                });

                // These APIs only exist on .NET 8+. It wasn't enabled by default in prior versions.
                smartPasteEndpoint.DisableAntiforgery();
                smartTextAreaEndpoint.DisableAntiforgery();
                smartTranslateEndpoint.DisableAntiforgery();
            });
        };
    }
}
