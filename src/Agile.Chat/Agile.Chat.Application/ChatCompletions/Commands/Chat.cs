using System.ClientModel;
using System.Security.Claims;
using System.Text;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.Audits.Services;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Application.ChatCompletions.Utils;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Audits.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Ai;
using Agile.Framework.AzureAiSearch.Interfaces;
using Agile.Framework.AzureAiSearch.Models;
using Agile.Framework.Common.Enums;
using Agile.Framework.Common.EnvironmentVariables;
using FluentValidation;
using Mapster;
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
        IAssistantService assistantService, 
        IChatThreadService chatThreadService, 
        IChatMessageService chatMessageService,
        IAzureAiSearch azureAiSearch,
        IAuditService<Message> chatMessageAuditService,
        IAuditService<ChatThread> chatThreadAuditService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            //Fetch what's needed to do chatting
            var thread = await chatThreadService.GetItemByIdAsync(request.ThreadId, ChatType.Thread.ToString());
            var messages = await chatMessageService.GetAllAsync(thread!.Id);
            var assistant = !string.IsNullOrWhiteSpace(thread.AssistantId)
                ? await assistantService.GetItemByIdAsync(thread.AssistantId)
                : null;
            
            //Perform RAG if needed
            var hasIndex = !string.IsNullOrWhiteSpace(assistant?.FilterOptions.IndexName);
            var documents = hasIndex
                ? await GetSearchDocumentsAsync(request.UserPrompt, assistant!.FilterOptions.IndexName, thread.FilterOptions)
                : [];
            
            //Execute Chat Stream
            var chatSettings = thread.PromptOptions.ParseAzureOpenAiPromptExecutionSettings();
            var aiStreamChats = hasIndex
                ? appKernel.GetPromptFileChatStream(chatSettings, 
                    Constants.ChatCompletionsPromptsPath,
                    Constants.Prompts.ChatWithRag, 
                    new Dictionary<string, object?>
                {
                    {"documents", string.Join('\n', documents.Select((x, index) => x.ToString(index + 1)))},
                    {"userPrompt", request.UserPrompt},
                    {"chatHistory", messages.ParseSemanticKernelChatHistory(request.UserPrompt)}
                })
                : 
                appKernel.GetChatStream(
                    messages.ParseSemanticKernelChatHistory(request.UserPrompt), chatSettings);

            var assistantFullResponse = new StringBuilder();
            try
            {
                await foreach (var chatStream in aiStreamChats)
                {
                    assistantFullResponse.Append(chatStream[ResponseType.Chat]);
                    await ChatUtils.WriteToResponseStreamAsync(contextAccessor.HttpContext!, chatStream);
                }
            }
            catch (Exception ex) when (ex is ClientResultException exception && exception.Status == 429)
            {
                return Results.BadRequest("Rate limit exceeded");
            }
            catch (Exception ex) when (ex is ClientResultException exception && exception.Message.Contains("content_filter"))
            {
                return Results.BadRequest("High likelyhood of adult content. Response denied.");
            }


            
            //If its a new chat, update the threads name
            if (messages.Count == 0)
            {
                thread.Update(TruncateUserPrompt(request.UserPrompt), thread.IsBookmarked, thread.PromptOptions, thread.FilterOptions);
                await chatThreadService.UpdateItemByIdAsync(thread.Id, thread, ChatType.Thread.ToString());
            }

            await SaveUserAndAssistantMessagesAsync(thread.Id, request.UserPrompt, assistantFullResponse.ToString(), documents);
            return Results.Empty;
        }
        private string TruncateUserPrompt(string userPrompt) => userPrompt.Substring(0, Math.Min(userPrompt.Length, 39)) +
                                                                (userPrompt.Length <= 39
                                                                    ? string.Empty
                                                                    : "...");

        
        private bool AssistantResponseHasCitations(string assistantResponse) => assistantResponse.Any(c =>
        {
            List<char> chars = ['⁰', '¹', '²', '³', '⁴', '⁵', '⁶', '⁷', '⁸', '⁹'];
            return chars.Contains(c);
        });
        
        private async Task<List<AzureSearchDocument>> GetSearchDocumentsAsync(string userPrompt, string indexName, ChatThreadFilterOptions filterOptions)
        {
            var embedding = await appKernel.GenerateEmbeddingAsync(userPrompt);
            return await azureAiSearch.SearchAsync(indexName, 
                new AiSearchOptions(userPrompt, embedding)
                {
                    DocumentLimit = filterOptions.DocumentLimit,
                    Strictness = filterOptions.Strictness
                });
        }

        private async Task SaveUserAndAssistantMessagesAsync(string threadId, string userPrompt, string assistantResponse, List<AzureSearchDocument>? documents = null)
        {
            var userMessage = Message.CreateUser(threadId, userPrompt, new MessageOptions());
            await chatMessageService.AddItemAsync(userMessage);
            await chatMessageAuditService.AddItemAsync(Audit<Message>.Create(userMessage));
            
            var assistantMessage = Message.CreateAssistant(threadId, assistantResponse, new MessageOptions());
            if(documents?.Count > 0 && AssistantResponseHasCitations(assistantResponse))
                assistantMessage.AddMetadata(MetadataType.Citations, documents.Adapt<List<Citation>>());
            
            await chatMessageService.AddItemAsync(assistantMessage);
            await chatMessageAuditService.AddItemAsync(Audit<Message>.Create(assistantMessage));

            await ChatUtils.WriteToResponseStreamAsync(contextAccessor.HttpContext!, new Dictionary<ResponseType, List<Message>>
            {
                { ResponseType.DbMessages, [userMessage, assistantMessage] }
            });
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
            var thread = await chatThreadService.GetItemByIdAsync(threadId, ChatType.Thread.ToString());
            if (thread is null) return false;
            
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            return thread.UserId.Equals(username, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}