using Agile.Chat.Api;
using Agile.Chat.Application;
using Agile.Framework;
using Agile.Framework.Authentication;
using Agile.Framework.Common.EnvironmentVariables;
using Carter;
using Serilog;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
Configs.InitializeConfigs(builder.Configuration);

builder.Services
    .AddFramework(builder.Configuration)
    .AddApplication(builder)
    .AddApi();

var app = builder.Build();
await app.InitializeServicesAsync();

// Configure forwarded headers and path base for Application Gateway
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                      ForwardedHeaders.XForwardedProto |
                      ForwardedHeaders.XForwardedHost
});

// Handle path base from Application Gateway X-Forwarded-Prefix header
app.Use((context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Forwarded-Prefix", out var prefix))
    {
        context.Request.PathBase = prefix.FirstOrDefault() ?? "";
    }
    return next();
});

app.UseCors();

if (!app.Environment.IsProduction())
{
    app.UseSwagger(c =>
    {
        c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        {
            var pathBase = httpReq.PathBase.Value ?? "";
            if (!string.IsNullOrEmpty(pathBase))
            {
                swaggerDoc.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
                {
                    new() { Url = $"https://{httpReq.Host}{pathBase}" }
                };
            }
        });
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("./v1/swagger.json", "AIOne API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.UsePermissionsHandling();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapCarter();

app.Run();