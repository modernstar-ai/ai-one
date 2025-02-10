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

    public class Handler(ILogger<Handler> logger, IMediator mediator, IFileService fileService, IHttpContextAccessor contextAccessor, IAzureAiSearch azureAiSearch) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Validated Authorization for web hook. Received body {@Body}", request.Body);
            if (GetEventGridValidationCode(request.Body, out var code))
                return Results.Ok(new { validationResponse = code });
            
            var (indexName, folderName) = EventGridHelpers.GetIndexAndFolderName(request.Body);
            var (fileName, eventType) = EventGridHelpers.GetFileNameAndEventType(request.Body);
            var fileMetadata = EventGridHelpers.GetFileCreatedMetaData(request.Body);
            logger.LogInformation("Fetched index name {IndexName} folder name {FolderName}", indexName, folderName);
            logger.LogInformation("Fetched file name {FileName} event type {EventType}", fileName, eventType);
            
            if (string.IsNullOrWhiteSpace(indexName))
                return Results.BadRequest();

            var command = new FileIndexer.Command(fileName, indexName, folderName, fileMetadata, eventType);
            return await mediator.Send(command, cancellationToken);
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