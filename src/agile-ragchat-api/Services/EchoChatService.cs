// Copyright (c) Microsoft. All rights reserved.

using Azure.Core;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;

namespace MinimalApi.Services;
public class EchoChatService
{
      
    public EchoChatService()
    {
       

    }

    public async Task<string> ReplyAsync(
        ChatMessage[] history,
        string? prompt,
        RequestOverrides? overrides,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult($"thanks! '{prompt}'");
    }
}
