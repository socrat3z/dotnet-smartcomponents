// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace SmartComponents.StaticAssets.Inference;

/// <summary>
/// Parameters for a chat conversation.
/// </summary>
public class ChatParameters
{
    /// <summary>
    /// Gets or sets the list of chat messages.
    /// </summary>
    public IList<ChatMessage> Messages { get; set; } = [];

    /// <summary>
    /// Gets or sets the chat options.
    /// </summary>
    public ChatOptions? Options { get; set; }
}
