using System.Collections.Generic;
using Agile.Framework.Common.EnvironmentVariables;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace Agile.Framework.Authentication;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureAdAuth(this IServiceCollection services)
    {
        var azureAdConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"AzureAd:Instance", "https://login.microsoftonline.com/"},
                {"AzureAd:ClientId", Configs.AzureAd.ClientId},
                {"AzureAd:TenantId", Configs.AzureAd.TenantId},
                {"AzureAd:AllowWebApiToBeAuthorizedByACL", "True"}
            })
            .Build();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(azureAdConfig);

        return services;
    }
}