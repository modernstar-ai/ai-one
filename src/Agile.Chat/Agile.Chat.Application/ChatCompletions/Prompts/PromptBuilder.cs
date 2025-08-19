using Agile.Chat.Application.ChatCompletions.Utils;
using Agile.Chat.Domain.ChatThreads.Entities;

namespace Agile.Chat.Application.ChatCompletions.Prompts;

public static class PromptBuilder
{
    public static string BuildChatWithRagPrompt(List<ChatThreadFile> threadFiles)
    {
        var basePrompt = @"You MUST add citations to ANY part of the responses you create with the help/assistance of the document chunks provided to you in any way shape or form.
  These reference numbers can be found from the ReferenceNumber before every document chunk.
  In longer responses, you must avoid adding all the citations at the end of the response. Spread your citations out at each part of the response you use references.
  The citations must be in the following format:
  【doc{DOC_NUMBER}】 where {DOC_NUMBER} is the ReferenceNumber. i.e. 【doc1】, 【doc3】, 【doc12】 etc.";

        if (threadFiles.Count > 0)
        {
            basePrompt += $@"
----
Here are some special uploaded files attached as context for the conversation. Citations must be added for this special case if the information is used as part of your response and must follow the same guidelines and rules stated previously only with the exception of formatting differences.
  The citations for these special uploaded files must be in the following format:
  【file{{DOC_NUMBER}}】 where {{DOC_NUMBER}} is the ReferenceNumber. i.e. 【file1】, 【file3】, 【file12】 etc.

  Files:
  {ChatUtils.GetThreadFilesString(threadFiles)}
----
";
        }

        basePrompt +=
            @"
Your references MUST ONLY come from documents provided to you in tool calls or special uploaded files attached as context for the conversation in the system prompt.
  ALL of your responses MUST be in markdown format.";
        
        return basePrompt;
    }
}