using SmartComponents.Abstractions;
using Xunit;

namespace SmartComponents.Tests;

public class SimilarityQueryTests
{
    [Fact]
    public void CanInitializeQuery()
    {
        var query = new SimilarityQuery
        {
            SearchText = "test",
            MaxResults = 10,
            MinSimilarity = 0.5f
        };

        Assert.Equal("test", query.SearchText);
        Assert.Equal(10, query.MaxResults);
        Assert.Equal(0.5f, query.MinSimilarity);
    }
}
