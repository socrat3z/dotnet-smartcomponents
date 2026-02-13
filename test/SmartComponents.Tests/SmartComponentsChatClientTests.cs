using Microsoft.Extensions.AI;
using Moq;
using SmartComponents.Inference;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SmartComponents.Tests;

public class SmartComponentsChatClientTests
{
    [Fact]
    public async Task GetResponseAsync_DelegatesToInnerClient()
    {
        // Arrange
        var mockInner = new Mock<IChatClient>();
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, "Hello"));
        mockInner.Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var client = new SmartComponentsChatClient(mockInner.Object);
        var messages = new[] { new ChatMessage(ChatRole.User, "Hi") };

        // Act
        var result = await client.GetResponseAsync(messages);

        // Assert
        Assert.Equal("Hello", result.Text);
        mockInner.Verify(c => c.GetResponseAsync(messages, It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
