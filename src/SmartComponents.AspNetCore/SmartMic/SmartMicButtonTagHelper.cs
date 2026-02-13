// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace SmartComponents.AspNetCore;

/// <summary>
/// A Tag Helper that renders a smart mic button for voice input.
/// </summary>
[HtmlTargetElement("smart-mic-button", TagStructure = TagStructure.NormalOrSelfClosing)]
public class SmartMicButtonTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the view context.
    /// </summary>
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether to use the default icon.
    /// </summary>
    public bool DefaultIcon { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.Attributes.Add("type", "button");
        output.Attributes.Add("title", "Use voice input to fill out the form");
        output.Attributes.Add("data-smart-mic-trigger", "true");

        var services = ViewContext.HttpContext.RequestServices;
        var urlHelper = services.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(ViewContext);
        output.Attributes.Add("data-url", urlHelper.Content("~/_smartcomponents/smartpaste"));

        var antiforgery = services.GetRequiredService<IAntiforgery>();
        if (antiforgery is not null)
        {
            var tokens = antiforgery.GetAndStoreTokens(ViewContext.HttpContext);
            output.Attributes.Add("data-antiforgery-name", tokens.FormFieldName);
            output.Attributes.Add("data-antiforgery-value", tokens.RequestToken);
        }

        if (!output.Attributes.ContainsName("class"))
        {
            output.AddClass("smart-paste-button", HtmlEncoder.Default);
        }

        if (DefaultIcon)
        {
            output.PreContent.SetHtmlContent(@"
<svg class=""smart-mic-icon smart-mic-icon-normal"" fill=""currentColor"" viewBox=""0 0 16 16"" xmlns=""http://www.w3.org/2000/svg"">
    <path d=""M3.5 6.5A.5.5 0 0 1 4 7v1a4 4 0 0 0 8 0V7a.5.5 0 0 1 1 0v1a5 5 0 0 1-4.5 4.975V15h3a.5.5 0 0 1 0 1h-7a.5.5 0 0 1 0-1h3v-2.025A5 5 0 0 1 3 8V7a.5.5 0 0 1 .5-.5z""/>
    <path d=""M10 8a2 2 0 1 1-4 0V3a2 2 0 1 1 4 0v5zM8 0a3 3 0 0 0-3 3v5a3 3 0 0 0 6 0V3a3 3 0 0 0-3-3z""/>
</svg>
<svg class=""smart-mic-icon smart-mic-icon-running"" viewBox=""0 0 24 24"" xmlns=""http://www.w3.org/2000/svg""><g stroke=""currentColor""><circle cx=""12"" cy=""12"" r=""9.5"" fill=""none"" stroke-width=""3"" stroke-linecap=""round""><animate attributeName=""stroke-dasharray"" dur=""1.5s"" calcMode=""spline"" values=""0 150;42 150;42 150;42 150"" keyTimes=""0;0.475;0.95;1"" keySplines=""0.42,0,0.58,1;0.42,0,0.58,1;0.42,0,0.58,1"" repeatCount=""indefinite"" /><animate attributeName=""stroke-dashoffset"" dur=""1.5s"" calcMode=""spline"" values=""0;-16;-59;-59"" keyTimes=""0;0.475;0.95;1"" keySplines=""0.42,0,0.58,1;0.42,0,0.58,1;0.42,0,0.58,1"" repeatCount=""indefinite"" /></circle><animateTransform attributeName=""transform"" type=""rotate"" dur=""2s"" values=""0 12 12;360 12 12"" repeatCount=""indefinite"" /></g></svg>
");
        }

        if (output.TagMode == TagMode.StartTagAndEndTag
            && (await output.GetChildContentAsync()) is { } content
            && (content is { IsEmptyOrWhiteSpace: false }))
        {
            output.Content = content;
        }
        else
        {
            // Default content. In most cases we expect apps to override this, since that's how
            // it would be localized.
            output.Content.SetHtmlContent("Voice Input");
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}
