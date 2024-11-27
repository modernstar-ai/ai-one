using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Security.Claims;
using System.Threading;
using static ChatCompletionsEndpoint;

public static class ChatThreadEndpoints
{
    public static void MapChatThreadEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/chat-threads", async (IChatThreadService chatThreadService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
        {
            try
            {
                var threads = await Task.Run(() => chatThreadService.GetAll());
                return Results.Ok(threads);
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                logger.LogError(ex, "Error processing chat completion request.");

                return Results.Problem("An error occurred while fetching chat threads." + ex.Message, statusCode: 500);
            }
        });

        // Get threads by user ID
        app.MapGet("/chat-threads/user/{userId}", async (string userId, IChatThreadService chatThreadService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
        {
            try
            {
                var threads = await Task.Run(() => chatThreadService.GetAllByUserId(userId));
                return Results.Ok(threads);
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                logger.LogError(ex, "Error processing chat completion request.");

                return Results.Problem("An error occurred while fetching user's chat threads." + ex.Message, statusCode: 500);
            }
        });

        app.MapGet("/chat-threads/threads/{threadId}", async (string threadId, IChatThreadService chatThreadService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
        {
            try
            {
                var threads = await Task.Run(() => chatThreadService.GetAllMessagesByThreadId(threadId));
                return Results.Ok(threads);
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                logger.LogError(ex, "Error processing chat completion request.");

                return Results.Problem("An error occurred while fetching thread's chat messages." + ex.Message, statusCode: 500);
            }
        });


        app.MapGet("/chat-threads/{id:guid}", async (string id, IChatThreadService chatThreadService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
        {
            try
            {
                var thread = await Task.Run(() => chatThreadService.GetById(id));
                return thread != null ? Results.Ok(thread) : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while fetching the chat thread." + ex.Message, statusCode: 500);
            }
        });

        app.MapPost("/chat-threads", async (ChatThread thread, IChatThreadService chatThreadService, [FromServices] ILogger < ChatCompletionsEndpointLogger > logger) =>
        {
            try
            {
                await Task.Run(() => chatThreadService.Create(thread, thread.userId));
                return Results.Created($"/chat-threads/{thread.id}", thread);
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                logger.LogError(ex, "Error processing chat completion request.");

                return Results.Problem("An error occurred while creating the chat thread." + ex.Message, statusCode: 500);
            }
        });

        app.MapPut("/chat-threads/{id:guid}", async (string id, ChatThread updatedThread, IChatThreadService chatThreadService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
        {
            try
            {
                var existingThread = await Task.Run(() => chatThreadService.GetById(id));
                if (existingThread is null)
                {
                    return Results.NotFound();
                }

                await Task.Run(() => chatThreadService.Update(updatedThread));
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                logger.LogError(ex, "Error processing chat completion request.");

                return Results.Problem("An error occurred while updating the chat thread." + ex.Message, statusCode: 500);
            }
        });

        app.MapDelete("/chat-threads/{id:guid}", async (string id, string userid, IChatThreadService chatThreadService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
        {
            try
            {
                var thread = await Task.Run(() => chatThreadService.GetById(id));
                if (thread is null)
                {
                    return Results.NotFound();
                }

                await Task.Run(() => chatThreadService.Delete(id, userid));
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                logger.LogError(ex, "Error processing chat completion request.");

                return Results.Problem("An error occurred while deleting the chat thread." + ex.Message, statusCode: 500);
            }
        });



        // Add like
        app.MapPost("/chat-threads/messagereaction/like/{messageId}", async (HttpContext context, string messageId, string userId,
            IChatThreadService chatThreadService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
        {
            try
            { 
                await Task.Run(() => chatThreadService.UpdateMessageReaction(messageId, userId, ReactionType.Like, true));
                return Results.Ok();
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                logger.LogError(ex, "Error processing chat completion request.");
                return Results.Problem("An error occurred while adding like to the message: " + ex.Message, statusCode: 500);
            }
        });

        // Remove like
        app.MapPost("/chat-threads/messagereaction/removelike/{messageId}", async (HttpContext context, string messageId, string userId,
            IChatThreadService chatThreadService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
        {
            try
            {
                await Task.Run(() => chatThreadService.UpdateMessageReaction(messageId, userId, ReactionType.Like, false));
                return Results.Ok();
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                logger.LogError(ex, "Error processing chat completion request.");
                return Results.Problem("An error occurred while removing like from the message: " + ex.Message, statusCode: 500);
            }
        });

        // Add dislike
        app.MapPost("/chat-threads/messagereaction/dislike/{messageId}", async (HttpContext context, string messageId, string userId,
            IChatThreadService chatThreadService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
        {
            try
            {
                await Task.Run(() => chatThreadService.UpdateMessageReaction(messageId, userId, ReactionType.Dislike, true));
                return Results.Ok();
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                logger.LogError(ex, "Error processing chat completion request.");
                return Results.Problem("An error occurred while adding dislike to the message: " + ex.Message, statusCode: 500);
            }
        });

        // Remove dislike
        app.MapPost("/chat-threads/messagereaction/removedislike/{messageId}", async (HttpContext context, string messageId,   string userId,
            IChatThreadService chatThreadService, [FromServices] ILogger<ChatCompletionsEndpointLogger> logger) =>
        {
            try
            {
                await Task.Run(() => chatThreadService.UpdateMessageReaction(messageId, userId, ReactionType.Dislike, false));
                return Results.Ok();
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                logger.LogError(ex, "Error processing chat completion request.");
                return Results.Problem("An error occurred while removing dislike from the message: " + ex.Message, statusCode: 500);
            }
        });

    }

}