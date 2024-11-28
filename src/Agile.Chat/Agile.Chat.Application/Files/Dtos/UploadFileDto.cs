using Microsoft.AspNetCore.Http;

namespace Agile.Chat.Application.Files.Dtos;

public class UploadFileDto
{
    public IFormFile File { get; set; }

    public string IndexName { get; set; }

    public string? FolderName { get; set; }
}