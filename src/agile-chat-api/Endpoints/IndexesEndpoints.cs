using System.Text.Json.Nodes;
using agile_chat_api.Services;
using Azure.Storage.Blobs;
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
    }
}