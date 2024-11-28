namespace Agile.Framework.Common.EnvironmentVariables;

public static class Constants
{
    #region CosmosDbConstants
    public const string CosmosDatabaseName = "AgileChat";
    public const string CosmosAssistantsContainerName = "Assistants";
    public const string CosmosAuditsContainerName = "Audits";
    public const string CosmosChatsContainerName = "Chats";
    public const string CosmosToolsContainerName = "Tools";
    public const string CosmosFilesContainerName = "Files";
    public const string CosmosIndexesContainerName = "Indexes";
    #endregion
    
    #region BlobStorageConstants
    public const string BlobContainerName = "index-content";
    #endregion
}