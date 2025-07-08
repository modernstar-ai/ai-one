namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class AssistantTextModelSelectionDto
{
    public string ModelId { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = false;
}