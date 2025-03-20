using System.Security.Claims;
using Agile.Framework.Authentication.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Agile.Framework.Authentication.Implementations.AzureAD;

public class AzureAdPermissionsResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public AzureAdPermissionsResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRoleService roleService)
    {
        var principal = context.User;

        if (principal?.Identity?.IsAuthenticated == true)
        {
            var claimsIdentity = principal.Identity as ClaimsIdentity;

            var userObjectId = claimsIdentity!.Name;
            var (roles, groups) = await roleService.GetRolesAndGroupsByUserIdAsync(userObjectId!);

            var customClaims = new List<Claim>();
            customClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.ToString())));
            customClaims.AddRange(groups.Select(group => new Claim("Groups", group)));

            foreach (var claim in customClaims)
            {
                if (!claimsIdentity.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
                {
                    claimsIdentity.AddClaim(claim);
                }
            }
        }

        await _next(context);
    }
}
