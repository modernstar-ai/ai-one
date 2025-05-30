namespace Agile.Framework.Common.EnvironmentVariables.Models;

public class AzureServiceBusConfig
{
    public string Namespace { get; set; }
    public string FullyQualifiedNamespace
    {
        get
        { return $"{Namespace}.servicebus.windows.net"; }
    }

    public string ConnectionString { get; set; }
    public string BlobQueueName { get; set; }
}