using Agile.Chat.Application.ChatCompletions.Routing;
using Agile.Chat.Application.Events;
using Agile.Framework;
using Agile.Framework.Mediator.Pipelines;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Chat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, WebApplicationBuilder builder) =>
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly)
            .AddServiceBus(builder);

    private static IServiceCollection AddServiceBus(this IServiceCollection services, WebApplicationBuilder builder)
    {
        if (builder.Environment.IsLocal()) return services;

        return services.AddHostedService<ServiceBusQueueConsumer>();
    }
}