using System.Security.Claims;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.Audits.Services;
using Agile.Chat.Application.ChatCompletions.Services;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.Audits.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
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
        IChatMessageService chatMessageService,
        IAzureAIAgentService azureAIAgentService,
        IAuditService<Message> chatMessageAuditService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid().ToString();
            logger.LogDebug("Handler executed {Handler}", typeof(Handler).Namespace);

            var thread = await chatThreadService.GetItemByIdAsync(request.ThreadId, ChatType.Thread.ToString());
            var assistant = await assistantService.GetItemByIdAsync(thread.AssistantId);

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
            var assistantResponse = await azureAIAgentService.GetChatResultAsync
                (request.UserPrompt, contextAccessor.HttpContext, assistant.AgentConfiguration.AgentId,
                thread.AgentThreadConfiguration.AgentThreadId);

            logger.LogDebug("Received response from Azure AI Agent service");
            var userMessage = Message.CreateUser(thread.Id, request.UserPrompt);
            var assistantMessage = Message.CreateAssistant(thread.Id, assistantResponse);

            logger.LogDebug("Updating chat thread with new messages");
            await Task.WhenAll([
                chatMessageService.AddItemAsync(userMessage),
                chatMessageService.AddItemAsync(assistantMessage)]);

            logger.LogDebug("Adding messages to audit");
            await Task.WhenAll([
                chatMessageAuditService.AddItemAsync(Audit<Message>.Create(userMessage)),
                chatMessageAuditService.AddItemAsync(Audit<Message>.Create(assistantMessage))
            ]);

            logger.LogDebug("Saving chat thread updates");
            ////If its a new chat, update the threads name, otherwise just update the last modified date
            thread.Update(chatHistory.Count <= 1 ? TruncateUserPrompt(request.UserPrompt) :
                thread.Name, thread.IsBookmarked, thread.PromptOptions, thread.FilterOptions, thread.ModelOptions);

            await chatThreadService.UpdateItemByIdAsync(thread.Id, thread, ChatType.Thread.ToString());

            return Results.Empty;
        }

        private string TruncateUserPrompt(string userPrompt) => userPrompt.Substring(0, Math.Min(userPrompt.Length, 39)) +
                                                             (userPrompt.Length <= 39
                                                                 ? string.Empty
                                                                 : "...");
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

