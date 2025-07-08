namespace Agile.Chat.Application.ChatCompletions.Dtos;

public class SearchResponseDto
{
    public string AssistantResponse { get; set; }
    public SearchProcess SearchProcess { get; set; }
}

public class SearchProcess
{
    public string ThoughtProcess { get; set; }
}