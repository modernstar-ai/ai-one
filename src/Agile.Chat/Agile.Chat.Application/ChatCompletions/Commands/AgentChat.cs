using System.Security.Claims;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.Audits.Services;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Application.ChatCompletions.Services;
using Agile.Chat.Application.ChatCompletions.Utils;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.Audits.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Ai;
using Agile.Framework.AzureAiSearch;
using Agile.Framework.Common.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ChatType = Agile.Chat.Domain.ChatThreads.ValueObjects.ChatType;
using Message = Agile.Chat.Domain.ChatThreads.Entities.Message;

namespace Agile.Chat.Application.ChatCompletions.Commands;

public static class AgentChat
{
    public record Command(string UserPrompt, string ThreadId) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger,
        IHttpContextAccessor contextAccessor,
        IAssistantService assistantService,
        IChatThreadService chatThreadService,
        IChatThreadFileService chatThreadFileService,
        IChatMessageService chatMessageService,
        IAzureAIAgentService azureAIAgentService,
        IAppKernelBuilder appKernelBuilder,
        IAzureAiSearch azureAiSearch,
        IAuditService<Message> chatMessageAuditService) : IRequestHandler<Command, IResult>
    {
        private ChatContainer _agentContainer;
        
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid().ToString();
            logger.LogDebug("Handler executed {Handler}", typeof(Handler).Namespace);

            var thread = await chatThreadService.GetItemByIdAsync(request.ThreadId, ChatType.Thread.ToString());
            var chatMessages = await chatMessageService.GetAllMessagesAsync(thread!.Id);
            var files = await chatThreadFileService.GetAllAsync(request.ThreadId);
            var assistant = await assistantService.GetItemByIdAsync(thread.AssistantId);

            _agentContainer = new ChatContainer()
            {
                AppKernel = ChatUtils.GetAppKernel(appKernelBuilder, thread.ModelOptions?.ModelId),
                Thread = thread,
                ThreadFiles = files,
                Messages = chatMessages,
                UserPrompt = request.UserPrompt,
                Assistant = assistant,
                AzureAiSearch = azureAiSearch,
                Citations = new(),
                AgentCitations = new List<AgentCitation>(),
            };

            using (logger.BeginScope(new
            {
                ChatRequestId = requestId,
                AssistantId = assistant.Id,
                ThreadId = thread.Id,
                AgentThreadId = thread.AgentThreadConfiguration.AgentThreadId,
            }))
            {
                return await HandleAsync(request, requestId, assistant, thread, cancellationToken);
            }
        }

        public async Task<IResult> HandleAsync(Command request, string requestId, Assistant assistant, ChatThread thread, CancellationToken cancellationToken)
        {
            var chatMessages = await chatMessageService.GetAllMessagesAsync(thread!.Id);
            var chatHistory = chatMessages.ParseSemanticKernelChatHistory(request.UserPrompt);

            logger.LogDebug("Calling Azure AI Agent service");
            var resp = await azureAIAgentService.GetChatResultAsync
                (request.UserPrompt, contextAccessor.HttpContext, assistant.AgentConfiguration.AgentId,
                thread.AgentThreadConfiguration.AgentThreadId,
                _agentContainer);
            var (assistantResponse, metadata) = GetAssistantResponseAndMetadata(resp);

            logger.LogDebug("Received response from Azure AI Agent service");
            await SaveUserAssistantCitationsMessagesAsync(assistantResponse, metadata);

            logger.LogDebug("Saving chat thread updates");
            //If its a new chat, update the threads name, otherwise just update the last modified date
            thread.Update(chatHistory.Count <= 1 ? ChatUtils.TruncateUserPrompt(request.UserPrompt) :
                thread.Name, thread.IsBookmarked, thread.PromptOptions, thread.FilterOptions, thread.ModelOptions);

            await chatThreadService.UpdateItemByIdAsync(thread.Id, thread, ChatType.Thread.ToString());

            return Results.Empty;
        }
        
        private (string, Dictionary<MetadataType, object>) GetAssistantResponseAndMetadata(string response)
        {
            var assistantResponse = response;
            var metadata = new Dictionary<MetadataType, object>();

            var (finalResponse, citations) = ChatUtils.ReplaceCitationText(assistantResponse, _agentContainer);

            if (citations.Count > 0)
                metadata.Add(MetadataType.Citations, citations);

            return (finalResponse!, metadata);
        }
        
        private async Task SaveUserAssistantCitationsMessagesAsync(string assistantResponse, Dictionary<MetadataType, object>? assistantMetadata)
        {
            // Create messages
            var (userMessage, assistantMessage, citationMessages) = CreateUserAssistantAndCitationMessages(assistantResponse, assistantMetadata);

            // Save messages 
            await Task.WhenAll([
                chatMessageService.AddItemAsync(userMessage),
                chatMessageService.AddItemAsync(assistantMessage),
                ..citationMessages.Select(m => chatMessageService.AddItemAsync(m))]);

            // Save audit logs
            await SaveAuditLogsAsync(userMessage, assistantMessage, citationMessages);

            // Write to response stream
            await ChatUtils.WriteToResponseStreamAsync(
                contextAccessor.HttpContext!,
                ResponseType.DbMessages,
                new List<Message> { userMessage, assistantMessage });
        }
        
        private async Task SaveAuditLogsAsync(Message userMessage, Message assistantMessage, List<Message> citationMessages)
        {
            await Task.WhenAll([
                chatMessageAuditService.AddItemAsync(Audit<Message>.Create(userMessage)),
                chatMessageAuditService.AddItemAsync(Audit<Message>.Create(assistantMessage)),
                ..citationMessages.Select(c => chatMessageAuditService.AddItemAsync(Audit<Message>.Create(c)))
            ]);
        }
        
        private (Message userMessage, Message assistantMessage, List<Message> citationMessages) CreateUserAssistantAndCitationMessages(string assistantResponse, Dictionary<MetadataType, object>? assistantMetadata)
        {
            var userMessage = Message.CreateUser(_agentContainer.Thread.Id, _agentContainer.UserPrompt);
            var assistantMessage = Message.CreateAssistant(_agentContainer.Thread.Id, assistantResponse);
            var citationMessages = assistantMetadata?.TryGetValue(MetadataType.Citations, out var citations) ?? false ?
                (citations as List<ChatContainerCitation>)!.Select(c => Message.CreateCitation(_agentContainer.Thread.Id, c.Content)).ToList() : [];

            if (assistantMetadata != null)
            {
                foreach (var (key, value) in assistantMetadata)
                {
                    assistantMessage.AddMetadata(key, value);
                }
            }

            return (userMessage, assistantMessage, citationMessages);
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

