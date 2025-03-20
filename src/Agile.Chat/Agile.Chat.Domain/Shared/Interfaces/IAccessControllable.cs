using Agile.Chat.Domain.Shared.ValueObjects;

namespace Agile.Chat.Domain.Shared.Interfaces;

public interface IAccessControllable
{
    public PermissionsAccessControl AccessControl { get; }
}