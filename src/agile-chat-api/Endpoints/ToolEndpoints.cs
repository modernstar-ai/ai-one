public static class ToolEndpoints
{
    public static void MapToolEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tools", async (IToolService toolService) =>
        {
            try
            {
                var tools = await Task.Run(() => toolService.GetAll()); // Run synchronously if GetAll isn't async
                return Results.Ok(tools);
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while fetching tools.", statusCode: 500);
            }
        }).RequireAuthorization();

        app.MapGet("/tools/{id:guid}", async (Guid id, IToolService toolService) =>
        {
            try
            {
                var tool = await Task.Run(() => toolService.GetById(id)); // Run synchronously if GetById isn't async
                return tool != null ? Results.Ok(tool) : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while fetching the tool.", statusCode: 500);
            }
        }).RequireAuthorization();

        app.MapPost("/tools", async (Tool tool, IToolService toolService) =>
        {
            try
            {
                toolService.Create(tool); // Assuming Create is synchronous; wrap with Task.Run if needed
                return Results.Created($"/tools/{tool.id}", tool);
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while creating the tool.", statusCode: 500);
            }
        }).RequireAuthorization();

        app.MapPut("/tools/{id:guid}", async (Guid id, Tool updatedTool, IToolService toolService) =>
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
                return Results.Problem("An error occurred while updating the tool.", statusCode: 500);
            }
        }).RequireAuthorization();

        app.MapDelete("/tools/{id:guid}", async (Guid id, IToolService toolService) =>
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
                return Results.Problem("An error occurred while deleting the tool.", statusCode: 500);
            }
        }).RequireAuthorization();
    }
}




//public static class ToolEndpoints
//{
//    public static void MapToolEndpoints(this IEndpointRouteBuilder app)
//    {
//        app.MapGet("/tools", (IToolService toolService) =>
//        {
//            var tools = toolService.GetAll();
//            return Results.Ok(tools);
//        });

//        app.MapGet("/tools/{id:guid}", (Guid id, IToolService toolService) =>
//        {
//            var tool = toolService.GetById(id);
//            return tool != null ? Results.Ok(tool) : Results.NotFound();
//        });

//        app.MapPost("/tools", (Tool tool, IToolService toolService) =>
//        {
//            toolService.Create(tool);
//            return Results.Created($"/tools/{tool.Id}", tool);
//        });

//        app.MapPut("/tools/{id:guid}", (Guid id, Tool updatedTool, IToolService toolService) =>
//        {
//            var existingTool = toolService.GetById(id);
//            if (existingTool is null)
//            {
//                return Results.NotFound();
//            }

//            toolService.Update(id, updatedTool);
//            return Results.NoContent();
//        });

//        app.MapDelete("/tools/{id:guid}", (Guid id, IToolService toolService) =>
//        {
//            var tool = toolService.GetById(id);
//            if (tool is null)
//            {
//                return Results.NotFound();
//            }

//            toolService.Delete(id);
//            return Results.NoContent();
//        });
//    }
//}
