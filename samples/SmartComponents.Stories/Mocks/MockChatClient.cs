using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace SmartComponents.Stories.Mocks;

public class MockChatClient : IChatClient
{
    public ChatClientMetadata Metadata => new("MockChatClient");

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    public TService? GetService<TService>(object? key = null) where TService : class
    {
        return this as TService;
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ChatResponse(new List<ChatMessage> { new(ChatRole.Assistant, "Mock response") }));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return new ChatResponseUpdate { Contents = [new TextContent("Mock")] };
        await Task.Yield();
    }
    
    public void Dispose() {}
}
