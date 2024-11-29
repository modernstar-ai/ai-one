using System.Text;
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
}

public static class MessageExtensions
{
    public static ChatHistory ParseSemanticKernelChatHistory(this List<Message> messages)
    {
        ChatHistory chatHistory = new ChatHistory();
        foreach (var message in messages)
        {
            switch (message.Type)
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
    
    public static string ParseSemanticKernelChatHistoryString(this List<Message> messages)
    {
        var chatHistorySb = new StringBuilder();
        foreach (var message in messages)
            chatHistorySb.AppendLine($"""<message role="{message.Type.ToString().ToLower()}">{message.Content}</message>""");
        
        return chatHistorySb.ToString();
    }
}