using agile_chat_api.Authentication;
using agile_chat_api.Authentication.UTS;
using Microsoft.AspNetCore.Mvc;

public static class UserEndpoints
{
    public class UserEndpointsLogger {}
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {

        app.MapGet("/{id}", async (string id, [FromServices] IRoleService roleService, [FromServices] ILogger<UserEndpointsLogger> logger) =>
        {
            var (roles, groups) = await roleService.GetRolesAndGroupsByUserIdAsync(id);
            return Results.Ok(new
            {
                Roles = roles,
                Groups = groups
            });
        });
    }
}