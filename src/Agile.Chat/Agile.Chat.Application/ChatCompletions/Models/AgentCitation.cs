namespace Agile.Chat.Application.ChatCompletions.Models;

public class AgentCitation
{
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    
    public AgentCitation(int startIndex, int endIndex, string name, string url)
    {
        StartIndex = startIndex;
        EndIndex = endIndex;
        Name = name;   
        Url = url;
    }
}