using Microsoft.AspNetCore.Mvc;
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
        async (HttpRequest request,
                IFormFileCollection files,
                [FromServices] ICosmosService cosmosService,
                [FromServices] IStorageService blobStorageService) =>
        {
            // Get the folder name from the form data
            var folder = request.Form["folder"].ToString();

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
                        string blobURL = await blobStorageService.UploadFileToBlobAsync(file, folder);

                        // Check if the file already exists in Cosmos DB using ICosmosService & then Upload the same
                        bool fileExists = await cosmosService.FileMetadataExistsAsync(file, blobURL,folder);
                        if (!fileExists)
                        {
                            //Save the file metadata with URL to Cosmos DB
                            await cosmosService.SaveFileMetadataToCosmosDbAsync(file, blobURL, folder);
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
        });

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
                    fileToDelete.Add(file);
                }

                //Delete from Cosmos DB
                await cosmosService.DeleteFileMetadataFromCosmosDbAsync(request.FileIds);

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