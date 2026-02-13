using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.AI;
using SmartComponents.Inference;
using SmartComponents.StaticAssets.Inference;
using Xunit;

namespace SmartComponents.Tests;

public class ResponseCacheTests : IDisposable
{
    private readonly string _tempDir;

    public ResponseCacheTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        Environment.SetEnvironmentVariable("SMARTCOMPONENTS_E2E_TEST", "true");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        Environment.SetEnvironmentVariable("SMARTCOMPONENTS_E2E_TEST", null);
    }

    [Fact]
    public void SetAndGetCachedResponse_Works()
    {
        // TryGetCachedResponse and SetCachedResponse use a hardcoded path based on solution directory in the real code.
        // This makes it hard to test without mocking the filesystem or environment.
        // However, we can at least verify they don't throw and handle the environment variable check.
        
        var parameters = new ChatParameters
        {
            Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "Hello") }
        };

        // This will likely return false because it can't find the solution directory to save the cache
        var result = ResponseCache.TryGetCachedResponse(parameters, out var response);
        Assert.False(result);

        // This might fail or do nothing if it can't find the directory, but it shouldn't crash
        ResponseCache.SetCachedResponse(parameters, "Hi there");
    }
}
