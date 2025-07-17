using System.Security.Claims;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.Audits.Services;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Application.Services;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Framework.Ai;
using Agile.Framework.AzureAiSearch;
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
        IAzureAiSearch azureAiSearch,
        IChatThreadFileService chatThreadFileService,
        IAzureAIAgentService azureAIAgentService,
        IAppKernelBuilder appKernelBuilder,
        IAuditService<Message> chatMessageAuditService,
        IAuditService<ChatThread> chatThreadAuditService) : IRequestHandler<Command, IResult>
    {
        private ChatContainer _chatContainer = null!;
        private IAppKernel _appKernel = null!;

        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var thread = await chatThreadService.GetItemByIdAsync(request.ThreadId, ChatType.Thread.ToString());
            var chatMessages = await chatMessageService.GetAllMessagesAsync(thread!.Id);
            var files = await chatThreadFileService.GetAllAsync(request.ThreadId);
            var chatHistory = chatMessages.ParseSemanticKernelChatHistory(request.UserPrompt);
            var assistant = !string.IsNullOrWhiteSpace(thread.AssistantId)
                ? await assistantService.GetItemByIdAsync(thread.AssistantId)
                : null;

            //_chatContainer = new ChatContainer
            //{
            //    UserPrompt = request.UserPrompt,
            //    Thread = thread,
            //    Assistant = assistant,
            //    AzureAiSearch = azureAiSearch,
            //    Messages = chatMessages,
            //    AppKernel = _appKernel,
            //    ThreadFiles = files
            //};


            var assistantResponse = await azureAIAgentService.GetChatResultAsync
                (request.UserPrompt, contextAccessor.HttpContext, assistant.AgentConfiguration.AgentId,
                thread.AgentThreadConfiguration.AgentThreadId);

            //var agentThread = await GetOrCreateAgentThreadAsync(threadId);

            //var sp = new ServiceCollection()
            //    .AddSingleton<ChatContainer>(_ => _chatContainer)
            //    .BuildServiceProvider();
            //if (!string.IsNullOrWhiteSpace(assistant?.FilterOptions.IndexName))
            //    _appKernel.AddPlugin<AzureAiSearchRag>(sp);


            ////Execute Chat Stream
            //var chatSettings = ChatUtils.ParseAzureOpenAiPromptExecutionSettings(assistant, thread);
            //IResult assistantResult = assistant?.Type switch
            //{
            //    AssistantType.Chat or null => await GetChatResultAsync(chatSettings, chatHistory),
            //    AssistantType.Search => await GetSearchResultAsync(chatSettings),
            //    _ => Results.BadRequest("Unknown chat type")
            //};

            //if (assistantResult is BadRequest<string> badRequest)
            //{
            //    var (userMessage, assistantMessage, citationMessages) = CreateUserAssistantAndCitationMessages(badRequest.Value ?? string.Empty, null);
            //    await SaveAuditLogsAsync(userMessage, assistantMessage, citationMessages);
            //    return badRequest;
            //}

            ////Get the full response and metadata
            //var (assistantResponse, assistantMetadata) = GetAssistantResponseAndMetadata(assistant?.Type, assistantResult);
            //await SaveUserAssistantCitationsMessagesAsync(assistantResponse, assistantMetadata);

            ////If its a new chat, update the threads name, otherwise just update the last modified date
            //thread.Update(chatHistory.Count <= 1 ? TruncateUserPrompt(request.UserPrompt) :
            //    thread.Name, thread.IsBookmarked, thread.PromptOptions, thread.FilterOptions, thread.ModelOptions);

            //await chatThreadService.UpdateItemByIdAsync(thread.Id, thread, ChatType.Thread.ToString());
            return Results.Empty;
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

