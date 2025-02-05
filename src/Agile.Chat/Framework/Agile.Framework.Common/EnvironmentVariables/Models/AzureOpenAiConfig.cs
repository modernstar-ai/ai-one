namespace Agile.Framework.Common.EnvironmentVariables.Models;

public class AzureOpenAiConfig
{
    public AzureOpenAiApimConfig Apim { get; set; } = new();

    private string _endpoint = string.Empty;
    public string Endpoint
    {
        get
        {
            if(!string.IsNullOrEmpty(Apim.Endpoint))
                return Apim.Endpoint;
            return _endpoint;
        }
        set
        {
            _endpoint = value;
        }
    }

    public string? ApiKey { get; set; }
    public string? ApiVersion { get; set; }
    public string? InstanceName { get; set; }
    public string? DeploymentName { get; set; }
    public string? EmbeddingsDeploymentName { get; set; }
    public string EmbeddingsModelName { get; set; }
}

public class AzureOpenAiApimConfig
{
    public string? Endpoint { get; set; }
}