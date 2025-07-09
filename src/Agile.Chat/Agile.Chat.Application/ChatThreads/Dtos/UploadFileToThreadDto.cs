using Microsoft.AspNetCore.Http;

namespace Agile.Chat.Application.ChatThreads.Dtos;

public class UploadFileToThreadDto
{
    public IFormFile File { get; set; }
}