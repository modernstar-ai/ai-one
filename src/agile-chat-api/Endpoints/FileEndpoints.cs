using System.Text.Json.Nodes;
using agile_chat_api.Dtos;
using agile_chat_api.Services;
using agile_chat_api.Utils;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace agile_chat_api.Endpoints;
public static class FileEndpoints
{
    /// <summary>
    /// Maps the file endpoints.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <returns></returns>
    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/file").RequireAuthorization();

        api.MapPost("webhook", async (HttpContext context, [FromServices] IAzureAiSearchService azureAiSearchService, [FromBody] JsonNode body, ILogger logger) =>
        {
            logger.LogInformation("Validated Authorization for web hook");
            //Validate the webhook handshake
            var eventTypeHeader = context.Request.Headers["aeg-event-type"].ToString();
            logger.LogDebug("Fetched aeg-event-type {EventType}", eventTypeHeader);

            if (eventTypeHeader == "SubscriptionValidation")
            {
                var code = body?.AsArray().FirstOrDefault()?["data"]?["validationCode"]?.ToString();
                logger.LogDebug("Fetched validation code {Code}", code);

                return Results.Ok(new { validationResponse = code });
            }

            var (indexName, folderName) = EventGridHelpers.GetIndexAndFolderName(body);
            logger.LogInformation("Fetched index name {IndexName} folder name {FolderName}", indexName, folderName);

            if (string.IsNullOrWhiteSpace(indexName))
                return Results.BadRequest();
            
            var success = await azureAiSearchService.RunIndexerAsync(indexName);
            return success ? Results.Ok() : Results.BadRequest();

        });

        api.MapGet("folders", async ([FromServices] IStorageService blobStorageService) =>
        {
            var folders = await blobStorageService.GetHighLevelFolders();
            return Results.Ok(folders);
        });


        api.MapPost("upload", [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
        async ([FromForm] FileUploadsDto request,
                [FromServices] ICosmosService cosmosService,
                [FromServices] IStorageService blobStorageService) =>
        {
            // Get the folder name from the form data
            var resultMessages = new List<string>();

            if (request.Files == null || request.Files.Count == 0)
                return Results.BadRequest("No files received for upload.");
            
            try
            {
                foreach (var file in request.Files)
                {
                    if (file.Length <= 0)
                        continue; // Skip empty files

                    bool isFileExistsInCosmosDB = await cosmosService.FileMetadataExistsAsync(file.FileName, request.Folder);
                    bool isFileExistsInStorageService = await blobStorageService.FileExistsInBlobAsync(file.FileName, request.Folder);

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
                            await cosmosService.DeleteFileByNameFromCosmosAsync(file.FileName, request.Folder);

                            //Resaving it to the cosmos & storage data
                            string blobURL = await blobStorageService.UploadFileToBlobAsync(file, request.Folder);
                            await cosmosService.SaveFileMetadataToCosmosDbAsync(file, blobURL, request.Folder);
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
                            var blobURL = blobStorageService.GetBlobURLAsync(file.FileName, request.Folder);
                            if (blobURL != null)
                            {
                                await cosmosService.SaveFileMetadataToCosmosDbAsync(file, blobURL, request.Folder);
                            }
                            resultMessages.Add(file.FileName + " uploaded successfully.");
                        }
                        else //FALSE
                        {
                            // If file is in neither storage nor Cosmos DB, upload to storage and save metadata to Cosmos DB
                            Console.WriteLine($"File uploaded on both the CosmosDB and the Storage Account");
                            string blobURL = await blobStorageService.UploadFileToBlobAsync(file, request.Folder);
                            await cosmosService.SaveFileMetadataToCosmosDbAsync(file, blobURL, request.Folder);
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

        api.MapGet(string.Empty, async ([FromServices] ICosmosService cosmosService) =>
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
        });

        api.MapDelete(string.Empty, async ([FromServices] ICosmosService cosmosService,
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
        ).DisableAntiforgery();

    }
}