namespace Agile.Chat.Domain.Assistants.ValueObjects;

public class BingConfig
{
    public bool EnableWebSearch { get; set; } = false;
    public int WebResultsCount { get; set; } = 5;
}