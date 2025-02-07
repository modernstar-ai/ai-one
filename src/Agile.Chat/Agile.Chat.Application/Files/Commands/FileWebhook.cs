using System.Text.Json.Nodes;
using Agile.Chat.Application.Files.Services;
using Agile.Chat.Application.Files.Utils;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Files.Aggregates;
using Agile.Framework.AzureAiSearch.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Files.Commands;

public static class FileWebhook
{
    public record Command(JsonNode Body) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IFileService fileService, IHttpContextAccessor contextAccessor, IAzureAiSearch azureAiSearch) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Validated Authorization for web hook. Received body {@Body}", request.Body);
            if (GetEventGridValidationCode(request.Body, out var code))
                return Results.Ok(new { validationResponse = code });
            
            var (indexName, folderName) = EventGridHelpers.GetIndexAndFolderName(request.Body);
            var (fileName, eventType) = EventGridHelpers.GetFileNameAndEventType(request.Body);
            logger.LogInformation("Fetched index name {IndexName} folder name {FolderName}", indexName, folderName);
            logger.LogInformation("Fetched file name {FileName} event type {EventType}", fileName, eventType);
            
            if (string.IsNullOrWhiteSpace(indexName))
                return Results.BadRequest();

            await HandleFileSyncing(eventType, fileName, indexName, folderName, request.Body);

            if (!await azureAiSearch.IndexerExistsAsync(indexName))
            {
                logger.LogInformation("Creating default indexer for new index name: {IndexName}", indexName);
                await azureAiSearch.CreateIndexerAsync(indexName);
            }

            try
            {
                await azureAiSearch.RunIndexerAsync(indexName);
            }
            catch (Exception e)
            {
                logger.LogError("Failed to run indexer. Message {Message}", e.Message);
            }

            return Results.Ok();
            
        }

        private bool GetEventGridValidationCode(JsonNode body, out string code)
        {
            //Validate the webhook handshake
            var eventTypeHeader = contextAccessor.HttpContext?.Request.Headers["aeg-event-type"].ToString();
            logger.LogDebug("Fetched aeg-event-type {EventType}", eventTypeHeader);

            if (eventTypeHeader != "SubscriptionValidation")
            {
                code = string.Empty;
                return false;
            }
            
            code = body?.AsArray().FirstOrDefault()?["data"]?["validationCode"]?.ToString();
            logger.LogDebug("Fetched validation code {Code}", code);
            return true;
        }

        private async Task HandleFileSyncing(EventGridHelpers.Type eventType, string fileName, string indexName, string folderName, JsonNode body)
        {
            //In the case of blob deleting, ensure it's removed from cosmos too
            if (eventType == EventGridHelpers.Type.BlobDeleted && await fileService.ExistsAsync(fileName, indexName, folderName))
            {
                await fileService.DeleteByMetadataAsync(fileName, indexName, folderName);
            }
            //In the case of blob creating, ensure it's created in cosmos
            else if (eventType == EventGridHelpers.Type.BlobCreated && !await fileService.ExistsAsync(fileName, indexName, folderName))
            {
                var fileMetaData = EventGridHelpers.GetFileCreatedMetaData(body);
                var file = CosmosFile.Create(fileName, 
                    fileMetaData.BlobUrl, 
                    fileMetaData.ContentType,
                    fileMetaData.ContentLength, 
                    indexName, 
                    folderName);

                await fileService.AddItemAsync(file);
            }
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> logger)
        {
            RuleFor(request => request.Body)
                .NotNull()
                .WithMessage("Body is required");
        }
    }
}