using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace agile_chat_api.Authentication.UTS;

public class UTSClaimsTransformation : IClaimsTransformation
{
    private readonly IUTSRoleService _roleService;
    public UTSClaimsTransformation(IUTSRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var claimsIdentity = principal.Identity as ClaimsIdentity;
        if (claimsIdentity?.IsAuthenticated != true)
            return principal;

        var userObjectId = claimsIdentity.Name;
        var roles = await _roleService.GetRolesByUserIdAsync(userObjectId!);

        var customClaims = new List<Claim>();
        foreach (var role in roles)
        {
            customClaims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        foreach (var claim in customClaims)
        {
            if (!claimsIdentity.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
            {
                claimsIdentity.AddClaim(claim);
            }
        }

        return principal;
    }
}
