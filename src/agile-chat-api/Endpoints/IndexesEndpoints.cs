using System.Text.Json.Nodes;
using agile_chat_api.Services;
using Azure.Storage.Blobs;
using Dtos;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace agile_chat_api.Endpoints;

public static class IndexesEndpoints
{
    public static void MapIndexesEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/indexes").RequireAuthorization();

        api.MapGet(pattern:"", [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
            async ([FromServices] IContainerIndexerService cosmosService) =>
            {
                try
                {
                    var indexes = await cosmosService.GetContainerIndexesAsync();
                    return Results.Ok(indexes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    return Results.Problem("An error occurred while retrieving indexes.");
                }
            }).DisableAntiforgery();


        api.MapPost("create", [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
            async ([FromBody] IndexesDto request,
                   [FromServices] IContainerIndexerService cosmosService) =>
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return Results.BadRequest("Invalid data. Please provide required fields.");
                }

                try
                {
                    var indexes = await cosmosService.SaveIndexToCosmosDbAsync(request);
                    return Results.Ok(indexes);
                }
                catch (ArgumentNullException ex)
                {
                    // Handle specific case for null argument if needed
                    Console.WriteLine($"Argument error: {ex.Message}");
                    return Results.BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    // General error handling
                    Console.WriteLine($"An error occurred while creating the index: {ex.Message}");
                    return Results.Problem("An error occurred while creating the index. Please check logs for more details.");
                }
            }).DisableAntiforgery();
    }
}