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

public class SmartPasteButtonTagHelperTests
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

        var tagHelper = new SmartPasteButtonTagHelper
        {
            ViewContext = viewContext,
            DefaultIcon = true
        };

        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "smart-paste-button",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("button", output.TagName);
        Assert.Equal("button", output.Attributes["type"].Value);
        Assert.Equal("true", output.Attributes["data-smart-paste-trigger"].Value);
        Assert.Equal("formFieldName", output.Attributes["data-antiforgery-name"].Value);
        Assert.Equal("requestToken", output.Attributes["data-antiforgery-value"].Value);
        Assert.Contains("smart-paste-button", output.Attributes["class"].Value.ToString());
        
        var preContent = output.PreContent.GetContent();
        Assert.Contains("<svg", preContent);
        
        var content = output.Content.GetContent();
        Assert.Equal("Smart Paste", content);
    }
}
