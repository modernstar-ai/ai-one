using Agile.Framework.AzureDocumentIntelligence.Converters;
using Agile.Framework.AzureDocumentIntelligence.Extractors;
using Agile.Framework.AzureDocumentIntelligence.Extractors.Msg;

namespace Agile.Chat.Application.Files.Utils;

public static class FileHelpers
{
    private static HashSet<string> TextFormats = ["txt", "json", "md"];

    public static bool ContainsText(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.Replace(".", string.Empty);
        if (extension == null) return false;
        return TextFormats.Contains(extension.ToLower());
    }
    
    public static string GetContentType(string url)
    {
        var file = Path.GetFileName(url).Split(".").Last().ToLower();
        return file switch
        {
            // IMAGES
            "bmp" => "image/bmp",
            "jpg" => "image/jpg",
            "jpeg" => "image/jpeg",
            "png" => "image/png",
            "tiff" => "image/tiff",
            
            // DOCUMENTS
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "ppt" => "application/vnd.ms-powerpoint",
            "pptx" => "applicatiapplication/vnd.openxmlformats-officedocument.presentationml.presentation",
            "pdf" => "application/pdf",
            "xls" => "application/vnd.ms-excel",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            
            //MAIL
            "msg" => "application/vnd.ms-outlook",
            "eml" => "message/rfc822",
            
            // TEXT
            "htm" => "text/htm",
            "html" => "text/html",
            "json" => "application/json",
            "txt" => "text/plain",
            "md" => "text/markdown",
            
            // OTHER CUSTOM FORMATS
            _ => "application/octet-stream"
        };
    }
    
    public static bool HasCustomConverter(string url, out ICustomConverter converter)
    {
        var extension = Path.GetFileName(url).Split(".").Last().ToLower();
        switch(extension)
        {
            case "doc":
                converter = null!;
                return false;
            default:
                converter = null!;
                return false;
        }
    }
    
    public static bool HasCustomExtractor(string url, out ICustomExtractor extractor)
    {
        var extension = Path.GetFileName(url).Split(".").Last().ToLower();
        switch(extension)
        {
            case "msg":
                case "eml":
                    extractor = new MsgExtractor();
            return true;
            default:
                extractor = null!;
                return false;
        }
    }
}