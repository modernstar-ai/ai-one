using System.Text.Json.Serialization;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Shared.Interfaces;
using Agile.Chat.Domain.Shared.ValueObjects;
using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.Assistants.Aggregates;

public class Assistant : AuditableAggregateRoot, IAccessControllable
{
    [JsonConstructor]
    private Assistant(string name, string description, AssistantType type, RagType ragType, AssistantStatus status, string greeting, AssistantFilterOptions filterOptions, AssistantPromptOptions promptOptions, PermissionsAccessControl accessControl)
    {
        //Do validation logic and throw domain level exceptions if fails
        Name = name;
        Description = description;
        Type = type;
        RagType = ragType;
        Status = status;
        Greeting = greeting;
        FilterOptions = filterOptions;
        PromptOptions = promptOptions;
        AccessControl = accessControl;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Greeting { get; private set; }
    public AssistantType Type { get; private set; }
    public RagType RagType { get; private set; }
    public AssistantStatus Status { get; private set; }
    public AssistantPromptOptions PromptOptions { get; private set; }
    public AssistantFilterOptions FilterOptions { get; private set; }
    public PermissionsAccessControl AccessControl { get; private set; }

    public static Assistant Create(string name,
        string description,
        string greeting,
        AssistantType type,
        RagType ragType,
        AssistantStatus status,
        AssistantFilterOptions filterOptions,
        AssistantPromptOptions promptOptions,
        PermissionsAccessControl? accessControl = null)
    {
        return new Assistant(name, description, type, ragType, status, greeting, filterOptions, promptOptions, accessControl ?? new PermissionsAccessControl());
    }

    public void Update(string name,
        string description,
        string greeting,
        AssistantType type,
        RagType ragType,
        AssistantStatus status, 
        AssistantFilterOptions filterOptions,
        AssistantPromptOptions promptOptions)
    {
        //Do validation logic and throw domain level exceptions if fails
        Name = name;
        Description = description;
        Type = type;
        RagType = ragType;
        Status = status;
        Greeting = greeting;
        FilterOptions = filterOptions;
        PromptOptions = promptOptions; 
        LastModified = DateTime.UtcNow;
    }

    public void UpdateAccessControl(PermissionsAccessControl accessControl)
    {
        NormalizeAccessControl(accessControl);
        AccessControl = accessControl;
        LastModified = DateTime.UtcNow;
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
}