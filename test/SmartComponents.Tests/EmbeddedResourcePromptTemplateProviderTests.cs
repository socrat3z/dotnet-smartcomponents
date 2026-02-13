using SmartComponents.Inference;
using Xunit;
using System.Threading.Tasks;

namespace SmartComponents.Tests;

public class EmbeddedResourcePromptTemplateProviderTests
{
    [Theory]
    [InlineData("SmartPaste.System")]
    [InlineData("SmartPaste.User")]
    [InlineData("SmartTextArea.System")]
    [InlineData("SmartTextArea.User")]
    public async Task CanLoadEmbeddedTemplates(string templateName)
    {
        var provider = new EmbeddedResourcePromptTemplateProvider();
        
        var template = await provider.GetTemplateAsync(templateName);
        
        Assert.NotNull(template);
        Assert.NotEmpty(template);
    }
    
    [Theory]
    [InlineData("SmartPaste.System")]
    [InlineData("SmartPaste.User")]
    [InlineData("SmartTextArea.System")]
    [InlineData("SmartTextArea.User")]
    public void CanLoadEmbeddedTemplatesSynchronously(string templateName)
    {
        var provider = new EmbeddedResourcePromptTemplateProvider();
        
        var template = provider.GetTemplate(templateName);
        
        Assert.NotNull(template);
        Assert.NotEmpty(template);
    }

    [Fact]
    public void GetTemplate_ThrowsForMissingResource()
    {
        var provider = new EmbeddedResourcePromptTemplateProvider();
        Assert.Throws<System.IO.FileNotFoundException>(() => provider.GetTemplate("NonExtant"));
    }

    [Fact]
    public async Task GetTemplateAsync_ThrowsForMissingResource()
    {
        var provider = new EmbeddedResourcePromptTemplateProvider();
        await Assert.ThrowsAsync<System.IO.FileNotFoundException>(() => provider.GetTemplateAsync("NonExtant"));
    }
}
