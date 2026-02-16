using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using SmartComponents.Abstractions;

namespace SmartComponents.Stories.Mocks;

public class MockSmartTranslateInference : ISmartTranslateInference
{
    public Task<SmartTranslateResponseData> TranslateAsync(IChatClient chatClient, SmartTranslateRequestData requestData)
    {
        string translated = $"[Translated ({requestData.TargetLanguage})]: {requestData.OriginalText}";
        
        if (!string.IsNullOrEmpty(requestData.UserInstructions))
        {
            translated += $" (Refined with: {requestData.UserInstructions})";
        }
        
        return Task.FromResult(new SmartTranslateResponseData
        {
            TranslatedText = translated
        });
    }
}
