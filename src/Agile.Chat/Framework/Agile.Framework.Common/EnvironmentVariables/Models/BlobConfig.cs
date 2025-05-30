namespace Agile.Framework.Common.EnvironmentVariables.Models;

public class BlobConfig
{
    public string AccountName { get; set; }
    public string Key { get; set; }
    public string Endpoint
    {
        get
        {
            return $"https://{AccountName}.blob.core.windows.net";
        }
    }
    public string ConnectionString()
    {
        return $"DefaultEndpointsProtocol=https;AccountName={AccountName};AccountKey={Key};EndpointSuffix=core.windows.net";
    }
}