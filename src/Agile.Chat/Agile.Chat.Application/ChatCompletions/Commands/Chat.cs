using System.Security.Claims;
using System.Text;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Application.ChatCompletions.Utils;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Ai;
using Agile.Framework.AzureAiSearch.Interfaces;
using Agile.Framework.AzureAiSearch.Models;
using Agile.Framework.Common.Enums;
using Agile.Framework.Common.EnvironmentVariables;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatCompletions.Commands;

public static class Chat
{
    public record Command(string UserPrompt, string ThreadId) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, 
        IAppKernel appKernel, 
        IHttpContextAccessor contextAccessor, 
        IAssistantsService assistantsService, 
        IChatThreadService chatThreadService, 
        IChatMessageService chatMessageService,
        IAzureAiSearch azureAiSearch) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            //Fetch what's needed to do chatting
            var thread = await chatThreadService.GetItemByIdAsync(request.ThreadId);
            var messages = await chatMessageService.GetAllAsync(thread!.Id);
            var assistant = !string.IsNullOrWhiteSpace(thread.AssistantId)
                ? await assistantsService.GetItemByIdAsync(thread.AssistantId)
                : null;
            
            //Perform RAG if needed
            var hasIndex = !string.IsNullOrWhiteSpace(assistant?.FilterOptions.IndexName);
            var documents = hasIndex
                ? await GetCitationDocumentsAsync(request.UserPrompt, assistant!.FilterOptions.IndexName, thread.FilterOptions)
                : [];
            
            //Execute Chat Stream
            var chatSettings = thread.PromptOptions.ParseAzureOpenAiPromptExecutionSettings();
            var aiStreamChats = hasIndex
                ? appKernel.GetPromptFileChatStream(chatSettings, 
                    Constants.ChatCompletionsPromptsPath + Constants.Prompts.ChatWithRag, 
                    new Dictionary<string, object?>
                {
                    {"documents", string.Join(string.Empty, documents.Select(x => x.ToString()))},
                    {"userPrompt", request.UserPrompt},
                    {"chatHistory", messages.ParseSemanticKernelChatHistoryString()}
                })
                : 
                appKernel.GetChatStream(
                    messages.ParseSemanticKernelChatHistory(), chatSettings);

            var assistantFullResponse = new StringBuilder();
            await foreach (var chatStream in aiStreamChats.WithCancellation(cancellationToken))
            {
                assistantFullResponse.Append(chatStream[ResponseType.Chat]);
                await ChatUtils.WriteToResponseStreamAsync(contextAccessor.HttpContext!, chatStream);
            }
            
            //Write citations object to stream
            if (documents.Count > 0)
                await ChatUtils.WriteToResponseStreamAsync(contextAccessor.HttpContext!,
                    new Dictionary<ResponseType, object>()
                    {
                        {ResponseType.Citations, documents}
                    });
                    
            await SaveUserAndAssistantMessagesAsync(thread.Id, request.UserPrompt, assistantFullResponse.ToString());
            return Results.Ok();
        }

        private async Task<List<Citation>> GetCitationDocumentsAsync(string userPrompt, string indexName, ChatThreadFilterOptions filterOptions)
        {
            var embedding = await appKernel.GenerateEmbeddingAsync(userPrompt);
            var documents = await azureAiSearch.SearchAsync(indexName, 
                new AiSearchOptions(userPrompt, embedding)
                {
                    DocumentLimit = filterOptions.DocumentLimit,
                    Strictness = filterOptions.Strictness
                });
            return documents.Select(x => new Citation
            {
                Chunk = x.Chunk,
                Link = x.Url
            }).ToList();
        }

        private async Task SaveUserAndAssistantMessagesAsync(string threadId, string userPrompt, string assistantResponse)
        {
            var userMessage = Message.CreateUser(threadId, userPrompt, new MessageOptions());
            await chatMessageService.AddItemAsync(userMessage);
            var assistantMessage = Message.CreateAssistant(threadId, assistantResponse, new MessageOptions());
            await chatMessageService.AddItemAsync(assistantMessage);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IChatThreadService chatThreadService, IHttpContextAccessor contextAccessor)
        {
            RuleFor(req => req.UserPrompt)
                .NotNull()
                .NotEmpty()
                .WithMessage("User message cannot be empty");

            RuleFor(req => req.ThreadId)
                .MustAsync(async (threadId, _) => await ValidateUserThreadAsync(contextAccessor, chatThreadService, threadId))
                .WithMessage("Thread not found or invalid access requirements");
        }

        private async Task<bool> ValidateUserThreadAsync(IHttpContextAccessor contextAccessor, IChatThreadService chatThreadService, string threadId)
        {
            var thread = await chatThreadService.GetItemByIdAsync(threadId);
            if (thread is null) return false;
            
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            return thread.UserId.Equals(username, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}