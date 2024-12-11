namespace Agile.Chat.Application.Files.Utils;

public static class FileHelpers
{
    public static string GetContentType(string url)
    {
        var file = Path.GetFileName(url).Split(".").Last();
        return file switch
        {
            "bmp" => "image/bmp",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "htm" => "text/htm",
            "html" => "text/html",
            "jpg" => "image/jpg",
            "jpeg" => "image/jpeg",
            "pdf" => "application/pdf",
            "png" => "image/png",
            "ppt" => "application/vnd.ms-powerpoint",
            "pptx" => "applicatiapplication/vnd.openxmlformats-officedocument.presentationml.presentation",
            "tiff" => "image/tiff",
            "txt" => "text/plain",
            "xls" => "application/vnd.ms-excel",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}