
using Agile.Chat.Domain.Shared.ValueObjects;

namespace Agile.Chat.Application.Indexes.Dtos;

public class UpdateIndexDto
{
    public string Description { get; set; }

    public PermissionsAccessControl AccessControl { get; set; } = new();
}