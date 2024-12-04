using Agile.Framework.Authentication.Enums;

namespace Agile.Framework.Authentication.Implementations.UTS.Models;

public class UtsUserRole
{
    public UserRole Role { get; set; }
    public List<string> Groups { get; set; } = new();
}