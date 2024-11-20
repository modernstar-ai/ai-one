using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace agile_chat_api.Authentication.UTS;

public class AuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IRoleService _roleService;

    public AuthenticationHandler(IRoleService roleService, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
        : base(options, logger, encoder, clock)
    {
    }
    
    public static string SchemeName => "ApiAuth";
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(authHeader);

        if (jsonToken == null) return AuthenticateResult.Fail("Missing Token");

        var claims = jsonToken.Claims.ToList();
        var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
        var roles = await _roleService.GetRolesByUserIdAsync(userId);

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }
}
