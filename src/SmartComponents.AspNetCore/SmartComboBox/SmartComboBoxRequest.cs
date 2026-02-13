// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using SmartComponents.Inference;
using SmartComponents.Abstractions;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Represents a request for smart combo box suggestions.
/// </summary>
public readonly struct SmartComboBoxRequest
{
    /// <summary>
    /// Gets the similarity query associated with the request.
    /// </summary>
    public SimilarityQuery Query { get; init; }

    /// <summary>
    /// Gets the HTTP context associated with the request.
    /// </summary>
    public HttpContext HttpContext { get; init; }
}
