using OpenAI.Chat;
using System.ClientModel;
using agile_chat_api.Services;
using Azure.AI.OpenAI.Chat;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public static class ChatCompletionsEndpoint
{
    public static void MapChatCompletionsEndpoint(this IEndpointRouteBuilder app)
    {

        app.MapPost("/chat", async (HttpContext context, string? threadId) =>
        {
            #region Service Initialization
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
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
                var chatThread = chatThreadService.GetOrCreateChatThread(threadId, userQuery, userId, username);

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
                        chatThread.assistantTitle = newTitle.Length > 30 ?
                            newTitle.Substring(0, 27) + "..." :
                            newTitle;
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


        //app.MapPost("/chat", async (HttpContext context, string? threadId) =>
        //{
        //    IChatThreadService chatThreadService = new ChatThreadService();

        //    // Deserialize the incoming JSON payload into a list of ChatMessage objects
        //    var messages = await ChatService.GetChatMessagesFromContext(context);
        //    if (messages == null || !messages.Any())
        //    {
        //        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        //        await context.Response.WriteAsync("No messages provided.");
        //        await context.Response.Body.FlushAsync();
        //        return;
        //    }

        //    // Extract user information from claims
        //    var user = context.User;
        //    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        //    var username = user.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";

        //    // Set up the necessary headers for SSE
        //    context.Response.Headers.ContentType = "text/event-stream";
        //    context.Response.Headers.CacheControl = "no-cache";
        //    context.Response.Headers.Connection = "keep-alive";

        //    // Get the ChatClient
        //    var chatClient = ChatService.GetChatClient();
        //    if (chatClient == null)
        //    {
        //        const string error = "OpenAI endpoint or API key is not set in environment variables.";
        //        //logger.LogError(error);
        //        // context.Response.StatusCode = 500;
        //        await context.Response.WriteAsync(error);
        //        await context.Response.Body.FlushAsync();
        //        return;
        //    }



        //    //get the options container for RAG and Tools optoins
        //    ChatCompletionOptions options = new ChatCompletionOptions();

        //    //configure RAG
        //    //             var indexName = "gptkbindex";
        //    //             var dataSource = ChatService.GetChatCompletionOptionsDataSource(indexName);
        //    // #pragma warning disable AOAI001
        //    //             options.AddDataSource(dataSource);

        //    // Get the AOAI Messages from the JSON messages
        //    var oaiMessages = ChatService.GetOaiChatMessages(messages);


        //    string userQuery = ChatService.GetLatestUserMessageContent(oaiMessages);

        //    ChatThread chatThread;
        //    chatThread = chatThreadService.GetOrCreateChatThread(threadId, userQuery, userId, username);


        //    // Create new chat message
        //    Message userMessage = new Message
        //    {
        //        threadId = threadId,
        //        id = Guid.NewGuid().ToString(),
        //        name = username,
        //        userId = userId,
        //        role = "user",
        //        createdAt = DateTime.UtcNow,
        //        isDeleted = false,
        //        content = userQuery,
        //    };
        //    chatThreadService.CreateChat(userMessage);

        //    string assistantMessageContent = "";

        //    try
        //    {
        //        // Get streaming completion updates
        //        var completionUpdates = chatClient.CompleteChatStreaming(oaiMessages, options);

        //        // Stream responses as Server-Sent Events (SSE)
        //        foreach (var completionUpdate in completionUpdates)
        //        {
        //            foreach (var contentPart in completionUpdate.ContentUpdate)
        //            {
        //                var content = contentPart.Text;
        //                if (!string.IsNullOrEmpty(content))
        //                {
        //                    //content = content.Replace("\n", "<br>");

        //                    assistantMessageContent += content;

        //                    // Send each chunk of the response to the client as an SSE event
        //                    await context.Response.WriteAsync(content);
        //                    await context.Response.Body.FlushAsync();
        //                }
        //            }
        //        }

        //        // Create new chat message
        //        Message assistantMessage = new Message
        //        {
        //            threadId = threadId,
        //            id = Guid.NewGuid().ToString(),
        //            name = username,
        //            userId = userId,

        //            role = "assistant",
        //            createdAt = DateTime.UtcNow,
        //            isDeleted = false,
        //            content = assistantMessageContent,
        //        };
        //        chatThreadService.CreateChat(assistantMessage);

        //        if (chatThread.name == "New Chat")
        //        {
        //            chatThread.lastMessageAt = DateTime.UtcNow;
        //            // If this is the first message, update the title to be more meaningful

        //            if (chatThread.createdAt == chatThread.lastMessageAt)
        //            {
        //                // Use first few words of the assistant's response as the title
        //                string[] words = assistantMessageContent.Split(' ');
        //                string newTitle = string.Join(" ", words.Take(5)) + "...";
        //                chatThread.assistantTitle = newTitle.Length > 30 ? newTitle.Substring(0, 27) + "..." : newTitle;
        //            }
        //            chatThreadService.Update(chatThread);
        //        }

        //        // Complete the response
        //        await context.Response.CompleteAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception and return an error response
        //        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        //        logger.LogError(ex, "Error processing chat completion request.");

        //        LogService.LogException(ex);

        //        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        //        await context.Response.WriteAsync("An error occurred while processing the request.");
        //        await context.Response.Body.FlushAsync();
        //    }

        //    // Close the response stream
        //    context.Response.Body.Close();

        //    //not required when streaming. it will cause an error
        //    //return Results.Ok();
        //    //return null;
        //}).RequireAuthorization();


//        app.MapPost("/chat", async (HttpContext context) =>
//        {
//            // Deserialize the incoming JSON payload into a list of ChatMessage objects
//            var messages = await ChatService.GetChatMessagesFromContext(context);
//            if (messages == null || !messages.Any())
//            {
//                context.Response.StatusCode = StatusCodes.Status400BadRequest;
//                await context.Response.WriteAsync("No messages provided.");
//                await context.Response.Body.FlushAsync();
//                return;
//            }

//                // Set up the necessary headers for SSE
//                context.Response.Headers.ContentType = "text/event-stream";
//                context.Response.Headers.CacheControl = "no-cache";
//                context.Response.Headers.Connection = "keep-alive";

//            // Get the ChatClient
//            var chatClient = ChatService.GetChatClient();
//            if (chatClient == null)
//            {
//                const string error = "OpenAI endpoint or API key is not set in environment variables.";
//                //logger.LogError(error);
//                // context.Response.StatusCode = 500;
//                await context.Response.WriteAsync(error);
//                await context.Response.Body.FlushAsync();
//                return;
//            }

//            //get the options container for RAG and Tools optoins
//            ChatCompletionOptions options = new ChatCompletionOptions();

//            //configure RAG
////             var indexName = "gptkbindex";
////             var dataSource = ChatService.GetChatCompletionOptionsDataSource(indexName);
//// #pragma warning disable AOAI001
////             options.AddDataSource(dataSource);

//            // Get the AOAI Messages from the JSON messages
//            var oaiMessages = ChatService.GetOaiChatMessages(messages);

//            try
//            {
//                // Get streaming completion updates
//                var completionUpdates = chatClient.CompleteChatStreaming(oaiMessages, options);

//                // Stream responses as Server-Sent Events (SSE)
//                foreach (var completionUpdate in completionUpdates)
//                {
//                    foreach (var contentPart in completionUpdate.ContentUpdate)
//                    {
//                        var content = contentPart.Text;
//                        if (!string.IsNullOrEmpty(content))
//                        {
//                            // Send each chunk of the response to the client as an SSE event
//                            await context.Response.WriteAsync(content);
//                            await context.Response.Body.FlushAsync();
//                        }
//                    }
//                }

//                // Complete the response
//                await context.Response.CompleteAsync();
//            }
//            catch (Exception ex)
//            {
//                // Log the exception and return an error response
//                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//                logger.LogError(ex, "Error processing chat completion request.");
//                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//                await context.Response.WriteAsync("An error occurred while processing the request.");
//                await context.Response.Body.FlushAsync();
//            }

//            // Close the response stream
//            context.Response.Body.Close();

//            //not required when streaming. it will cause an error
//            //return Results.Ok();
//            //return null;
//        }).RequireAuthorization();


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
                        //Console.WriteLine(content);
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