using System.Text.Json.Nodes;
using agile_chat_api.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace agile_chat_api.Endpoints;
public class FileEndpointsLogger { }
public static class FileEndpoints
{
    /// <summary>
    /// Maps the file endpoints.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <returns></returns>
    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/file");

        api.MapPost("webhook", async (HttpContext context, [FromServices] IAzureAiSearchService azureAiSearchService, [FromBody] JsonNode body, ILogger<FileEndpointsLogger> logger) =>
        {
            logger.LogInformation("Validated Authorization for web hook");
            //Validate the webhook handshake
            var eventType = context.Request.Headers["aeg-event-type"].ToString();
            logger.LogDebug("Fetched aeg-event-type {EventType}", eventType);

            if (eventType == "SubscriptionValidation")
            {
                var code = body?.AsArray().FirstOrDefault()?["data"]?["validationCode"]?.ToString();
                logger.LogDebug("Fetched validation code {Code}", code);

                return Results.Ok(new { validationResponse = code });
            }

            var success = await azureAiSearchService.RunIndexer(AzureAiSearchService.FOLDERS_INDEX_NAME);
            return success ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization();

        api.MapGet("folders", async ([FromServices] IBlobStorageService blobStorageService) =>
        {
            var folders = await blobStorageService.GetHighLevelFolders(BlobStorageService.FOLDERS_CONTAINER_NAME);
            return Results.Ok(folders);
        }).RequireAuthorization();


        app.MapPost("/upload", [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
        async (HttpRequest request,
                IFormFileCollection files,
                [FromServices] ICosmosService cosmosService,
                [FromServices] IStorageService blobStorageService) =>
        {
            // Get the folder name from the form data
            var folder = request.Form["folder"].ToString();
            var resultMessages = new List<string>();

            if (files == null || files.Count == 0)
            {
                return Results.BadRequest("No files received for upload.");
            }
            try
            {
                foreach (var file in files)
                {
                    if (file.Length <= 0)
                    {
                        continue; // Skip empty files
                    }

                    bool isFileExistsInCosmosDB = await cosmosService.FileMetadataExistsAsync(file.FileName, folder);
                    bool isFileExistsInStorageService = await blobStorageService.FileExistsInBlobAsync(file.FileName, folder);

                    //Check if file Exists in Cosmos DB
                    if (isFileExistsInCosmosDB) //TRUE
                    {
                        //Check if file Exists in Storage Service
                        if (isFileExistsInStorageService) //TRUE
                        {
                            Console.WriteLine($"File Exists in both CosmosDB & Storage Account.");
                            resultMessages.Add(file.FileName + " already exists. ");
                        }
                        else //FALSE
                        {
                            Console.WriteLine($"File Exists in CosmosDB but not in Storage Account, " +
                                $"we will be creating it in Storage account then and delete existing cosmosDB record " +
                                $"and add it with new record with new id & URL");
                            await cosmosService.DeleteFileByNameFromCosmosAsync(file.FileName, folder);

                            //Resaving it to the cosmos & storage data
                            string blobURL = await blobStorageService.UploadFileToBlobAsync(file, folder);
                            await cosmosService.SaveFileMetadataToCosmosDbAsync(file, blobURL, folder);
                            resultMessages.Add(file.FileName + " uploaded successfully.");
                        }
                    }
                    else //FALSE
                    {
                        if (isFileExistsInStorageService) //TRUE
                        {
                            //File exists in Storage but not in cosmos DB
                            Console.WriteLine($"File Exists in Storage Account but not in CosmosDB, " +
                                $"we will be creating it in CosmosDB record " +
                                $"with the URL from existing record from Storage Account");
                            var blobURL = blobStorageService.GetBlobURLAsync(file.FileName, folder);
                            if (blobURL != null)
                            {
                                await cosmosService.SaveFileMetadataToCosmosDbAsync(file, blobURL, folder);
                            }
                            resultMessages.Add(file.FileName + " uploaded successfully.");
                        }
                        else //FALSE
                        {
                            // If file is in neither storage nor Cosmos DB, upload to storage and save metadata to Cosmos DB
                            Console.WriteLine($"File uploaded on both the CosmosDB and the Storage Account");
                            string blobURL = await blobStorageService.UploadFileToBlobAsync(file, folder);
                            await cosmosService.SaveFileMetadataToCosmosDbAsync(file, blobURL, folder);
                            resultMessages.Add(file.FileName + " uploaded successfully.");
                        }
                    }
                }
                // After processing all files, return the accumulated messages as a popup
                string finalMessage = string.Join(Environment.NewLine, resultMessages);
                return Results.Ok($"{finalMessage}");
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return Results.Problem("An error occurred while processing the upload.");
            }
        }).DisableAntiforgery();

        app.MapGet("/files", async ([FromServices] ICosmosService cosmosService) =>
        {
            try
            {
                var files = await cosmosService.GetFileUploadsAsync();
                return Results.Ok(files);
            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred while retrieving files: {ex.Message}");
            }
        }).RequireAuthorization();

        app.MapDelete("/files", async ([FromServices] ICosmosService cosmosService,
                                       [FromServices] IStorageService blobStorageService,
                                       [FromBody] DeleteFilesRequestDto request) =>
        {
            try
            {
                if (request == null || request.FileIds == null || !request.FileIds.Any())
                {
                    return Results.BadRequest("No file IDs provided for deletion.");
                }

                var fileToDelete = new List<FileMetadata>();
                foreach (var fileId in request.FileIds)
                {
                    var file = await cosmosService.GetFileByIdAsync(fileId);
                    fileToDelete.Add(file!);
                }

                //Delete from Cosmos DB
                await cosmosService.DeleteBulkFileMetadataFromCosmosAsync(request.FileIds);

                var deleteTasks = new List<Task>();
                //Delete from BlobStorage
                foreach (var file in fileToDelete)
                {
                    deleteTasks.Add(blobStorageService.DeleteFileFromBlobAsync(file));
                }
                return Results.Ok("Selected files deleted successfully.");
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return Results.Problem("An error occurred while deleting the files.");
            }
        }
        ).DisableAntiforgery().RequireAuthorization();

    }
}