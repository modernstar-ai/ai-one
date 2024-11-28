
namespace Agile.Chat.Application.Indexes.Dtos;

public class CreateIndexDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string? Group { get; set; }
}