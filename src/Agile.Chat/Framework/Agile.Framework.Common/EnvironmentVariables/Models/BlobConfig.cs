namespace Agile.Framework.Common.EnvironmentVariables.Models;

public class BlobConfig
{
    public string Name { get; set; }
    public string Key { get; set; }

    public string ConnectionString()
    {
        return $"DefaultEndpointsProtocol=https;AccountName={Name};AccountKey={Key};EndpointSuffix=core.windows.net";
    }
}