using OpenAI.Chat;
using System.ClientModel;
using agile_chat_api.Services;
using Azure.AI.OpenAI.Chat;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

public static class ChatCompletionsEndpoint
{
    public class ChatCompletionsEndpointLogger{}
    public static void MapChatCompletionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/chat",
            async (HttpContext context, string? threadId, [FromServices] IAssistantService assistantService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
            {
                #region Service Initialization
                
                IChatThreadService chatThreadService = new ChatThreadService();
                string assistantMessageContent = "";

                #endregion

                try
                {
                    #region Message Validation

                    // Validate and get incoming messages
                    var messages = await ChatService.GetChatMessagesFromContext(context);
                    if (messages == null || !messages.Any())
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("No messages provided.");
                        await context.Response.Body.FlushAsync();
                        return;
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
                        await context.Response.WriteAsync(error);
                        await context.Response.Body.FlushAsync();
                        return;
                    }

                    #endregion

                    #region Message Processing

                    // Process incoming messages and create chat options
                    var options = new ChatCompletionOptions();
                    var oaiMessages = ChatService.GetOaiChatMessages(messages);

                    string userQuery = chatThreadService.GetLatestUserMessageContent(oaiMessages);

                    // Get or create chat thread
                    ChatThread chatThread =
                        chatThreadService.GetOrCreateChatThread(threadId, userQuery, userId, username);

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
                        userId = userId,
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

                    // Stream chat completion responses
                    var completionUpdates = chatClient.CompleteChatStreaming(oaiMessages, options);

                    foreach (var completionUpdate in completionUpdates)
                    {
                        foreach (var contentPart in completionUpdate.ContentUpdate)
                        {
                            var content = contentPart.Text;
                            if (!string.IsNullOrEmpty(content))
                            {
                                assistantMessageContent += content;
                                await context.Response.WriteAsync(content);
                                await context.Response.Body.FlushAsync();
                            }
                        }
                    }

                    #endregion

                    #region Save Assistant Response

                    // Create and save assistant message
                    Message assistantMessage = new Message
                    {
                        threadId = threadId,
                        id = Guid.NewGuid().ToString(),
                        name = username,
                        userId = userId,
                        role = "assistant",
                        sender = "assistant",
                        createdAt = DateTime.UtcNow,
                        isDeleted = false,
                        content = assistantMessageContent,
                    };
                    chatThreadService.CreateChat(assistantMessage);

                    #endregion

                    #region Update Chat Thread

                    // Update chat thread title for new chats
                    if (chatThread.name == "New Chat")
                    {
                        chatThread.lastMessageAt = DateTime.UtcNow;

                        // Update title based on first message
                        if (chatThread.createdAt == chatThread.lastMessageAt)
                        {
                            // Use first few words of the assistant's response as the title
                            string[] words = assistantMessageContent.Split(' ');
                            string newTitle = string.Join(" ", words.Take(5)) + "...";
                            chatThread.assistantTitle =
                                newTitle.Length > 42 ? newTitle.Substring(0, 39) + "..." : newTitle;
                        }

                        chatThreadService.Update(chatThread);
                    }

                    #endregion

                    // Complete the response
                    await context.Response.CompleteAsync();
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