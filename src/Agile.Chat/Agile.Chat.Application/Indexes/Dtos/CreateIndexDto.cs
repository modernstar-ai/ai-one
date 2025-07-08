
using Agile.Chat.Domain.Shared.ValueObjects;

namespace Agile.Chat.Application.Indexes.Dtos;

public class CreateIndexDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int ChunkSize { get; set; }
    public int ChunkOverlap { get; set; }
    public PermissionsAccessControl AccessControl { get; set; } = new();
}