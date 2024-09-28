public static class ToolEndpoints
{
    public static void MapToolEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tools", (IToolService toolService) =>
        {
            var tools = toolService.GetAll();
            return Results.Ok(tools);
        });

        app.MapGet("/tools/{id:guid}", (Guid id, IToolService toolService) =>
        {
            var tool = toolService.GetById(id);
            return tool != null ? Results.Ok(tool) : Results.NotFound();
        });

        app.MapPost("/tools", (Tool tool, IToolService toolService) =>
        {
            toolService.Create(tool);
            return Results.Created($"/tools/{tool.Id}", tool);
        });

        app.MapPut("/tools/{id:guid}", (Guid id, Tool updatedTool, IToolService toolService) =>
        {
            var existingTool = toolService.GetById(id);
            if (existingTool is null)
            {
                return Results.NotFound();
            }

            toolService.Update(id, updatedTool);
            return Results.NoContent();
        });

        app.MapDelete("/tools/{id:guid}", (Guid id, IToolService toolService) =>
        {
            var tool = toolService.GetById(id);
            if (tool is null)
            {
                return Results.NotFound();
            }

            toolService.Delete(id);
            return Results.NoContent();
        });
    }
}
