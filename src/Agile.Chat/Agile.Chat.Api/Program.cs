using Agile.Chat.Api;
using Agile.Chat.Application;
using Agile.Framework;
using Agile.Framework.Authentication;
using Agile.Framework.Common.EnvironmentVariables;
using Carter;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
Configs.InitializeConfigs(builder.Configuration);

builder.Services
    .AddFramework(builder.Configuration)
    .AddApplication(builder)
    .AddApi();

var app = builder.Build();
await app.InitializeServicesAsync();

app.EnableProxyHeaders();
app.UseCors();

if (!app.Environment.IsProduction())
{
    app.ConfigureSwagger();
}

app.UseAuthentication();
app.UseAuthorization();
app.UsePermissionsHandling();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapCarter();

app.Run();