namespace Agile.Framework.Common.EnvironmentVariables;

public static class Constants
{
    #region CosmosDbConstants

    public static class Cosmos
    {
        public static class Files
        {
            public const string ContainerName = "Files";
            public const string PartitionKeyPath = "/id";
            public static readonly string[] SortableTextProperties = ["Name", "ContentType", "IndexName", "FolderName"];
        }
    }


    public const string CosmosAssistantsContainerName = "Assistants";
    public const string CosmosAssistantsPartitionKeyPath = "/id";

    public const string CosmosAuditsContainerName = "Audits";
    public const string CosmosAuditsPartitionKeyPath = "/type";

    public const string CosmosChatsContainerName = "Chats";
    public const string CosmosChatsPartitionKeyPath = "/type";

    public const string CosmosToolsContainerName = "Tools";
    public const string CosmosToolsPartitionKeyPath = "/id";

    public const string CosmosIndexesContainerName = "Indexes";
    public const string CosmosIndexesPartitionKeyPath = "/id";
    #endregion

    #region BlobStorageConstants
    public const string BlobIndexContainerName = "index-content";
    public const string BlobThreadContainerName = "thread-content";
    #endregion

    #region PromptPaths
    public static string ChatCompletionsPromptsPath => Path.Combine(@"ChatCompletions", "Prompts");
    public static class Prompts
    {
        public const string ChatWithRag = "chatWithRag.prompt.yaml";
        public const string ChatWithSearch = "chatWithSearch.prompt.yaml";
    }

    public static class TextModels
    {
        public const string Gpt4o = "GPT-4o";
        public const string O3Mini = "o3-mini";
    }

    #endregion
}