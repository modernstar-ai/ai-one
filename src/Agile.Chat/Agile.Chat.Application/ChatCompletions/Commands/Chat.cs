using System.ClientModel;
using System.Security.Claims;
using System.Text.Json;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.Audits.Services;
using Agile.Chat.Application.ChatCompletions.Dtos;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Application.ChatCompletions.Plugins;
using Agile.Chat.Application.ChatCompletions.Utils;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Audits.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Ai;
using Agile.Framework.AzureAiSearch;
using Agile.Framework.AzureAiSearch.Models;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ChatType = Agile.Chat.Domain.ChatThreads.ValueObjects.ChatType;
using Constants = Agile.Framework.Common.EnvironmentVariables.Constants;
using Message = Agile.Chat.Domain.ChatThreads.Entities.Message;
using ResponseType = Agile.Framework.Common.Enums.ResponseType;

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
        private ChatContainer _chatContainer;

        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            //Fetch what's needed to do chatting
            var thread = await chatThreadService.GetItemByIdAsync(request.ThreadId, ChatType.Thread.ToString());
            var chatMessages = await chatMessageService.GetAllMessagesAsync(thread!.Id);
            var chatHistory = chatMessages.ParseSemanticKernelChatHistory(request.UserPrompt);
            var assistant = !string.IsNullOrWhiteSpace(thread.AssistantId)
                ? await assistantService.GetItemByIdAsync(thread.AssistantId)
                : null;

            _chatContainer = new ChatContainer
            {
                Thread = thread,
                Assistant = assistant,
                AzureAiSearch = azureAiSearch,
                Messages = chatMessages,
                AppKernel = appKernel
            };

            var hasIndex = !string.IsNullOrWhiteSpace(assistant?.FilterOptions.IndexName);

            if (assistant?.RagType == RagType.Plugin)
            {
                var sp = new ServiceCollection()
                    .AddSingleton<ChatContainer>(_ => _chatContainer)
                    .BuildServiceProvider();
                if (hasIndex)
                    appKernel.AddPlugin<AzureAiSearchRag>(sp);
            }


            //Execute Chat Stream
            var chatSettings = ChatUtils.ParseAzureOpenAiPromptExecutionSettings(assistant, thread);
            IResult assistantResult = assistant?.Type switch
            {
                AssistantType.Chat or null => await GetChatResultAsync(request.UserPrompt, assistant?.PromptOptions?.SystemPrompt, assistant?.FilterOptions, chatSettings, chatHistory),
                AssistantType.Search => await GetSearchResultAsync(request.UserPrompt, assistant?.PromptOptions?.SystemPrompt, assistant?.FilterOptions, chatSettings),
                _ => Results.BadRequest("Unknown chat type")
            };

            if (assistantResult is BadRequest<string> badRequest)
            {
                var (userMessage, assistantMessage, citationMessages) = CreateUserAssistantAndCitationMessages(thread.Id, request.UserPrompt, badRequest.Value ?? string.Empty, null);
                await SaveAuditLogsAsync(userMessage, assistantMessage, citationMessages);
                return badRequest;
            }

            //Get the full response and metadata
            var (assistantResponse, assistantMetadata) = GetAssistantResponseAndMetadata(assistant?.Type, assistantResult);
            await SaveUserAssistantCitationsMessagesAsync(thread.Id, request.UserPrompt, assistantResponse, assistantMetadata);

            //If its a new chat, update the threads name, otherwise just update the last modified date
            thread.Update(chatHistory.Count <= 1 ? TruncateUserPrompt(request.UserPrompt) : thread.Name, thread.IsBookmarked, thread.PromptOptions, thread.FilterOptions);
            await chatThreadService.UpdateItemByIdAsync(thread.Id, thread, ChatType.Thread.ToString());
            return Results.Empty;
        }
        private string TruncateUserPrompt(string userPrompt) => userPrompt.Substring(0, Math.Min(userPrompt.Length, 39)) +
                                                                (userPrompt.Length <= 39
                                                                    ? string.Empty
                                                                    : "...");

        private (string, Dictionary<MetadataType, object>) GetAssistantResponseAndMetadata(AssistantType? assistantType, IResult result)
        {
            var assistantResponse = string.Empty;
            var metadata = new Dictionary<MetadataType, object>();

            switch (assistantType)
            {
                case AssistantType.Chat or null:
                    assistantResponse = (result as Ok<string>)!.Value;
                    break;
                case AssistantType.Search:
                    var searchResponse = (result as Ok<SearchResponseDto>)!.Value;
                    assistantResponse = searchResponse!.AssistantResponse;
                    metadata.Add(MetadataType.SearchProcess, searchResponse.SearchProcess);
                    break;
            }

            if (_chatContainer.Assistant?.RagType == RagType.Plugin && _chatContainer.Citations.Count > 0)
            {
                var citations = _chatContainer.Citations.Cast<ChatContainerCitationExt>()
                    .Where(x => assistantResponse?.Contains("⁽" + ToSuperscript(x.ReferenceNumber) + "⁾") ?? false).ToList();

                if (citations.Count > 0)
                {
                    metadata.Add(MetadataType.Citations, citations.Adapt<List<ChatContainerCitation>>());

                    for (var i = 0; i < citations.Count; i++)
                    {
                        var citation = citations[i];
                        var updatedCitationNumber = i + 1;
                        assistantResponse = assistantResponse!.Replace("⁽" + ToSuperscript(citation.ReferenceNumber) + "⁾", $"⁽{ToSuperscript(updatedCitationNumber)}⁾");
                    }
                }
            }
            else if (_chatContainer.Assistant?.RagType == RagType.AzureSearchChatDataSource)
            {
                var citations = new List<ChatContainerCitation>();
                var fetchedCitations = _chatContainer.Citations.Cast<ChatContainerCitation>().ToList();
                var docIndex = 1;
                for (var i = 0; i < fetchedCitations.Count; i++)
                {
                    if (!assistantResponse!.Contains($"[doc{i + 1}]")) continue;

                    citations.Add(fetchedCitations[i]);
                    assistantResponse = assistantResponse.Replace($"[doc{i + 1}]", $"⁽{ToSuperscript(docIndex)}⁾");
                    docIndex++;
                }
            
                if (citations.Count > 0)
                    metadata.Add(MetadataType.Citations, citations);
            }

            return (assistantResponse!, metadata);
        }

        private string ToSuperscript(int number)
        {
            var map = new Dictionary<char, char>()
            {
                { '0', '⁰' },
                { '1', '¹' },
                { '2', '²' },
                { '3', '³' },
                { '4', '⁴' },
                { '5', '⁵' },
                { '6', '⁶' },
                { '7', '⁷' },
                { '8', '⁸' },
                { '9', '⁹' }
            };
            return number.ToString().Select(x => map[x]).Aggregate("", (x, y) => x + y);
        }

        private async Task<IResult> GetChatResultAsync(string userPrompt, string? assistantSystemPrompt, AssistantFilterOptions? assistantFilterOptions, AzureOpenAIPromptExecutionSettings chatSettings, ChatHistory chatHistory)
        {
            var hasIndex = !string.IsNullOrWhiteSpace(assistantFilterOptions?.IndexName);
            if (!hasIndex)
            {
                var assistantResp = await ChatUtils.StreamAndGetAssistantResponseAsync(contextAccessor.HttpContext!, appKernel.GetChatStream(chatHistory, chatSettings));
                return assistantResp;
            }

            var indexStream = appKernel.GetPromptFileChatStream(chatSettings,
                Constants.ChatCompletionsPromptsPath,
                _chatContainer.Assistant?.RagType == RagType.Plugin ? Constants.Prompts.ChatWithRag : Constants.Prompts.ChatWithAzureSearchDataSource,
                new Dictionary<string, object?>
                {
                    { "assistantSystemPrompt", assistantSystemPrompt },
                    { "userPrompt", userPrompt },
                    { "chatHistory", chatHistory.TakeLast(14).ToList() },
                    { "limitKnowledge", assistantFilterOptions?.LimitKnowledgeToIndex ?? false}
                });

            var assistantFullResponse = await ChatUtils.StreamAndGetAssistantResponseAsync(contextAccessor.HttpContext!, indexStream!, _chatContainer);
            return assistantFullResponse;
        }

        private async Task<IResult> GetSearchResultAsync(string userPrompt, string? assistantSystemPrompt, AssistantFilterOptions? assistantFilterOptions, AzureOpenAIPromptExecutionSettings chatSettings)
        {
            try
            {
#pragma warning disable SKEXP0010
                chatSettings.ResponseFormat = "json_object";

                var aiResponse = await appKernel.GetPromptFileChat(chatSettings,
                    Constants.ChatCompletionsPromptsPath,
                    Constants.Prompts.ChatWithSearch,
                    new Dictionary<string, object?>
                    {
                        {"assistantSystemPrompt", assistantSystemPrompt},
                        { "userPrompt", userPrompt },
                        { "limitKnowledge", assistantFilterOptions?.LimitKnowledgeToIndex ?? false}
                    });
                var searchResponse = JsonSerializer.Deserialize<SearchResponseDto>(aiResponse);
                return TypedResults.Ok(searchResponse);
            }
            catch (Exception ex) when (ex is ClientResultException exception && exception.Status == 429)
            {
                return TypedResults.BadRequest("Rate limit exceeded");
            }
            catch (Exception ex) when (ex is ClientResultException exception && exception.Message.Contains("content_filter"))
            {
                return TypedResults.BadRequest("High likelyhood of adult content. Response denied.");
            }
        }

        private async Task SaveUserAssistantCitationsMessagesAsync(string threadId, string userPrompt, string assistantResponse, Dictionary<MetadataType, object>? assistantMetadata)
        {
            // Create messages
            var (userMessage, assistantMessage, citationMessages) = CreateUserAssistantAndCitationMessages(threadId, userPrompt, assistantResponse, assistantMetadata);

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

        private (Message userMessage, Message assistantMessage, List<Message> citationMessages) CreateUserAssistantAndCitationMessages(string threadId, string userPrompt, string assistantResponse, Dictionary<MetadataType, object>? assistantMetadata)
        {
            var userMessage = Message.CreateUser(threadId, userPrompt);
            var assistantMessage = Message.CreateAssistant(threadId, assistantResponse);
            var citationMessages = new List<Message>();

            if (assistantMetadata != null)
            {
                foreach (var (key, value) in assistantMetadata)
                {
                    assistantMessage.AddMetadata(key, value);
                }
            }

            return (userMessage, assistantMessage, citationMessages);
        }

        private async Task SaveAuditLogsAsync(Message userMessage, Message assistantMessage, List<Message> citationMessages)
        {
            await Task.WhenAll([
                chatMessageAuditService.AddItemAsync(Audit<Message>.Create(userMessage)),
                chatMessageAuditService.AddItemAsync(Audit<Message>.Create(assistantMessage)),
                ..citationMessages.Select(c => chatMessageAuditService.AddItemAsync(Audit<Message>.Create(c)))
            ]);
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