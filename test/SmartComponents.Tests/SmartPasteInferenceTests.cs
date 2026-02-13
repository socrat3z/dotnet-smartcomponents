using Microsoft.Extensions.AI;
using Moq;
using SmartComponents.Inference;
using SmartComponents.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SmartComponents.Tests;

public class SmartPasteInferenceTests
{
    [Fact]
    public void BuildPrompt_UsesTemplateProvider()
    {
        // Arrange
        var mockProvider = new Mock<IPromptTemplateProvider>();
        mockProvider.Setup(p => p.GetTemplate("SmartPaste.System"))
            .Returns("Current date: {current_date}\n{field_output_examples}");
        mockProvider.Setup(p => p.GetTemplate("SmartPaste.User"))
            .Returns("USER_DATA: {user_data}");

        var inference = new SmartPasteInference(mockProvider.Object);
        var data = new SmartPasteRequestData
        {
            ClipboardContents = "Copied text contents",
            FormFields = new[] { new FormField { Identifier = "Name", Type = "text" } }
        };

        // Act
        var parameters = inference.BuildPrompt(data);

        // Assert
        Assert.NotNull(parameters);
        
        // System message check
        var systemMessage = parameters.Messages[0];
        Assert.Contains("Current date:", systemMessage.Text);
        Assert.Contains(DateTime.Today.ToString("D", CultureInfo.InvariantCulture), systemMessage.Text);
        Assert.Contains("\"Name\":", systemMessage.Text); // Field output example

        // User message check
        var userMessage = parameters.Messages[1];
        Assert.Contains("USER_DATA: Copied text contents", userMessage.Text);
    }

    [Fact]
    public async Task GetFormCompletionsAsync_ReturnsResponse()
    {
        // Arrange
        var mockProvider = new Mock<IPromptTemplateProvider>();
        mockProvider.Setup(p => p.GetTemplate(It.IsAny<string>())).Returns("Template");

        var mockChatClient = new Mock<IChatClient>();
        mockChatClient.Setup(c => c.GetResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Result JSON")));

        var inference = new SmartPasteInference(mockProvider.Object);
        var data = new SmartPasteRequestData
        {
            ClipboardContents = "Test Content",
            FormFields = new[] { new FormField { Identifier = "Name", Type = "text" } }
        };
        
        // Act
        var result = await inference.GetFormCompletionsAsync(mockChatClient.Object, data);

        // Assert
        Assert.False(result.BadRequest);
        Assert.Equal("Result JSON", result.Response);
    }

    [Fact]
    public async Task GetFormCompletionsAsync_WithJsonString_ReturnsResponse()
    {
        // Arrange
        var mockProvider = new Mock<IPromptTemplateProvider>();
        mockProvider.Setup(p => p.GetTemplate(It.IsAny<string>())).Returns("Template");

        var mockChatClient = new Mock<IChatClient>();
        mockChatClient.Setup(c => c.GetResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Result JSON")));

        var inference = new SmartPasteInference(mockProvider.Object);
        var json = "{\"ClipboardContents\":\"Test\",\"FormFields\":[{\"Identifier\":\"Field1\",\"Type\":\"text\"}]}";

        // Act
        var result = await inference.GetFormCompletionsAsync(mockChatClient.Object, json);

        // Assert
        Assert.False(result.BadRequest);
        Assert.Equal("Result JSON", result.Response);
    }

    [Fact]
    public async Task GetFormCompletionsAsync_WithEmptyFields_ReturnsBadRequest()
    {
        // Arrange
        var inference = new SmartPasteInference();
        var data = new SmartPasteRequestData { ClipboardContents = "Test", FormFields = System.Array.Empty<FormField>() };

        // Act
        var result = await inference.GetFormCompletionsAsync(new Mock<IChatClient>().Object, data);

        // Assert
        Assert.True(result.BadRequest);
    }

    [Fact]
    public void BuildPrompt_WithDetailedFields_IncludesDescriptionAndAllowedValues()
    {
        // Arrange
        var mockProvider = new Mock<IPromptTemplateProvider>();
        mockProvider.Setup(p => p.GetTemplate(It.IsAny<string>())).Returns("{field_output_examples}");

        var inference = new SmartPasteInference(mockProvider.Object);
        var data = new SmartPasteRequestData
        {
            ClipboardContents = "Test",
            FormFields = new[] 
            { 
                new FormField { Identifier = "Country", Type = "text", Description = "Born in", AllowedValues = new[] { "USA", "UK" } } 
            }
        };

        // Act
        var parameters = inference.BuildPrompt(data);

        // Assert
        var systemMessage = parameters.Messages[0].Text;
        Assert.Contains("The Born in", systemMessage);
        Assert.Contains("multiple choice, with allowed values: \"USA\",\"UK\"", systemMessage);
    }
}
