// Copyright (c) Microsoft. All rights reserved.

using Azure.Core;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;

namespace MinimalApi.Services;
#pragma warning disable SKEXP0011 // Mark members as static
#pragma warning disable SKEXP0001 // Mark members as static
public class SimpleChatService
{
    private readonly Kernel _kernel;

    private static string? GetEnvVar(string key) => Environment.GetEnvironmentVariable(key);

    public SimpleChatService(       
        OpenAIClient client
        )
    {
        var kernelBuilder = Kernel.CreateBuilder();

        var deployedModelName = GetEnvVar("AZURE_OPENAI_API_DEPLOYMENT_NAME");
        ArgumentNullException.ThrowIfNullOrWhiteSpace(deployedModelName);
        var embeddingModelName = GetEnvVar("AZURE_OPENAI_API_EMBEDDINGS_DEPLOYMENT_NAME");
        if (!string.IsNullOrEmpty(embeddingModelName))
        {
            var endpoint = GetEnvVar("AZURE_OPENAI_ENDPOINT") ?? throw new ArgumentNullException() ;            
            var openAiAPIKey = GetEnvVar("AZURE_OPENAI_API_KEY") ?? throw new ArgumentNullException();
            
                        kernelBuilder = kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(embeddingModelName, endpoint, openAiAPIKey);            
            kernelBuilder = kernelBuilder.AddAzureOpenAIChatCompletion(deployedModelName, endpoint, openAiAPIKey);
        }

        _kernel = kernelBuilder.Build();        
    }

    public static string GetSystemPrompt(string template) {

        if (template=="science")
        {
            return @"You are a helpful science teacher. Be clear in your answers, and target a 12 year old audience";
        }

        if (template == "health") {
        return    @"You are a helpful AI assistant, generate search query for followup question.
Make your respond simple and precise. Return the query only, do not return any other text.
e.g.
Northwind Health Plus AND standard plan.
standard plan AND dental AND employee benefit.
";
        }


        return "You are a helpful assistant.";
}

    public async Task<string> ReplyAsync(
        ChatMessage[] history,
        CancellationToken cancellationToken = default)
    {
        
        var chat = _kernel.GetRequiredService<IChatCompletionService>();
        var embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        
        var question = history.LastOrDefault(m => m.IsUser)?.Content is { } userQuestion
            ? userQuestion
            : throw new InvalidOperationException("Use question is null");
   

        // step 1
        // use llm to get query if retrieval mode is not vector
        string? query = null;
        var systemPrompt = GetSystemPrompt("default");

        var queryChatHistory = new ChatHistory(systemPrompt);

        queryChatHistory.AddUserMessage(question);
        ChatMessageContent answer = await chat.GetChatMessageContentAsync(
            queryChatHistory,
            cancellationToken: cancellationToken);

        query = answer.Content ?? throw new InvalidOperationException("Failed to get search query");
      
        var answerJson = answer.Content ?? throw new InvalidOperationException("Failed to get search query");
        
        return answerJson;

    }
}
