using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace SmartComponents.Tests;

public class OpenRouterExtensionsTests
{
    [Fact]
    public void WithOpenRouter_RegistersInferenceBackend()
    {
        var mockBuilder = new Mock<ISmartComponentsBuilder>();
        mockBuilder.Setup(b => b.WithInferenceBackend(It.IsAny<IChatClient>(), It.IsAny<string>()))
            .Returns(mockBuilder.Object);

        mockBuilder.Object.WithOpenRouter("fake-key", "fake-model", "https://example.com", "Example Site");

        mockBuilder.Verify(b => b.WithInferenceBackend(It.IsAny<IChatClient>(), It.IsAny<string>()), Times.Once);
    }
}
