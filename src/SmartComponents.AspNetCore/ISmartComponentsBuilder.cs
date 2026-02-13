// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.AI;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// A builder for smart components.
/// </summary>
public interface ISmartComponentsBuilder
{
    /// <summary>
    /// Configures the inference backend.
    /// </summary>
    /// <typeparam name="T">The type of the chat client.</typeparam>
    /// <param name="name">The name of the backend.</param>
    /// <returns>The builder instance.</returns>
    public ISmartComponentsBuilder WithInferenceBackend<T>(string? name = null) where T : class, IChatClient;

    /// <summary>
    /// Configures the inference backend.
    /// </summary>
    /// <param name="instance">The chat client instance.</param>
    /// <param name="name">The name of the backend.</param>
    /// <returns>The builder instance.</returns>
    public ISmartComponentsBuilder WithInferenceBackend(IChatClient instance, string? name = null);

    /// <summary>
    /// Enables antiforgery validation.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public ISmartComponentsBuilder WithAntiforgeryValidation();
}
