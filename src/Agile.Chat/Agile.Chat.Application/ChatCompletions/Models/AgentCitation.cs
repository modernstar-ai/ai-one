namespace Agile.Chat.Application.ChatCompletions.Models;

public class AgentCitation
{
    public int ContentIndex { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    
    public AgentCitation(int contentIndex, int startIndex, int endIndex, string name, string url)
    {
        ContentIndex = contentIndex;
        StartIndex = startIndex;
        EndIndex = endIndex;
        Name = name;   
        Url = url;
    }
}