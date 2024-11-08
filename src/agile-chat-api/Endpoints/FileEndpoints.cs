using System.Text.Json.Nodes;
using agile_chat_api.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

namespace agile_chat_api.Endpoints;
public class FileEndpointsLogger{}
public static class FileEndpoints
{
    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/file");
        
        api.MapPost("webhook", async (HttpContext context, [FromServices] IAzureAiSearchService azureAiSearchService, [FromBody] JsonNode body, ILogger<FileEndpointsLogger> logger) =>
        {
            logger.LogInformation("Validated Authorization for web hook");
            //Validate the webhook handshake
            var eventType = context.Request.Headers["aeg-event-type"].ToString();
            logger.LogInformation("Fetched aeg-event-type {EventType}", eventType);

            if (eventType == "SubscriptionValidation")
            {
                var code = body?.AsArray().FirstOrDefault()?["data"]?["validationCode"]?.ToString();
                logger.LogInformation("Fetched validation code {Code}", code);
                
                return Results.Ok(new { validationResponse = code });
            }
            
            var success = await azureAiSearchService.RunIndexer(AzureAiSearchService.FOLDERS_INDEX_NAME);
            return  success ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();
        
        api.MapGet("folders", async ([FromServices] IBlobStorageService blobStorageService) =>
        {
            var folders = await blobStorageService.GetHighLevelFolders(BlobStorageService.FOLDERS_CONTAINER_NAME);
            return Results.Ok(folders);
        }).RequireAuthorization();
        
        app.MapPost("/upload", [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken] async (IFormFileCollection files) =>
        {
            var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_CONNECTION");
            string containerName = "fileuploadsv1";
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    BlobClient blobClient = containerClient.GetBlobClient(fileName);
                    await blobClient.UploadAsync(file.OpenReadStream(), true);
                }
            }

            return Results.Ok("Files uploaded successfully.");
        }).DisableAntiforgery().RequireAuthorization();
    }
}