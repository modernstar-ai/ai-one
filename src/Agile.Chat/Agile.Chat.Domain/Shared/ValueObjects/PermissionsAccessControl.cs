namespace Agile.Chat.Domain.Shared.ValueObjects;

public class PermissionsAccessControl
{
    public AccessLevel ContentManagers { get; set; } = new();
    public AccessLevel Users { get; set; } = new();
}

public class AccessLevel
{
    public bool AllowAccessToAll { get; set; } = true;
    public List<string> UserIds { get; set; } = [];
    public List<string> Groups { get; set; } = [];
}