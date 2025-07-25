﻿using System.Text.Json.Nodes;
using Agile.Chat.Application.Files.Commands;
using Agile.Chat.Application.Files.Utils;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Events;

public class ServiceBusQueueConsumer : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<ServiceBusQueueConsumer> _logger;
    private readonly IServiceProvider _sp;

    public ServiceBusQueueConsumer(ILogger<ServiceBusQueueConsumer> logger, IServiceProvider sp)
    {
        _logger = logger;
        _sp = sp;
        if (!string.IsNullOrEmpty(Configs.AzureServiceBus.ConnectionString))
        {
            _client = new ServiceBusClient(Configs.AzureServiceBus.ConnectionString, new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            });
        }
        else
        {
            _client = new ServiceBusClient(Configs.AzureServiceBus.FullyQualifiedNamespace,
                new DefaultAzureCredential(), new ServiceBusClientOptions()
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets
                });
        }

        _processor = _client.CreateProcessor(Configs.AzureServiceBus.BlobQueueName, new ServiceBusProcessorOptions()
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false,
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        using var cts = new CancellationTokenSource();
        var autoRenewTask = AutoRenewLockAsync(args, cts.Token);
        try
        {
            var body = await JsonNode.ParseAsync(args.Message.Body.ToStream());
            _logger.LogInformation("Received queue message: {Body}", body?.ToString());

            // Process your message here
            var response = await ProcessMessageAsync(body!);
            if (response is not IStatusCodeHttpResult { StatusCode: 200 })
            {
                _logger.LogError("Error processing queue message: {Result}", response);
                await args.AbandonMessageAsync(args.Message);
            }
            else
            {
                // Complete the message only if processing was successful
                await args.CompleteMessageAsync(args.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing queue message");
            // In case of error, abandon the message to retry later
            await cts.CancelAsync();
            await autoRenewTask;
            await args.AbandonMessageAsync(args.Message);

        }
        finally
        {
            // Stop the auto-renewal loop
            await cts.CancelAsync();
            await autoRenewTask;
        }
    }

    private async Task AutoRenewLockAsync(ProcessMessageEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Delay should be less than the lock duration
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                await args.RenewMessageLockAsync(args.Message, cancellationToken);
                _logger.LogInformation("Lock on queue item renewed successfully. New date of Expiry: {Date}", args.Message.ExpiresAt);
            }
        }
        catch (TaskCanceledException) { } // Expected when cancellationToken is cancelled
        catch (Exception ex)
        {
            _logger.LogInformation($"Error during lock renewal: {ex.Message}");
        }
    }

    private async Task<IResult> ProcessMessageAsync(JsonNode body)
    {
        var (indexName, folderName) = EventGridHelpers.GetIndexAndFolderName(body);
        var (fileName, eventType) = EventGridHelpers.GetFileNameAndEventType(body);
        var fileMetadata = EventGridHelpers.GetFileCreatedMetaData(body);
        fileMetadata.BlobUrl = EventGridHelpers.GetBlobUrl(Configs.BlobStorage.Endpoint, body);
        _logger.LogInformation("Blob URL: {BlobUrl}", fileMetadata.BlobUrl);
        _logger.LogInformation("Fetched index name {IndexName} folder name {FolderName}", indexName, folderName);
        _logger.LogInformation("Fetched file name {FileName} event type {EventType}", fileName, eventType);

        //Skip processing files not uploaded to a folder (aka container)
        if (string.IsNullOrWhiteSpace(indexName))
            return Results.Ok();

        var command = new FileIndexer.Command(fileName, indexName, folderName, fileMetadata, eventType);
        var mediator = _sp.CreateScope().ServiceProvider.GetService<IMediator>();
        return await mediator!.Send(command);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Message processing error");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await _processor.StopProcessingAsync(stoppingToken);
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
        await base.StopAsync(stoppingToken);
    }
}