using System.Text.Json.Serialization;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Shared.DomainHelpers;
using Agile.Chat.Domain.Shared.Interfaces;
using Agile.Chat.Domain.Shared.ValueObjects;
using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.Assistants.Aggregates;

public class Assistant : AuditableAggregateRoot, IAccessControllable
{
    [JsonConstructor]
    private Assistant(string name, string description, AssistantType type, AssistantStatus status, string greeting,
        AssistantFilterOptions filterOptions, AssistantPromptOptions promptOptions, AssistantModelOptions modelOptions,
        PermissionsAccessControl accessControl, AgentConfiguration agentConfiguration = null)
    {
        //Do validation logic and throw domain level exceptions if fails
        Name = name;
        Description = description;
        Type = type;
        Status = status;
        Greeting = greeting;
        FilterOptions = filterOptions;
        PromptOptions = promptOptions;
        ModelOptions = modelOptions;
        AccessControl = accessControl;
        AgentConfiguration = agentConfiguration;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Greeting { get; private set; }
    public AssistantType Type { get; private set; }
    public AssistantStatus Status { get; private set; }
    public AssistantPromptOptions PromptOptions { get; private set; }
    public AssistantFilterOptions FilterOptions { get; private set; }
    public PermissionsAccessControl AccessControl { get; private set; }
    public AssistantModelOptions ModelOptions { get; private set; }
    public AgentConfiguration? AgentConfiguration { get; private set; }

    public static Assistant Create(string name,
        string description,
        string greeting,
        AssistantType type,
        AssistantStatus status,
        AssistantFilterOptions filterOptions,
        AssistantPromptOptions promptOptions,
        AssistantModelOptions modelOptions,
        PermissionsAccessControl? accessControl = null)
    {
        DomainHelpers.NormalizeAccessControl(accessControl);
        return new Assistant(name, description, type, status, greeting, filterOptions,
            promptOptions, modelOptions, accessControl ?? new PermissionsAccessControl());
    }

    public void Update(string name,
        string description,
        string greeting,
        AssistantType type,
        AssistantStatus status,
        AssistantFilterOptions filterOptions,
        AssistantPromptOptions promptOptions,
        AssistantModelOptions modelOptions)
    {
        //Do validation logic and throw domain level exceptions if fails
        Name = name;
        Description = description;
        Type = type;
        Status = status;
        Greeting = greeting;
        FilterOptions = filterOptions;
        PromptOptions = promptOptions;
        ModelOptions = modelOptions;
        LastModified = DateTime.UtcNow;
    }

    public void AddAgentConfiguration(AgentConfiguration? agentConfiguration)
    {
        AgentConfiguration = agentConfiguration;
        LastModified = DateTime.UtcNow;
    }

    public void UpdateModelOptions(AssistantModelOptions modelOptions)
    {
        ModelOptions = modelOptions;
        LastModified = DateTime.UtcNow;
    }

    public void UpdateAccessControl(PermissionsAccessControl accessControl)
    {
        DomainHelpers.NormalizeAccessControl(accessControl);
        AccessControl = accessControl;
        LastModified = DateTime.UtcNow;
    }
}