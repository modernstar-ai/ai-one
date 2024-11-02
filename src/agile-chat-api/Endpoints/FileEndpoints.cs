using Azure.Storage.Blobs;
using System.Runtime.InteropServices;

public static class FileEndpoints
{
    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {

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