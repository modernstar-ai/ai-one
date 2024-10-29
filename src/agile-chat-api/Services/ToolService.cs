using agile_chat_api.Enums;
using DotNetEnv;
using Microsoft.Azure.Cosmos;
using System.Collections.Concurrent;
using System.ComponentModel;

public interface IToolService
{
    IEnumerable<Tool> GetAll();
    Tool? GetById(Guid id);
    void Create(Tool tool);
    void Update(Tool tool);
    void Delete(Guid id);
}
public class ToolService : IToolService
{
    private readonly Microsoft.Azure.Cosmos.Container _container;

    public ToolService()
    {
        // Read Cosmos DB configuration from environment variables
        string cosmosDbUri = Env.GetString("AZURE_COSMOSDB_URI") ?? throw new InvalidOperationException("Cosmos DB URI is missing.");
        string cosmosDbKey = Env.GetString("AZURE_COSMOSDB_KEY") ?? throw new InvalidOperationException("Cosmos DB Key is missing.");

        // Read database and container names from environment variables
        string databaseName = Env.GetString("AZURE_COSMOSDB_DATABASE_NAME") ?? throw new InvalidOperationException("Cosmos DB Database Name is missing.");
        string containerName = Env.GetString("AZURE_COSMOSDB_TOOLS_CONTAINER_NAME") ?? throw new InvalidOperationException("Cosmos DB Tools Container Name is missing.");


        // Initialize CosmosClient manually
        var cosmosClient = new CosmosClient(cosmosDbUri, cosmosDbKey);
        
        // Initialize the container
        _container = cosmosClient.GetContainer(databaseName, containerName);

      

    }

     


    public IEnumerable<Tool> GetAll()
    {
        var query = _container.GetItemQueryIterator<Tool>();
        var results = new List<Tool>();

        while (query.HasMoreResults)
        {
            // Reading the next set of results
            var response = query.ReadNextAsync().GetAwaiter().GetResult();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public Tool? GetById(Guid id)
    {
        try
        {
           // var response = _container.ReadItemAsync<Tool>(id.ToString(), PartitionKey.None).GetAwaiter().GetResult();
            
            var query = _container.GetItemLinqQueryable<Tool>(true)
                      .Where(t => t.id.ToString() == id.ToString())
                      .AsEnumerable()
                      .FirstOrDefault();

            //return response.Resource;

            return query;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null; // Return null if the item is not found
        }
    }

    public void Create(Tool tool)
    {
        tool.id  = Guid.NewGuid(); // Assign a new Guid if not already set
        tool.CreatedDate = DateTime.UtcNow.ToString();
        tool.Name = tool.Name;
        tool.type = tool.type;
        tool.Status = tool.Status;
        tool.DatabaseDSN = tool.DatabaseDSN;
        tool.DatabaseQuery = tool.DatabaseQuery;
        tool.JsonTemplate = tool.JsonTemplate;
        tool.Description = tool.Description;
        tool.Method = tool.Method;
        tool.Api = tool.Api;

        tool.LastUpdatedDate = DateTime.UtcNow.ToString();

        _container.CreateItemAsync(tool, new PartitionKey(tool.type.ToString())).GetAwaiter().GetResult();
    }

    public void Update(Tool tool)
    {
        var existingTool = GetById(tool.id);
        if (existingTool != null)
        {
            existingTool.Name = tool.Name;
            existingTool.type = tool.type;
            existingTool.Status = tool.Status;
            existingTool.DatabaseDSN = tool.DatabaseDSN;
            existingTool.DatabaseQuery = tool.DatabaseQuery;
            existingTool.JsonTemplate = tool.JsonTemplate;
            existingTool.Description = tool.Description;
            existingTool.CreatedDate = tool.CreatedDate;
            existingTool.Method = tool.Method;
            existingTool.Api = tool.Api;
            existingTool.LastUpdatedDate = DateTime.UtcNow.ToString();

            _container.ReplaceItemAsync(existingTool, tool.id.ToString(), new PartitionKey(tool.type.ToString())).GetAwaiter().GetResult();
        }
    }

   
    public void Delete(Guid id)
    {
        var existingTool = GetById(id);
        if (existingTool != null)
        {
            _container.DeleteItemAsync<Tool>(id.ToString(), new PartitionKey(existingTool.type.ToString())).GetAwaiter().GetResult();
        }
    }
    

}
