namespace Agile.Framework.Common.EnvironmentVariables;

public static class Constants
{
    #region CosmosDbConstants
    public const string CosmosDatabaseName = "AgileChat";
    
    public const string CosmosAssistantsContainerName = "Assistants";
    public const string CosmosAssistantsPartitionKeyPath = "/Id";
    
    public const string CosmosAuditsContainerName = "Audits";
    public const string CosmosAuditsPartitionKeyPath = "/Type";
    
    public const string CosmosChatsContainerName = "Chats";
    public const string CosmosChatsPartitionKeyPath = "/Type";
    
    public const string CosmosToolsContainerName = "Tools";
    public const string CosmosToolsPartitionKeyPath = "/Id";
    
    public const string CosmosFilesContainerName = "Files";
    public const string CosmosFilesPartitionKeyPath = "/Id";
    
    public const string CosmosIndexesContainerName = "Indexes";
    public const string CosmosIndexesPartitionKeyPath = "/Id";
    #endregion
    
    #region BlobStorageConstants
    public const string BlobContainerName = "index-content";
    #endregion
    
    #region PromptPaths
    public const string ChatCompletionsPromptsPath = "/Agile.Chat.Application/ChatCompletions/Prompts";
    public static class Prompts
    {
        public const string ChatWithRag = "chatWithRag.prompt.yaml";
    }

    #endregion
}