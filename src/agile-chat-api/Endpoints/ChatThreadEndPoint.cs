using Microsoft.Azure.Cosmos;
using System.Threading;

public static class ChatThreadEndpoints
{
    public static void MapChatThreadEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/chat-threads", async (IChatThreadService chatThreadService) =>
        {
            try
            {
                var threads = await Task.Run(() => chatThreadService.GetAll());
                return Results.Ok(threads);
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while fetching chat threads." + ex.Message, statusCode: 500);
            }
        });

        // Get threads by user ID
        app.MapGet("/chat-threads/user/{userId}", async (string userId, IChatThreadService chatThreadService) =>
        {
            try
            {
                var threads = await Task.Run(() => chatThreadService.GetAllByUserId(userId));
                return Results.Ok(threads);
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while fetching user's chat threads." + ex.Message, statusCode: 500);
            }
        });



        app.MapGet("/chat-threads/threads/{threadId}", async (string threadId, IChatThreadService chatThreadService) =>
        {
            try
            {
                var threads = await Task.Run(() => chatThreadService.GetAllMessagesByThreadId(threadId));
                return Results.Ok(threads);
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while fetching thread's chat messages." + ex.Message, statusCode: 500);
            }
        });


        app.MapGet("/chat-threads/{id:guid}", async (string id, IChatThreadService chatThreadService) =>
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




        app.MapPost("/chat-threads", async (ChatThread thread, IChatThreadService chatThreadService) =>
        {
            try
            {
                await Task.Run(() => chatThreadService.Create(thread, thread.userId));
                return Results.Created($"/chat-threads/{thread.id}", thread);
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while creating the chat thread." + ex.Message, statusCode: 500);
            }
        });

        app.MapPut("/chat-threads/{id:guid}", async (string id, ChatThread updatedThread, IChatThreadService chatThreadService) =>
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
                return Results.Problem("An error occurred while updating the chat thread." + ex.Message, statusCode: 500);
            }
        });

        app.MapDelete("/chat-threads/{id:guid}", async (string id, string userid, IChatThreadService chatThreadService) =>
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
                return Results.Problem("An error occurred while deleting the chat thread." + ex.Message, statusCode: 500);
            }
        });

        app.MapPost("/chat-threads/{id:guid}/extensions", async (string id, ExtensionUpdate data, IChatThreadService chatThreadService) =>
        {
            try
            {
                var thread = await Task.Run(() => chatThreadService.GetById(id));
                if (thread is null)
                {
                    return Results.NotFound();
                }

                // await Task.Run(() => chatThreadService.AddExtension(data));
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while adding the extension." + ex.Message, statusCode: 500);
            }
        });

        app.MapDelete("/chat-threads/{id:guid}/extensions/{extensionId:guid}", async (string id, Guid extensionId, IChatThreadService chatThreadService) =>
        {
            try
            {
                var thread = await Task.Run(() => chatThreadService.GetById(id));
                if (thread is null)
                {
                    return Results.NotFound();
                }

                //await Task.Run(() => chatThreadService.RemoveExtension(new ExtensionUpdate { ChatThreadId = id, ExtensionId = extensionId }));
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while removing the extension." + ex.Message, statusCode: 500);
            }
        });
    }
}