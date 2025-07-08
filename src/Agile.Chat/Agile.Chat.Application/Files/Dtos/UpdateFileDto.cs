using Microsoft.AspNetCore.Http;

namespace Agile.Chat.Application.Files.Dtos;

public class UpdateFileDto
{
    public string Id { get; set; }
    
    public List<string> Tags { get; set; } = new();
}