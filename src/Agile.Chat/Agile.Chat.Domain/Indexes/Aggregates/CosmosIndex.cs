using System.Text.Json.Serialization;
using Agile.Chat.Domain.Shared.Interfaces;
using Agile.Chat.Domain.Shared.ValueObjects;
using Agile.Chat.Domain.Indexes.ValueObjects;
using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.Indexes.Aggregates;

public class CosmosIndex : AuditableAggregateRoot, IAccessControllable
{
    [JsonConstructor]
    private CosmosIndex(string name, string description, int chunkSize, int chunkOverlap, PermissionsAccessControl accessControl, List<TaggingSettings>? taggingSettings)
    {
        Name = name;
        Description = description;
        AccessControl = accessControl;
        ChunkSize = chunkSize;
        ChunkOverlap = chunkOverlap;
        TaggingSettings = taggingSettings;
    }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int ChunkSize { get; private set; }
    public int ChunkOverlap { get; private set; }
    public PermissionsAccessControl AccessControl { get; private set; }

    public List<TaggingSettings>? TaggingSettings { get; set; }
    public static CosmosIndex Create(string name,
        string description,
        int chunkSize,
        int chunkOverlap,
        PermissionsAccessControl? accessControl,
        List<TaggingSettings>? taggingSettings)
    {
        NormalizeAccessControl(accessControl);
        //Do validation logic and throw domain level exceptions if fails
        return new CosmosIndex(name, description, chunkSize, chunkOverlap, accessControl ?? new PermissionsAccessControl(), taggingSettings ?? new List<TaggingSettings>());
    }

    public void Update(string description, List<TaggingSettings>? taggingSettings)
    {
        //Do validation logic and throw domain level exceptions if fails
        Description = description;
        LastModified = DateTime.UtcNow;
        TaggingSettings = taggingSettings ?? new List<TaggingSettings>();
    }

    private static void NormalizeAccessControl(PermissionsAccessControl? accessControl)
    {
        if (accessControl == null)
            return;

        // Normalize all user IDs and group names to lowercase so that cosmos DB queries are case-insensitive
        accessControl.Users.UserIds = accessControl.Users.UserIds
        .Select(id => id.ToLowerInvariant())
        .ToList();

        accessControl.Users.Groups = accessControl.Users.Groups
            .Select(g => g.ToLowerInvariant())
            .ToList();

        accessControl.ContentManagers.UserIds = accessControl.ContentManagers.UserIds
            .Select(id => id.ToLowerInvariant())
            .ToList();

        accessControl.ContentManagers.Groups = accessControl.ContentManagers.Groups
            .Select(g => g.ToLowerInvariant())
            .ToList();
    }

    public void UpdateAccessControl(PermissionsAccessControl accessControl)
    {
        NormalizeAccessControl(accessControl);
        AccessControl = accessControl;
        LastModified = DateTime.UtcNow;
    }
}