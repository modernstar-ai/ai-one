using System.Reflection;
using System.Text.Json.Serialization;
using Agile.Chat.Api.Exceptions;
using Agile.Framework.Common.Attributes;
using Carter;
using Mapster;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Agile.Chat.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddMapster();
        return services
            .AddCorsRules()
            .AddHttpContextAccessor()
            .AddGlobalExceptionHandling()
            .AddExportedServices()
            .AddEndpoints()
            .AddSwagger();
    }

    private static IServiceCollection AddCorsRules(this IServiceCollection services) =>
        services.AddCors(x => x.AddDefaultPolicy(options =>
            options
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()));

    private static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services) =>
        services.AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails();

    private static IServiceCollection AddSwagger(this IServiceCollection services) =>
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "AgileChat", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }});
        });

    private static IServiceCollection AddEndpoints(this IServiceCollection services) =>
        services.AddEndpointsApiExplorer()
            .Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddCarter();

    private static IServiceCollection AddExportedServices(this IServiceCollection services)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is null || string.IsNullOrEmpty(entryAssembly.FullName))
            throw new ArgumentException("entryAssembly cannot be null.");

        var assembliesToCheck = new Queue<Assembly>();
        var loadedAssemblies = new Dictionary<string, Assembly>();

        assembliesToCheck.Enqueue(entryAssembly);
        loadedAssemblies.Add(entryAssembly.FullName, entryAssembly);

        while (assembliesToCheck.Any())
        {
            var assemblyToCheck = assembliesToCheck.Dequeue();

            foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
            {
                if (!loadedAssemblies.ContainsKey(reference.FullName) && !reference.FullName.StartsWith("System") && !reference.FullName.StartsWith("Microsoft"))
                {
                    var assembly = Assembly.Load(reference);
                    assembliesToCheck.Enqueue(assembly);
                    loadedAssemblies.Add(reference.FullName, assembly);
                }
            }
        }

        foreach (var assembly in loadedAssemblies)
        {
            RegisterServices(assembly.Value, services);
        }

        return services;
    }

    private static void RegisterServices(Assembly assembly, IServiceCollection services)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attributes = type.GetCustomAttributes<ExportAttribute>();
            foreach (var att in attributes)
            {
                switch (att.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        if (att.ServiceType != null)
                            services.AddSingleton(att.ServiceType, type);
                        else
                            services.AddSingleton(type);
                        break;

                    case ServiceLifetime.Scoped:
                        if (att.ServiceType != null)
                            services.AddScoped(att.ServiceType, type);
                        else
                            services.AddScoped(type);
                        break;
                    case ServiceLifetime.Transient:
                        if (att.ServiceType != null)
                            services.AddTransient(att.ServiceType, type);
                        else
                            services.AddTransient(type);
                        break;
                }
            }
        }
    }

    public static void EnableProxyHeaders(this IApplicationBuilder app)
    {
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
                var pathBase = prefix.FirstOrDefault() ?? "";
                context.Request.PathBase = pathBase;
            }
            return next();
        });
    }

    public static void ConfigureSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger(c =>
        {
            c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            {
                // Ensure OpenAPI version is explicitly set
                if (swaggerDoc.Info != null)
                {
                    swaggerDoc.Info.Version = "v1";
                }

                var pathBase = httpReq.PathBase.Value ?? "";
                if (!string.IsNullOrEmpty(pathBase))
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
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
}