using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Agile.Chat.Domain.ChatThreads.ValueObjects;

public class ChatThreadPromptOptions
{
    public string SystemPrompt { get; set; }
    public float? Temperature { get; set; }
    public float? TopP { get; set; }
    public int? MaxTokens { get; set; }
    
    public AzureOpenAIPromptExecutionSettings ParseAzureOpenAiPromptExecutionSettings()
    {
        var options = new AzureOpenAIPromptExecutionSettings()
        {
            ChatSystemPrompt = string.IsNullOrWhiteSpace(SystemPrompt) ? null : SystemPrompt,
            Temperature = Temperature,
            TopP = TopP,
            MaxTokens = MaxTokens
        };
        return options;
    }
}