using Html2Markdown;
using MsgReader.Outlook;

namespace Agile.Framework.AzureDocumentIntelligence.Converters.Msg;

public class MsgConverter : ICustomConverter
{
    public async Task<string> ExtractDocumentAsync(Stream fileStream)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var msg = new Storage.Message(memoryStream);

        return $"""
                Email Message
                
                Subject: {msg.Subject}
                Sender: {msg.Sender.DisplayName} {msg.Sender.Email}
                To: {string.Join(", ", msg.Recipients.Select(r => r.Email))}
                Send Date: {msg.SentOn.ToString()}
                
                Body:
                {msg.BodyText}
               """;
    }
}