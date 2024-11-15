using System.Text.Json.Nodes;
using agile_chat_api.Services;
using Azure.Storage.Blobs;
using Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Models;
using Services;

namespace agile_chat_api.Endpoints;

public static class IndexesEndpoints
{
    public static void MapIndexesEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/indexes").RequireAuthorization();

        api.MapGet(pattern: string.Empty, [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
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
                   [FromServices] IContainerIndexerService cosmosService,
                [FromServices] IAzureAiSearchService searchService) =>
            {
                if(cosmosService.IndexExistsAsync(request.Name))
                    return Results.BadRequest("Container name already exists.");
                
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return Results.BadRequest("Invalid data. Please provide required fields.");
                }

                try
                {
                    var indexes = await cosmosService.SaveIndexToCosmosDbAsync(request);
                    await searchService.CreateDefaultIndexerAsync(request.Name);
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

        api.MapDelete("delete/{id}", [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
            async (string id, [FromServices] IContainerIndexerService cosmosService, [FromServices] IAzureAiSearchService azureSearchService, [FromServices] IStorageService storageService) =>
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return Results.BadRequest("Invalid ID. Please provide a valid index ID.");
                }

                try
                {
                    var itemDeleted = await cosmosService.DeleteIndexWithRetryAsync(id);
                    if (itemDeleted != null)
                    {
                        await azureSearchService.DeleteIndexAsync(itemDeleted.Name);
                        await storageService.DeleteAllFilesFromIndexAsync(itemDeleted.Name);
                    }
                    
                    return Results.Ok($"Index with ID {id} deleted successfully.");
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    Console.WriteLine($"Rate limit hit for ID {id}. Deletion failed after retries: {ex.Message}");
                    return Results.Problem("Rate limit exceeded. Please try again later.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while deleting the index with ID {id}: {ex.Message}");
                    return Results.Problem("An error occurred while deleting the index. Please check logs for more details.");
                }
            }).DisableAntiforgery();
    }
}