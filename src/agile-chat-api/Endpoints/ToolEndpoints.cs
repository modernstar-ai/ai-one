using Microsoft.AspNetCore.Mvc;

public static class ToolEndpoints
{
    public class ToolEndpointsLogger {}
    public static void MapToolEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tools", async (IToolService toolService, [FromServices] ILogger<ToolEndpointsLogger> logger) =>
        {
            try
            {
                var tools = await Task.Run(() => toolService.GetAll()); 
                return Results.Ok(tools);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching tools.");
                return Results.Problem("An error occurred while fetching tools.", statusCode: 500);
            }
        }).RequireAuthorization();

        app.MapGet("/tools/{id:guid}", async (Guid id, IToolService toolService, [FromServices] ILogger<ToolEndpointsLogger> logger) =>
        {
            try
            {
                var tool = await Task.Run(() => toolService.GetById(id)); 
                return tool != null ? Results.Ok(tool) : Results.NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching the tool.");
                return Results.Problem("An error occurred while fetching the tool.", statusCode: 500);
            }
        }).RequireAuthorization();

        app.MapPost("/tools", (Tool tool, IToolService toolService, [FromServices] ILogger<ToolEndpointsLogger> logger) =>
        {
            try
            {
                toolService.Create(tool); 
                return Task.FromResult(Results.Created($"/tools/{tool.id}", tool));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating the tool.");
                return Task.FromResult(Results.Problem("An error occurred while creating the tool.", statusCode: 500));
            }
        }).RequireAuthorization();

        app.MapPut("/tools/{id:guid}", async (Guid id, Tool updatedTool, IToolService toolService, [FromServices] ILogger<ToolEndpointsLogger> logger) =>
        {
            try
            {
                var existingTool = await Task.Run(() => toolService.GetById(id)); // Synchronous if GetById isn't async
                if (existingTool is null)
                {
                    return Results.NotFound();
                }

                toolService.Update(updatedTool); // Assuming Update is synchronous; wrap with Task.Run if needed
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating the tool.");
                return Results.Problem("An error occurred while updating the tool.", statusCode: 500);
            }
        }).RequireAuthorization();

        app.MapDelete("/tools/{id:guid}", async (Guid id, IToolService toolService, [FromServices] ILogger<ToolEndpointsLogger> logger) =>
        {
            try
            {
                var tool = await Task.Run(() => toolService.GetById(id)); // Synchronous if GetById isn't async
                if (tool is null)
                {
                    return Results.NotFound();
                }

                toolService.Delete(tool.id); // Assuming Delete is synchronous; wrap with Task.Run if needed
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting the tool.");
                return Results.Problem("An error occurred while deleting the tool.", statusCode: 500);
            }
        }).RequireAuthorization();
    }
}


