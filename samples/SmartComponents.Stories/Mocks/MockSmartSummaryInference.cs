using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using SmartComponents.Abstractions;

namespace SmartComponents.Stories.Mocks;

public class MockSmartSummaryInference : ISmartSummaryInference
{
    public async IAsyncEnumerable<string> SummarizeStreamingAsync(IChatClient chatClient, SmartSummaryRequestData requestData, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string mockSummary;
        
        if (requestData.LengthPreference == SummaryLengthPreference.TlDr)
        {
             mockSummary = "TL;DR: Short summary. Key points only.";
        }
        else
        {
             mockSummary = $"This is a detailed summary for '{requestData.Text}'. It streams word by word to simulate LLM generation. The content is summarized based on the input text length and complexity.";
        }

        var words = mockSummary.Split(' ');
        foreach (var word in words)
        {
            await Task.Delay(100, cancellationToken);
            yield return word + " ";
        }
    }
}
