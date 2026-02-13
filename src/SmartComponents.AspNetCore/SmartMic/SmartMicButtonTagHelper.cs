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
            output.AddClass("smart-mic-button", HtmlEncoder.Default);
        }

        if (DefaultIcon)
        {
            output.PreContent.SetHtmlContent(@"
<svg class=""smart-mic-icon"" fill=""currentColor"" viewBox=""0 0 24 24"" xmlns=""http://www.w3.org/2000/svg"">
    <path d=""M12 14c1.66 0 3-1.34 3-3V5c0-1.66-1.34-3-3-3S9 3.34 9 5v6c0 1.66 1.34 3 3 3z""/>
    <path d=""M17 11c0 2.76-2.24 5-5 5s-5-2.24-5-5H5c0 3.53 2.61 6.43 6 6.92V21h2v-3.08c3.39-.49 6-3.39 6-6.92h-2z""/>
</svg>
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
