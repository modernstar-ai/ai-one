using System.Text.Json.Serialization;

namespace agile_chat_api.Authentication.UTS.Models;

public class UTSUserRole
{
    public UserRole Role { get; set; }
    public List<string> Groups { get; set; } = new();
}