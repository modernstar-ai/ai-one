
using DotNetEnv;
using Microsoft.Azure.Cosmos;
using Microsoft.Graph.TermStore;
using OpenAI.Chat;
using System.Net;

public interface IChatThreadService
{
    IEnumerable<ChatThread> GetAll();
    IEnumerable<ChatThread> GetAllByUserId(string userId);
    IEnumerable<Message> GetAllMessagesByThreadId(string threadId);
    ChatThread? GetById(string id);
    void Create(ChatThread chatThread, string userId);

    void CreateChat(Message message);
    void Update(ChatThread chatThread);

    string GetLatestUserMessageContent(List<ChatMessage> messages);

    ChatThread GetChatThread(string threadId, string prompt, string userId, string userName);
    void Delete(string id, string userid);
    void UpdateMessageReaction(string messageId, string userId, ReactionType reactionType, bool value);
}

public class ChatThreadService : IChatThreadService
{
    private readonly Container _container;

    public ChatThreadService()
    {
        string cosmosDbUri = Env.GetString("AZURE_COSMOSDB_URI") ?? throw new InvalidOperationException("Cosmos DB URI is missing.");
        string cosmosDbKey = Env.GetString("AZURE_COSMOSDB_KEY") ?? throw new InvalidOperationException("Cosmos DB Key is missing.");
        string databaseName = Env.GetString("AZURE_COSMOSDB_DB_NAME") ?? throw new InvalidOperationException("Cosmos DB Database Name is missing.");
        string containerName = Env.GetString("AZURE_COSMOSDB_CHAT_THREADS_CONTAINER_NAME") ?? throw new InvalidOperationException("Cosmos DB Chat Threads Container Name is missing.");

        var cosmosClient = new CosmosClient(cosmosDbUri, cosmosDbKey);
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }


    public ChatThread GetChatThread(string threadId, string prompt, string userId, string userName)
    {
            ChatThread chatThread = _container.GetItemLinqQueryable<ChatThread>(true)
                  .Where(t => t.id.ToString() == threadId.ToString())
                  .AsEnumerable()
                  .First();

            return chatThread;
        
    }


    public IEnumerable<ChatThread> GetAll()
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.type = 'CHAT_THREAD'");
        var query = _container.GetItemQueryIterator<ChatThread>(queryDefinition);
        var results = new List<ChatThread>();
        while (query.HasMoreResults)
        {
            var response = query.ReadNextAsync().GetAwaiter().GetResult();
            results.AddRange(response.ToList());
        }
        return results;
    }

    public IEnumerable<ChatThread> GetAllByUserId(string userId)
    {
        var queryDefinition = new QueryDefinition(
            "SELECT * FROM c WHERE c.type = 'CHAT_THREAD' AND c.userId = @userId AND c.name <> @name AND c.isDeleted = @isDeleted ORDER BY c.createdAt DESC")
            .WithParameter("@userId", userId)
            .WithParameter("@name", "New Chat")
            .WithParameter("@isDeleted", false);


        var query = _container.GetItemQueryIterator<ChatThread>(queryDefinition);
        var results = new List<ChatThread>();

        while (query.HasMoreResults)
        {
            var response = query.ReadNextAsync().GetAwaiter().GetResult();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public ChatThread? GetById(string id)
    {
        try
        {
            var query = _container.GetItemLinqQueryable<ChatThread>(true)
                  .Where(t => t.id.ToString() == id.ToString())
                  .AsEnumerable()
                  .FirstOrDefault();

            return query;

        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {

            return null;
        }
    }

    public void Create(ChatThread chatThread, string userId)
    {
        try
        {
            chatThread.id = Guid.NewGuid().ToString();
            chatThread.userId = userId;
            chatThread.createdAt = DateTime.UtcNow;
            chatThread.lastMessageAt = DateTime.UtcNow;

            _container.CreateItemAsync(
                chatThread,
                new PartitionKey(chatThread.userId.ToString())
            ).GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void Update(ChatThread chatThread)
    {
        chatThread.lastMessageAt = DateTime.UtcNow;

        _container.ReplaceItemAsync(
            chatThread,
            chatThread.id.ToString(),
            new PartitionKey(chatThread.userId.ToString())
        ).GetAwaiter().GetResult();
    }

    public void UpdateMessageReaction(string messageId, string userId, ReactionType reactionType, bool value)
    {
        try
        {
            string propertyPath;
            if (reactionType == ReactionType.Like)
            {
                propertyPath = "/like";
            }
            else
            {
                propertyPath = "/disLike";
            }

            if (value)
            {
                string oppositePropertyPath;
                if (reactionType == ReactionType.Like)
                {
                    oppositePropertyPath = "/disLike";
                }
                else
                {
                    oppositePropertyPath = "/like";
                }

                var patchOperations = new[]
                {
                    PatchOperation.Set(propertyPath, value),
                    PatchOperation.Set(oppositePropertyPath, false)
                };

                _container.PatchItemAsync<Message>(
                    messageId,
                    new PartitionKey(userId),
                    patchOperations
                ).GetAwaiter().GetResult();
            }
            else
            {
                var patchOperations = new[]
                {
                    PatchOperation.Set(propertyPath, value)
                };

                _container.PatchItemAsync<Message>(
                    messageId,
                    new PartitionKey(userId),
                    patchOperations
                ).GetAwaiter().GetResult();
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            //Message not found
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update message reaction: userId: {userId} {ex.Message}", ex);
        }
    }


    public void Delete(string id, string userId)
    {
        if (GetById(id) is ChatThread chatThread)
        {
            chatThread.isDeleted = true;
            Update(chatThread);
        }
    }

    public void CreateChat(Message message)
    {
        try
        {
            message.id = Guid.NewGuid().ToString();
            message.createdAt = DateTime.UtcNow;

           string userIdString = message.userId?.ToString() ?? string.Empty;

             _container.CreateItemAsync(
                message,
                new PartitionKey(userIdString)
            ).GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public string GetLatestUserMessageContent(List<ChatMessage> messages)
    {
        // Reverse iterate through the list to find the last user message
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            var message = messages[i];


            if (message is UserChatMessage userMessage)
            {
                var contentPart = userMessage.Content;

                if (contentPart.Count > 0)
                {
                    return contentPart[0].Text;
                }
            }
            else if (message is AssistantChatMessage assistantMessage)  //It seems should be UserChatMessage not AssistantChatMessage!
            {
                var contentPart = assistantMessage.Content;

                if (contentPart.Count > 0)
                {
                    return contentPart[0].Text;
                }
            }
        }

        return string.Empty;
    }

    public IEnumerable<Message> GetAllMessagesByThreadId(string threadId)
    {
        var queryDefinition = new QueryDefinition(
            "SELECT * FROM c WHERE c.type = 'CHAT_MESSAGE' AND c.threadId = @threadId AND c.isDeleted = @isDeleted")
            .WithParameter("@threadId", threadId)
            .WithParameter("@isDeleted", false);

        var query = _container.GetItemQueryIterator<Message>(queryDefinition);
        var results = new List<Message>();

        while (query.HasMoreResults)
        {
            var response = query.ReadNextAsync().GetAwaiter().GetResult();
            results.AddRange(response.ToList());
        }

        return results;
    }
}