namespace SmartComponents.Abstractions;

/// <summary>
/// Represents a query for similarity search.
/// </summary>
public readonly struct SimilarityQuery
{
    /// <summary>
    /// Gets the text to search for.
    /// </summary>
    public string SearchText { get; init; }

    /// <summary>
    /// Gets the maximum number of results to return.
    /// </summary>
    public int MaxResults { get; init; }

    /// <summary>
    /// Gets the minimum similarity score for results.
    /// </summary>
    public float? MinSimilarity { get; init; }
}
