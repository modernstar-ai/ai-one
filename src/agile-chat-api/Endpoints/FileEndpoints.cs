using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Models;
using Services;

public static class FileEndpoints
{
    /// <summary>
    /// Maps the file endpoints.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <returns></returns>
    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/upload", [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
        async (IFormFileCollection files, [FromServices] ICosmosService cosmosService, [FromServices] IStorageService blobStorageService) =>
        {
            if (files == null || files.Count == 0)
            {
                return Results.BadRequest("No files received for upload.");
            }
            try
            {
                foreach (var file in files)
                {
                    if(file.Length > 0)
                    {
                        // Upload file to Azure Blob Storage using IBlobStorageService
                        string blobURL = await blobStorageService.UploadFileToBlobAsync(file);

                        // Check if the file already exists in Cosmos DB using ICosmosService & then Upload the same
                        bool fileExists = await cosmosService.FileMetadataExistsAsync(file.FileName,blobURL);
                        if (!fileExists)
                        {
                            //Save the file metadata with URL to Cosmos DB
                            await cosmosService.SaveFileMetadataToCosmosDbAsync(file, blobURL);
                        }
                    }
                }
                return Results.Ok("Files uploaded successfully.");
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return Results.Problem("An error occurred while processing the upload.");
            }
        }).DisableAntiforgery();
    }
}