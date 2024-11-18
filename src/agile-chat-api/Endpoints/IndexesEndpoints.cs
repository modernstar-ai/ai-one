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
    public class IndexesEndpointsLogger{}
    public static void MapIndexesEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/indexes").RequireAuthorization();

        api.MapGet(pattern: string.Empty, [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
            async ([FromServices] IContainerIndexerService cosmosService, [FromServices] ILogger<IndexesEndpointsLogger> logger) =>
            {
                try
                {
                    var indexes = await cosmosService.GetContainerIndexesAsync();
                    return Results.Ok(indexes);
                }
                catch (Exception ex)
                {
                    logger.LogError("Error occurred: {Message} stacktrace: {StackTrace}", ex.Message, ex.StackTrace);
                    return Results.Problem("An error occurred while retrieving indexes.");
                }
            }).DisableAntiforgery();


        api.MapPost("create", [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
            async ([FromBody] IndexesDto request,
                   [FromServices] IContainerIndexerService cosmosService,
                [FromServices] IAzureAiSearchService searchService,
                [FromServices] ILogger<IndexesEndpointsLogger> logger) =>
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
                    logger.LogError("Argument error: {Message} stacktrace: {StackTrace}", ex.Message, ex.StackTrace);
                    return Results.BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    // General error handling
                    logger.LogError("An error occurred while creating the index: {Message} stacktrace: {StackTrace}", ex.Message, ex.StackTrace);
                    return Results.Problem("An error occurred while creating the index. Please check logs for more details.");
                }
            }).DisableAntiforgery();

        api.MapDelete("delete/{id}", [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
            async (string id, [FromServices] IContainerIndexerService cosmosService, [FromServices] 
                IAzureAiSearchService azureSearchService, [FromServices] IFileUploadService fileUploadService, 
                [FromServices] IStorageService storageService,
                [FromServices] ILogger<IndexesEndpointsLogger> logger) =>
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
                        await storageService.DeleteAllFilesFromIndexAsync(itemDeleted.Name);
                        await fileUploadService.DeleteAllFilesInIndexAsync(itemDeleted.Name);
                        await azureSearchService.DeleteIndexAsync(itemDeleted.Name);
                    }
                    
                    return Results.Ok($"Index with ID {id} deleted successfully.");
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    logger.LogError("Rate limit hit for ID {Id}. Deletion failed after retries: {Message}, Stacktrace: {StackTrace}", id, ex.Message, ex.StackTrace);
                    return Results.Problem("Rate limit exceeded. Please try again later.");
                }
                catch (Exception ex)
                {
                    logger.LogError("An error occurred while deleting the index with ID {Id}. Message: {Message}, Stacktrace: {StackTrace}", id, ex.Message, ex.StackTrace);
                    return Results.Problem("An error occurred while deleting the index. Please check logs for more details.");
                }
            }).DisableAntiforgery();
    }
}