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

        api.MapGet(string.Empty, async ([FromServices] IAzureAiSearchService azureAiSearchService) =>
        {
            var indexes = await azureAiSearchService.GetAllIndexes();
            return Results.Ok(indexes);
        });


        api.MapPost("create", [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
        async ([FromBody] IndexesDto request ,
               [FromServices] IContainerIndexerService cosmosService) =>
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest("Invalid data. Please provide required fields.");
            }

            var isSaved = await cosmosService.SaveIndexToCosmosDbAsync(request);
            return isSaved != false ? Results.Ok() : Results.Problem("An error occurred while creating.");
        }).DisableAntiforgery();
    }
}