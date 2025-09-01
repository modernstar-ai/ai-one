namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class AgentConfiguration
{
    public string AgentId { get; set; }
    public string AgentName { get; set; }
    public string AgentDescription { get; set; }

    public BingConfig BingConfig { get; set; } = new();
    public List<ConnectedAgent> ConnectedAgents { get; set; } = new();
}

public class ConnectedAgent
{
    public string AgentId { get; set; }
    public string AgentName { get; set; }
    public string ActivationDescription { get; set; }
}
