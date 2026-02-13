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

public class SmartTextAreaInferenceTests
{
    [Fact]
    public void BuildPrompt_UsesTemplateProvider()
    {
        // Arrange
        var mockProvider = new Mock<IPromptTemplateProvider>();
        mockProvider.Setup(p => p.GetTemplate("SmartTextArea.System"))
            .Returns("Predict what text... {stock_phrases}");
        mockProvider.Setup(p => p.GetTemplate("SmartTextArea.User"))
            .Returns("ROLE: {user_role}\nUSER_TEXT: {text_before}^^^{text_after}");
        mockProvider.Setup(p => p.GetTemplate("SmartTextArea.Examples"))
            .Returns("USER: ExampleUser\nASSISTANT: ExampleAssistant");

        var inference = new SmartTextAreaInference(mockProvider.Object);
        var config = new SmartTextAreaConfig { UserRole = "TestRole", UserPhrases = new[] { "Phrase1" } };

        // Act
        var parameters = inference.BuildPrompt(config, "Hello ", " world");

        // Assert
        Assert.NotNull(parameters);
        Assert.True(parameters.Messages.Count >= 2, "Expected at least 2 messages (System and User), but got " + parameters.Messages.Count);
        
        // System message check
        var systemMessage = parameters.Messages[0];
        Assert.Contains("Predict what text...", systemMessage.Text);
        Assert.Contains("Phrase1", systemMessage.Text); // Should verify stock phrases are replaced
        
        // User message check
        var userMessage = parameters.Messages.Last(); // Wait, Messages list contains examples
        // Actually, BuildPrompt constructs a list with pre-defimed messages in the middle.
        // Let's check the LAST message which is the user prompt.
        userMessage = parameters.Messages[parameters.Messages.Count - 1];
        Assert.Contains("ROLE: TestRole", userMessage.Text);
        Assert.Contains("Hello ^^^ world", userMessage.Text);
    }

    [Fact]
    public async Task GetInsertionSuggestionAsync_ReturnsResponse()
    {
        // Arrange
        var mockProvider = new Mock<IPromptTemplateProvider>();
        mockProvider.Setup(p => p.GetTemplate(It.IsAny<string>())).Returns("Template");

        var mockChatClient = new Mock<IChatClient>();
        mockChatClient.Setup(c => c.GetResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "OK:[suggestion]")));

        var inference = new SmartTextAreaInference(mockProvider.Object);
        var config = new SmartTextAreaConfig { UserRole = "TestRole" };
        
        // Act
        var result = await inference.GetInsertionSuggestionAsync(mockChatClient.Object, config, "Text before", "Text after");

        // Assert
        Assert.Equal("suggestion", result);
    }
}
