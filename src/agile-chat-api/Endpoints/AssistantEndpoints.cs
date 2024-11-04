using Microsoft.AspNetCore.Mvc;

public static class AssistantEndpoints
{
    public static void MapAssistantEndpoints(this IEndpointRouteBuilder app)
    {

        app.MapGet("/assistants", async ([FromServices] IAssistantService assistantService, [FromServices] ILogger logger) =>
        {
            try
            {
                var assistants =
                    await assistantService.GetAllAsync();
                return Results.Ok(assistants);
            }
            catch (Exception ex)
            {
                var error = "An error occurred while retrieving all assistants.";
                logger.LogError(ex, error);
                return Results.Problem(error, statusCode: 500);
            }
        });

        app.MapGet("/assistants/{id:guid}", async (Guid id, [FromServices] IAssistantService assistantService, [FromServices] ILogger logger) =>
        {
            try
            {
                var assistant =
                    await assistantService.GetByIdAsync(id);
                return assistant != null ? Results.Ok(assistant) : Results.NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching the assistant.");
                return Results.Problem("An error occurred while fetching the assistant.", statusCode: 500);
            }
        });

        app.MapPost("/assistants", async (Assistant assistant, [FromServices] IAssistantService assistantService, [FromServices] ILogger logger) =>
        {
            try
            {
                await assistantService.CreateAsync(assistant);
                return Results.Created($"/assistants/{assistant.Id}", assistant);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating the assistant.");
                return Results.Problem("An error occurred while creating the assistant.", statusCode: 500);
            }
        });

        app.MapPut("/assistants/{id:guid}",
            async (Guid id, Assistant updatedAssistant, [FromServices] IAssistantService assistantService, [FromServices] ILogger logger) =>
            {
                try
                {
                    var existingAssistant =
                        await assistantService.GetByIdAsync(id);
                    if (existingAssistant is null)
                    {
                        return Results.NotFound();
                    }

                    await assistantService.UpdateAsync(updatedAssistant);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while updating the assistant.");
                    return Results.Problem("An error occurred while updating the assistant.", statusCode: 500);
                }
            });

        app.MapDelete("/assistants/{id:guid}", async (Guid id, [FromServices] IAssistantService assistantService, [FromServices] ILogger logger) =>
        {
            try
            {
                var assistant = await assistantService.GetByIdAsync(id);
                if (assistant is null)
                {
                    return Results.NotFound();
                }

                await assistantService.DeleteAsync(assistant.Id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting the assistant.");
                return Results.Problem("An error occurred while deleting the assistant.", statusCode: 500);
                
            }
        });
    }
}