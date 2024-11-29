using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Common.DomainAbstractions;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Agile.Chat.Domain.ChatThreads.Entities;

public class Message : AuditableAggregateRoot
{
    private Message(){}
    public string Content { get; private set; }
    public MessageType Type { get; private set; }
    public string ThreadId { get; private set; }
    public MessageOptions Options { get; private set; }

    public static Message CreateUser(string threadId,
        string content,
        MessageOptions options)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new Message
        {
            ThreadId = threadId,
            Content = content,
            Type = MessageType.User,
            Options = options,
        };
    }
    
    public static Message CreateAssistant(string threadId,
        string content,
        MessageOptions options)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new Message
        {
            ThreadId = threadId,
            Content = content,
            Type = MessageType.Assistant,
            Options = options,
        };
    }
    
    public void Update(MessageOptions options)
    {
        //Do validation logic and throw domain level exceptions if fails
        Options = options;
        LastModified = DateTime.UtcNow;
    }

    public static ChatHistory ParseSemanticKernelChatHistory(List<Message> messages, string? systemPrompt = null)
    {
        
    }
    
    public static string ParseSemanticKernelChatHistoryString(List<Message> messages, string? systemPrompt = null)
    {
        
    }
}