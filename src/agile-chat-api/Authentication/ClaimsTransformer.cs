using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace agile_chat_api.Authentication;

public class ClaimsTransformer : IClaimsTransformation
{
    private readonly IRoleService _roleService;
    public ClaimsTransformer(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var claimsIdentity = principal.Identity as ClaimsIdentity;
        if (claimsIdentity?.IsAuthenticated != true)
            return principal;

        var userObjectId = claimsIdentity.Name;
        var (roles, groups) = await _roleService.GetRolesAndGroupsByUserIdAsync(userObjectId!);

        var customClaims = new List<Claim>();
        customClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        customClaims.AddRange(groups.Select(group => new Claim("Groups", group)));

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
