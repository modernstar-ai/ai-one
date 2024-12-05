using System.Text;
using System.Text.Json.Serialization;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Common.DomainAbstractions;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Agile.Chat.Domain.ChatThreads.Entities;

public class Message : AuditableAggregateRoot
{
    [JsonConstructor]
    private Message(string content, ChatType type, MessageType messageType, string threadId, MessageOptions options)
    {
        Content = content;
        Type = type;
        MessageType = messageType;
        ThreadId = threadId;
        Options = options;
    }
    public string Content { get; private set; }
    public ChatType Type { get; private set; }
    public MessageType MessageType { get; private set; }
    public string ThreadId { get; private set; }
    public MessageOptions Options { get; private set; }

    public static Message CreateUser(string threadId,
        string content,
        MessageOptions options)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new Message(content, ChatType.Message, MessageType.User, threadId, options);
    }
    
    public static Message CreateAssistant(string threadId,
        string content,
        MessageOptions options)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new Message(content, ChatType.Message, MessageType.Assistant, threadId, options);
    }
    
    public void Update(bool isLiked, bool isDisliked)
    {
        //Do validation logic and throw domain level exceptions if fails
        Options.IsLiked = isLiked;
        Options.IsDisliked = isDisliked;
        LastModified = DateTime.UtcNow;
    }
    
    public void AddMetadata(MetadataType key, object value)
    {
        //Do validation logic and throw domain level exceptions if fails
        Options.Metadata.Add(key, value);
        LastModified = DateTime.UtcNow;
    }
}

public static class MessageExtensions
{
    public static ChatHistory ParseSemanticKernelChatHistory(this List<Message> messages)
    {
        ChatHistory chatHistory = new ChatHistory();
        foreach (var message in messages)
        {
            switch (message.MessageType)
            {
                case MessageType.User:
                    chatHistory.AddUserMessage(message.Content);
                    break;
                case MessageType.Assistant:
                    chatHistory.AddAssistantMessage(message.Content);
                    break;
            }
        }
        return chatHistory;
    }
    
    public static ChatHistory ParseSemanticKernelChatHistory(this List<Message> messages, string userPrompt)
    {
        ChatHistory chatHistory = ParseSemanticKernelChatHistory(messages);
        chatHistory.AddUserMessage(userPrompt);
        return chatHistory;
    }
}