using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Events;

public interface IServiceBusQueue
{
    public Task AddToQueueAsync(string body);
}

[Export(typeof(IServiceBusQueue), ServiceLifetime.Singleton)]
public class ServiceBusQueue : IServiceBusQueue
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusQueue> _logger;
    
    public ServiceBusQueue(ILogger<ServiceBusQueue> logger)
    {
        _logger = logger;
        _client = new ServiceBusClient(Configs.AzureServiceBus.ConnectionString, new ServiceBusClientOptions()
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        });
    }

    public async Task AddToQueueAsync(string body)
    {
        var sender = _client.CreateSender(Configs.AzureServiceBus.BlobQueueName);
        // Create a new message
        ServiceBusMessage message = new ServiceBusMessage(body);
        // Send the message to the queue
        await sender.SendMessageAsync(message);
        // Close the sender
        await sender.CloseAsync();
    }
}