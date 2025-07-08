using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.Ai;

[Experimental("SKEXP0010")]
public static class DependencyInjection
{
    public static IServiceCollection AddAppKernelBuilder(this IServiceCollection services)
    {
        return services.AddScoped<IAppKernelBuilder, AppKernelBuilder>();
    }
}