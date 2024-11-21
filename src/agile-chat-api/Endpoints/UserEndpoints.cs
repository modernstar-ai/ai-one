using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

public static class UserEndpoints
{
    public class UserEndpointsLogger {}
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("api/user").RequireAuthorization();
        users.MapGet(string.Empty, async (HttpContext context, [FromServices] ILogger<UserEndpointsLogger> logger) =>
        {
            return Results.Ok(new
            {
                Roles = context.User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value) ?? [],
                Groups = context.User.Claims.Where(x => x.Type == "Groups").Select(x => x.Value) ?? []
            });
        });
    }
}