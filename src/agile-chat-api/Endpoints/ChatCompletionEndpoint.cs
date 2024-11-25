using OpenAI.Chat;
using System.ClientModel;
using agile_chat_api.Services;
using Azure.AI.OpenAI.Chat;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using agile_chat_api.Models;

public static class ChatCompletionsEndpoint
{
    public class ChatCompletionsEndpointLogger { }
    public static void MapChatCompletionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/chat",
            async (HttpContext context, string threadId, [FromServices] IAssistantService assistantService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
            {
                #region Service Initialization

                IChatThreadService chatThreadService = new ChatThreadService();
                string assistantMessageContent = "";
                var responseCitations = new List<Citation>(); // To hold citation objects

                #endregion

                try
                {
                    #region Message Validation

                    // Validate and get incoming messages
                    var messages = await ChatService.GetChatMessagesFromContext(context);
                    if (messages == null || !messages.Any())
                    {
                        return Results.BadRequest("No messages provided.");
                    }

                    #endregion

                    #region User Authentication - Extract user information from claims

                    var user = context.User;
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                    var username = user.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";

                    // Configure Server-Sent Events headers
                    context.Response.Headers.ContentType = "text/event-stream";
                    context.Response.Headers.CacheControl = "no-cache";
                    context.Response.Headers.Connection = "keep-alive";

                    #endregion

                    #region Chat Client Initialization

                    // Initialize and validate chat client
                    var chatClient = ChatService.GetChatClient();
                    if (chatClient == null)
                    {
                        const string error = "OpenAI endpoint or API key is not set in environment variables.";
                        return Results.BadRequest(error);
                    }

                    #endregion

                    #region Message Processing

                    // Process incoming messages and create chat options
                    var options = new ChatCompletionOptions();
                    var oaiMessages = ChatService.GetOaiChatMessages(messages);

                    string userQuery = chatThreadService.GetLatestUserMessageContent(oaiMessages);

                    // Get chat thread 
                    ChatThread chatThread = chatThreadService.GetChatThread(threadId, userQuery, userId, username);

                    // Get the IndexID if appropriate
                    var indexToSearch = string.Empty;
                    var assistantId = chatThread.assistantId;
                    Assistant? assistant = null;
                    if (!string.IsNullOrEmpty(assistantId))
                    {
                        var id = new System.Guid(assistantId);
                        assistant = await assistantService.GetByIdAsync(id);
                        if (assistant != null)
                        {
                            indexToSearch = assistant.Index;
                            options.Temperature = (float?)assistant.Temperature; // 0.5;
                            if (assistant.TopP != null) options.TopP = (float?)assistant.TopP; // 1;
                            options.MaxOutputTokenCount = assistant.MaxResponseToken; // 100;
                        }
                    }


                    // Create and save user message
                    Message userMessage = new Message
                    {
                        threadId = threadId,
                        id = Guid.NewGuid().ToString(),
                        name = username,
                        userId = username,
                        role = "user",
                        createdAt = DateTime.UtcNow,
                        isDeleted = false,
                        content = userQuery,
                        sender = "user"
                    };
                    chatThreadService.CreateChat(userMessage);

                    #endregion

                    #region RAG

                    if (!string.IsNullOrEmpty(indexToSearch))
                    {
                        var dataSource = ChatService.GetChatCompletionOptionsDataSource(indexToSearch);
                        if (dataSource is not null)
                        {
                            //The maximum number of rewritten queries that should be sent to the search
                            //        provider for a single user message.
                            //By default, the system will make an automatic determination.
                            //dataSource.MaxSearchQueries = 1;
                            //The configured strictness of the search relevance filtering. 

                            if (assistant != null)
                            {
                                //Higher strictness will increase precision but lower recall of the answer.
                                dataSource.Strictness = assistant.Strictness;
                                //the configured number of docs to feature in the query
                                dataSource.TopNDocuments = assistant.DocumentLimit;
                            }




#pragma warning disable AOAI001
                            options.AddDataSource(dataSource);
                        }
                    }

                    #endregion

                    #region Chat Completion Streaming

                    AzureChatMessageContext? aggregatedMessageContext = null;
                    var aggregatedCitations = new List<AzureChatCitation>(); // Separate collection for citations

                    // Stream chat completion responses
                    var completionUpdates = chatClient.CompleteChatStreaming(oaiMessages, options);

                    foreach (var completionUpdate in completionUpdates)
                    {
                        var currentMessageContext = completionUpdate.GetAzureMessageContext();
                        // Aggregate the context if it's not null
                        if (currentMessageContext != null)
                        {
                            aggregatedMessageContext ??= currentMessageContext; // Keep the first available message context
                            // Add citations to the separate collection
                            if (currentMessageContext.Citations != null)
                            {
                                aggregatedCitations.AddRange(currentMessageContext.Citations);
                            }
                        }

                        foreach (var contentPart in completionUpdate.ContentUpdate)
                        {
                            var content = contentPart.Text;
                            if (!string.IsNullOrEmpty(content))
                            {
                                assistantMessageContent += content;
                                //await context.Response.WriteAsync(content);
                                //await context.Response.Body.FlushAsync();
                            }
                        }
                    }
                    // At this point, `aggregatedCitations` contains all collected citations
                    if (aggregatedCitations.Any())
                    {
                        var uniqueCitations = aggregatedCitations
                        .Where(citation => !string.IsNullOrEmpty(citation.Url) && !string.IsNullOrEmpty(citation.Title))
                        .GroupBy(citation => new { citation.Title, citation.Url }) // Group by Title and Url to ensure uniqueness
                        .Select(group => group.First()) // Take the first instance of each unique pair
                        .ToList();
                        responseCitations = uniqueCitations.Select(citation => new Citation
                        {
                            FileName = citation.Title,
                            FileUrl = citation.Url
                        }).ToList();
                    }
                    #endregion

                    #region Save Assistant Response

                    // Format responseCitations into a string representation
                    var citationsString = string.Join(Environment.NewLine, responseCitations.Select(citation =>
                        $"{citation.FileName}: {citation.FileUrl}"));
                    // Combine assistant message content with citations
                    var assistantMessageResponse = $"{assistantMessageContent}{Environment.NewLine}{citationsString}";


                    // Create and save assistant message
                    Message assistantMessage = new Message
                    {
                        threadId = threadId,
                        id = Guid.NewGuid().ToString(),
                        name = username,
                        userId = username,
                        role = "assistant",
                        sender = "assistant",
                        createdAt = DateTime.UtcNow,
                        isDeleted = false,
                        content = assistantMessageResponse,
                    };
                    chatThreadService.CreateChat(assistantMessage);

                    #endregion

                    #region Update Chat Thread

                    // Update chat thread title for new chats
                    if (chatThread.name == "New Chat")
                    {
                        chatThread.lastMessageAt = DateTime.UtcNow;

                        if (assistant != null)
                        {
                            chatThread.name = assistant.Name;
                            chatThread.assistantTitle = assistant.Name;
                            chatThread.assistantMessage = assistant.SystemMessage;
                            chatThread.Temperature = assistant.Temperature;
                            chatThread.TopP = assistant.TopP;
                            chatThread.MaxResponseToken = assistant.MaxResponseToken;
                            chatThread.Strictness = assistant.Strictness;
                            chatThread.DocumentLimit = assistant.DocumentLimit;

                        }
                        // Update title based on first message
                        else
                        {
                            // Use first few words of the assistant's response as the title
                            string[] words = assistantMessageContent.Split(' ');
                            string newTitle = string.Join(" ", words.Take(5)) + "...";
                            chatThread.name =
                                newTitle.Length > 42 ? newTitle.Substring(0, 39) + "..." : newTitle;
                        }

                        chatThreadService.Update(chatThread);
                    }

                    #endregion

                    // Create response structure
                    var response = new
                    {
                        response = assistantMessageContent,
                        citations = responseCitations
                    };
                    // Send the response as JSON
                    await context.Response.WriteAsJsonAsync(response);

                }
                catch (Exception ex) when (ex is ClientResultException clientException && clientException.Message.Contains("content_filter"))
                {
                    return Results.BadRequest("High likelyhook of adult profanity");
                }
                catch (Exception ex)
                {
                    #region Error Handling

                    // Log and handle any errors
                    logger.LogError(ex, "Error processing chat completion request.");

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("An error occurred while processing the request.");
                    await context.Response.Body.FlushAsync();

                    #endregion
                }
                finally
                {
                    #region Cleanup

                    // Clean up resources
                    context.Response.Body.Close();

                    #endregion
                }

                return Results.Ok();
            }).RequireAuthorization();


