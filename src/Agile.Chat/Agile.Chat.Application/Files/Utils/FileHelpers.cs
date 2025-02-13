using Agile.Framework.AzureDocumentIntelligence.Converters;
using Agile.Framework.AzureDocumentIntelligence.Converters.Msg;

namespace Agile.Chat.Application.Files.Utils;

public static class FileHelpers
{
    public static HashSet<string> TextFormats = ["text/htm", "text/html", "text/plain", "application/json"];
    public static string GetContentType(string url)
    {
        var file = Path.GetFileName(url).Split(".").Last();
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
            
            // OTHER CUSTOM FORMATS
            _ => "application/octet-stream"
        };
    }
    
    public static bool HasCustomConverter(string url, out ICustomConverter converter)
    {
        var extension = Path.GetFileName(url).Split(".").Last();
        switch(extension)
        {
            case "msg":
                case "eml":
            converter = new MsgConverter();
            return true;
            default:
                converter = null!;
                return false;
        }
    }
}