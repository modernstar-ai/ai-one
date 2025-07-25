﻿using Agile.Chat.Domain.ChatThreads.ValueObjects;

namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class AssistantFilterOptions
{
    public string IndexName { get; set; }
    public bool LimitKnowledgeToIndex { get; set; } = false;
    public bool AllowInThreadFileUploads { get; set; } = false;
    public int DocumentLimit { get; set; }
    public int? Strictness { get; set; } = null;
    public List<string> Folders { get; set; } = new();

    public List<string> Tags { get; set; } = new();

    public ChatThreadFilterOptions ParseChatThreadFilterOptions()
    {
        var options = new ChatThreadFilterOptions()
        {
            DocumentLimit = DocumentLimit,
            Strictness = Strictness
        };
        return options;
    }
}