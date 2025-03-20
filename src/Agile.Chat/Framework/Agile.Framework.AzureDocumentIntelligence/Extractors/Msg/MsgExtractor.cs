using MsgReader.Outlook;

namespace Agile.Framework.AzureDocumentIntelligence.Extractors.Msg;

public class MsgExtractor : ICustomExtractor
{
    public async Task<string> ExtractTextAsync(Stream fileStream)
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