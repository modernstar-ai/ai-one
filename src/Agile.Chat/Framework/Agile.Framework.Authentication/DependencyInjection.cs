using Agile.Framework.Authentication.Implementations.Default;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.Common.EnvironmentVariables;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace Agile.Framework.Authentication;

public static class DependencyInjection
{
    public static IServiceCollection AddAgileAuthentication(this IServiceCollection services) =>
        services.AddAzureAdAuth()
            .AddDefaultPermissions()
            .AddClaimsTransformer();
    
    private static IServiceCollection AddAzureAdAuth(this IServiceCollection services)
    {
        var azureAdConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"AzureAd:Instance", "https://login.microsoftonline.com/"},
                {"AzureAd:ClientId", Configs.AzureAd.ClientId},
                {"AzureAd:TenantId", Configs.AzureAd.TenantId},
                {"AzureAd:AllowWebApiToBeAuthorizedByACL", "True"}
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
    
    private static IServiceCollection AddDefaultPermissions(this IServiceCollection services) => services.AddScoped<IRoleService, DefaultRoleService>();

    private static IServiceCollection AddClaimsTransformer(this IServiceCollection services) =>
        // Register the claims transformation
        services.AddScoped<IClaimsTransformation, ClaimsTransformer>()
            // Configure JWT Bearer options
            .Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var claimsTransformation = context.HttpContext.RequestServices
                            .GetRequiredService<IClaimsTransformation>();
                    
                        context.Principal = await claimsTransformation
                            .TransformAsync(context.Principal);
                    }
                };
            });
}