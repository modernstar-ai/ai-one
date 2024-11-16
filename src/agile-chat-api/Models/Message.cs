public class Message
{
    public string? id { get; set; }
    public DateTime createdAt { get; set; }
    public string? type { get; set; } = "CHAT_MESSAGE";
    public bool isDeleted { get; set; }
    public string? content { get; set; }
    public string? name { get; set; }
    public string? role { get; set; }
    public string? threadId { get; set; }
    public string? userId { get; set; }
    public string? multiModalImage { get; set; }
    public string? sender { get; set; }

    

}
