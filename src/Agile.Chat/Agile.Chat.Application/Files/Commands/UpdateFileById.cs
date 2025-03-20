using Agile.Chat.Application.Events;
using Agile.Chat.Application.Files.Services;
using Agile.Chat.Application.Files.Utils;
using Agile.Chat.Domain.Files.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Files.Commands;

public static class UpdateFileById
{
    public record Command(string Id, List<string> Tags) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IFileService fileService, IServiceBusQueue serviceBusQueue) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var cosmosFile = await fileService.GetItemByIdAsync(request.Id);
            if (cosmosFile == null) return Results.NotFound();
            
            cosmosFile.Update(FileStatus.QueuedForIndexing, cosmosFile.Url, cosmosFile.ContentType, cosmosFile.Size, request.Tags);
            await fileService.UpdateItemByIdAsync(cosmosFile.Id, cosmosFile);

            var message =
                EventGridHelpers.CreateEventObjectFromCosmosFile(cosmosFile, EventGridHelpers.Type.BlobCreated);
            await serviceBusQueue.AddToQueueAsync(message);
            
            return Results.Ok();
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
        }
    }
}