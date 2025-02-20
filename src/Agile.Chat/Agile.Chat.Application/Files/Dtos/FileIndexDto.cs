using Agile.Chat.Application.Files.Utils;

namespace Agile.Chat.Application.Files.Dtos;

public class FileIndexDto
{
    public string FileName { get; set; }
    public string IndexName { get; set; }
    public string FolderName { get; set; }
    public EventGridHelpers.FileMetadata FileMetadata { get; set; }
    public EventGridHelpers.Type EventType { get; set; }
}