using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using SmartComponents.Abstractions;

namespace SmartComponents.Stories.Mocks;

public class MockSmartImageInference : ISmartImageInference
{
    public Task<SmartImageResponseData> AnalyzeImageAsync(IChatClient chatClient, SmartImageRequestData requestData)
    {
        bool isUnsafe = requestData.ImageUrl?.Contains("unsafe") ?? false;
        
        if (isUnsafe)
        {
             return Task.FromResult(new SmartImageResponseData
             {
                 AltText = "Content restricted.",
                 IsSafe = false,
                 FocalPoint = null
             });
        }
        
        return Task.FromResult(new SmartImageResponseData
        {
            AltText = "A mocked safe image description: A beautiful landscape.",
            IsSafe = true,
            FocalPoint = new SmartImageFocalPoint { X = 0.5f, Y = 0.2f }
        });
    }
}
