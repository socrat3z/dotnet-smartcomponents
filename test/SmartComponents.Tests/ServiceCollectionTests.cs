using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartComponents.Abstractions;
using SmartComponents.Inference;
using Xunit;

namespace SmartComponents.Tests;

public class ServiceCollectionTests
{
    [Fact]
    public void AddSmartComponents_RegistersInferenceServices()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddSmartComponents();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ISmartTextAreaInference>());
        Assert.NotNull(serviceProvider.GetService<ISmartPasteInference>());
    }

    [Fact]
    public void AddSmartComponents_WithInferenceBackend_RegistersChatClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockChatClient = new Mock<IChatClient>();

        // Act
        services.AddSmartComponents()
            .WithInferenceBackend(mockChatClient.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatClient = serviceProvider.GetService<IChatClient>();
        Assert.NotNull(chatClient);
        Assert.Same(mockChatClient.Object, chatClient);
    }

    [Fact]
    public void WithInferenceBackend_Generic_RegistersType()
    {
        var services = new ServiceCollection();
        services.AddSmartComponents().WithInferenceBackend<StubChatClient>();
        var serviceProvider = services.BuildServiceProvider();

        var chatClient = serviceProvider.GetService<IChatClient>();
        Assert.IsType<StubChatClient>(chatClient);
    }

    private sealed class StubChatClient : IChatClient
    {
        public void Dispose() { }
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
    }

    [Fact]
    public void WithAntiforgeryValidation_RegistersValidation()
    {
        var services = new ServiceCollection();
        services.AddSmartComponents().WithAntiforgeryValidation();
        var serviceProvider = services.BuildServiceProvider();

        Assert.True(DefaultSmartComponentsBuilder.HasEnabledAntiForgeryValidation(serviceProvider));
    }

    [Fact]
    public void WithInferenceBackend_Keyed_RegistersKeyedClient()
    {
        var services = new ServiceCollection();
        var mockChatClient = new Mock<IChatClient>();
        services.AddSmartComponents().WithInferenceBackend(mockChatClient.Object, "my-key");
        var serviceProvider = services.BuildServiceProvider();

        var chatClient = serviceProvider.GetKeyedService<IChatClient>("my-key");
        Assert.NotNull(chatClient);
        Assert.Same(mockChatClient.Object, chatClient);
    }

    [Fact]
    public void AddSmartComponents_RegistersTagHelperComponent()
    {
        var services = new ServiceCollection();
        services.AddSmartComponents();
        var serviceProvider = services.BuildServiceProvider();

        var tagHelperComponents = serviceProvider.GetServices<Microsoft.AspNetCore.Razor.TagHelpers.ITagHelperComponent>();
        Assert.Contains(tagHelperComponents, t => t.GetType().Name == "SmartComponentsScriptTagHelperComponent");
    }

    [Fact]
    public void AddSmartComponents_RegistersStartupFilter()
    {
        var services = new ServiceCollection();
        services.AddSmartComponents();
        var serviceProvider = services.BuildServiceProvider();

        var startupFilters = serviceProvider.GetServices<Microsoft.AspNetCore.Hosting.IStartupFilter>();
        Assert.Contains(startupFilters, f => f.GetType().Name == "AttachSmartComponentsEndpointsStartupFilter");
    }
}
