using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartComponents.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SmartComponents.Tests;

public class SmartComboBoxTagHelperTests
{
    [Fact]
    public async Task ProcessAsync_SetsCorrectAttributes()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns<string>(s => s);
        var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
        mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(mockUrlHelper.Object);

        var mockAntiforgery = new Mock<IAntiforgery>();
        mockAntiforgery.Setup(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()))
            .Returns(new AntiforgeryTokenSet("requestToken", "cookieToken", "formFieldName", "headerName"));

        services.AddSingleton(mockUrlHelperFactory.Object);
        services.AddSingleton(mockAntiforgery.Object);

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var viewContext = new ViewContext(
            actionContext, 
            Mock.Of<IView>(), 
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()), 
            Mock.Of<ITempDataDictionary>(), 
            TextWriter.Null, 
            new HtmlHelperOptions());

        var tagHelper = new SmartComboBoxTagHelper
        {
            ViewContext = viewContext,
            Url = "/api/suggestions",
            MaxSuggestions = 5,
            SimilarityThreshold = 0.8f
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "smart-combobox",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("input", output.TagName);
        Assert.Equal("combobox", output.Attributes["role"].Value);
        
        var postElementContent = output.PostElement.GetContent();
        Assert.Contains("data-max-suggestions=\"5\"", postElementContent);
        Assert.Contains("data-similarity-threshold=\"0.8\"", postElementContent);
        Assert.Contains("data-suggestions-url=\"/api/suggestions\"", postElementContent);
        Assert.Contains("data-antiforgery-name=\"formFieldName\"", postElementContent);
        Assert.Contains("data-antiforgery-value=\"requestToken\"", postElementContent);
    }

    [Fact]
    public async Task ProcessAsync_ThrowsIfUrlMissing()
    {
        var tagHelper = new SmartComboBoxTagHelper();
        var context = new TagHelperContext(new TagHelperAttributeList(), new Dictionary<object, object>(), "id");
        var output = new TagHelperOutput("smart-combobox", new TagHelperAttributeList(), (c, e) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        await Assert.ThrowsAsync<InvalidOperationException>(() => tagHelper.ProcessAsync(context, output));
    }
}