        app.MapPost("/purechat", async (HttpContext context) =>
        {
            // Deserialize the incoming JSON payload into a list of ChatMessage objects
            var messages = await ChatService.GetChatMessagesFromContext(context);
            if (messages == null || !messages.Any())
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("No messages provided.");
                await context.Response.Body.FlushAsync();
                return;
            }

            // Set up the necessary headers for SSE
            context.Response.Headers.Append("Content-Type", "text/event-stream");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

            // Get the ChatClient
            var chatClient = ChatService.GetChatClient();
            if (chatClient == null)
            {
                const string error = "OpenAI endpoint or API key is not set in environment variables.";
                //logger.LogError(error);
                // context.Response.StatusCode = 500;
                await context.Response.WriteAsync(error);
                await context.Response.Body.FlushAsync();
                return;
            }

            // Get the AOAI Messages from the JSON messages
            var oaiMessages = ChatService.GetOaiChatMessages(messages);

            // Get streaming completion updates
            CollectionResult<StreamingChatCompletionUpdate> completionUpdates =
                chatClient.CompleteChatStreaming(oaiMessages);

            // Stream responses as Server-Sent Events (SSE)
            foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
            {
                foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
                {
                    string content = contentPart.Text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        // Send each chunk of the response to the client as an SSE event
                        await context.Response.WriteAsync(content);
                        await context.Response.Body.FlushAsync();
                    }
                }
            }

            // Close the response stream
            context.Response.Body.Close();

            //not required when streaming. it will cause an error
            //return Results.Ok();
            //return null;
        }).RequireAuthorization();
    }
}