// Copyright (c) Microsoft. All rights reserved.

using agile_chat_api.Authentication;
using agile_chat_api.Authentication.UTS;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace agile_chat_api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddAzureAdAuth(this IServiceCollection services)
    {
        var azureAdConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AzureAd:Instance", "https://login.microsoftonline.com/" },
                { "AzureAd:ClientId", Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") },
                { "AzureAd:TenantId", Environment.GetEnvironmentVariable("AZURE_TENANT_ID") },
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
        
        // Register the claims transformation
        services.AddScoped<IRoleService, UTSRoleService>();
        services.AddScoped<IClaimsTransformation, ClaimsTransformer>();

        // Configure JWT Bearer options
        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
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

        return services;
    }
}
