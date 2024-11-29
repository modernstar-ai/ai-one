using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Agile.Framework.Ai.Models;

public class ChatSettings
{
    public string? SystemPrompt { get; set; }
    public double? Temperature { get; set; }
    public double? TopP { get; set; }
    public int? MaxTokens { get; set; }

    public AzureOpenAIPromptExecutionSettings ParseAzureOpenAiPromptExecutionSettings()
    {
        var options = new AzureOpenAIPromptExecutionSettings()
        {
            ChatSystemPrompt = SystemPrompt,
            Temperature = Temperature,
            TopP = TopP,
            MaxTokens = MaxTokens
        };
        return options;
    }
}