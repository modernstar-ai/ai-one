using Agile.Framework.Authentication.Implementations.AzureAD;
using Agile.Framework.Authentication.Implementations.Default;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.Common.EnvironmentVariables.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace Agile.Framework.Authentication;

public static class DependencyInjection
{
    public static IServiceCollection AddAgileAuthentication(this IServiceCollection services) =>
        services.AddAzureAdAuth()
            .AddPermissionsHandling();
    
    private static IServiceCollection AddAzureAdAuth(this IServiceCollection services)
    {
        var azureAdConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"AzureAd:Instance", "https://login.microsoftonline.com/"},
                {"AzureAd:ClientId", Configs.AzureAd.ClientId},
                {"AzureAd:ClientSecret", Configs.AzureAd.ClientSecret},
                {"AzureAd:TenantId", Configs.AzureAd.TenantId},
                {"AzureAd:AllowWebApiToBeAuthorizedByACL", "True"},
            })
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MicrosoftGraph:BaseUrl", "https://graph.microsoft.com/v1.0" },
                { "MicrosoftGraph:Scopes", "User.Read" }
            })
            .Build();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(azureAdConfig)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(azureAdConfig.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

        return services;
    }

    private static IServiceCollection AddPermissionsHandling(this IServiceCollection services)
    {
        var permissionsType = Configs.PermissionsType;

        switch (permissionsType)
        {
            case PermissionsType.AzureAd:
                return services.AddScoped<IRoleService, AzureAdRoleService>();
            default:
                return services.AddScoped<IRoleService, DefaultRoleService>();
        }
    }

    public static IApplicationBuilder UsePermissionsHandling(this IApplicationBuilder app)
    {
        var permissionsType = Configs.PermissionsType;

        return permissionsType switch
        {
            PermissionsType.AzureAd => app.UseMiddleware<AzureAdPermissionsResolutionMiddleware>(),
            _ => app.UseMiddleware<DefaultPermissionsResolutionMiddleware>()
        };
    }
}