using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExportAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; }
    public Type? ServiceType { get; }

    public ExportAttribute() : this(ServiceLifetime.Scoped)
    {
    }

    public ExportAttribute(Type serviceType) : this(serviceType, ServiceLifetime.Scoped)
    {
    }

    public ExportAttribute(Type serviceType, ServiceLifetime serviceLifetime) : this(serviceLifetime)
    {
        ServiceType = serviceType;
    }

    public ExportAttribute(ServiceLifetime serviceLifetime)
    {
        Lifetime = serviceLifetime;
    }
}