using agile_chat_api.Authentication;
using Microsoft.AspNetCore.Mvc;

public static class AssistantEndpoints
{
    public class AssistantEndpointsLogger {}
    public static void MapAssistantEndpoints(this IEndpointRouteBuilder app)
    {
        var assistants = app.MapGroup("/api/assistants").RequireAuthorization();

        assistants.MapGet("/", async ([FromServices] IAssistantService assistantService, [FromServices] ILogger<AssistantEndpointsLogger> logger) =>
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

        assistants.MapGet("/{id:guid}", async (Guid id, [FromServices] IAssistantService assistantService, [FromServices] ILogger<AssistantEndpointsLogger> logger) =>
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

        assistants.MapPost("/", async (Assistant assistant, [FromServices] IAssistantService assistantService, [FromServices] ILogger<AssistantEndpointsLogger> logger) =>
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

        assistants.MapPut("/{id:guid}",
            async (Guid id, Assistant updatedAssistant, [FromServices] IRoleService roleService, [FromServices] IAssistantService assistantService, [FromServices] ILogger<AssistantEndpointsLogger> logger) =>
            {
                try
                {
                    var existingAssistant =
                        await assistantService.GetByIdAsync(id);
                    if (existingAssistant is null)
                    {
                        return Results.NotFound();
                    }

                    if (!string.IsNullOrWhiteSpace(existingAssistant.Group) &&
                        !roleService.IsUserInRole(UserRole.ContentManager, existingAssistant.Group))
                        return Results.Forbid();

                    await assistantService.UpdateAsync(updatedAssistant);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while updating the assistant.");
                    return Results.Problem("An error occurred while updating the assistant.", statusCode: 500);
                }
            });

        assistants.MapDelete("/{id:guid}", async (Guid id, [FromServices] IRoleService roleService, [FromServices] IAssistantService assistantService, [FromServices] ILogger<AssistantEndpointsLogger> logger) =>
        {
            try
            {
                var assistant = await assistantService.GetByIdAsync(id);
                if (assistant is null)
                {
                    return Results.NotFound();
                }
                if (!string.IsNullOrWhiteSpace(assistant.Group) &&
                    !roleService.IsUserInRole(UserRole.ContentManager, assistant.Group))
                    return Results.Forbid();

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