using System.Text;
using System.Text.Json.Serialization;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Common.DomainAbstractions;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Agile.Chat.Domain.ChatThreads.Entities;

public class ChatThreadFile : AuditableAggregateRoot
{
    [JsonConstructor]
    private ChatThreadFile(string name, string content, string contentType, long size, string url, string threadId, ChatType type)
    {
        Name = name;
        Content = content;
        ContentType = contentType;
        Size = size;
        Type = type;
        Url = url;
        ThreadId = threadId;
    }
    public string Name { get; private set; }
    public string Content { get; private set; }
    public string? ContentType { get; private set; }
    public long Size { get; private set; }
    public string Url { get; private set; }
    public ChatType Type { get; private set; }
    public string ThreadId { get; private set; }

    public static ChatThreadFile Create(string name, string content, string contentType, long size, string url, string threadId)
    {
        return new ChatThreadFile(name, content, contentType, size, url, threadId, ChatType.File);
    }
}