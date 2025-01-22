using System.Text.Json.Nodes;
using Agile.Chat.Application.Files.Services;
using Agile.Chat.Application.Files.Utils;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Files.Aggregates;
using Agile.Framework.AzureAiSearch.Interfaces;
using Agile.Framework.AzureDocumentIntelligence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Files.Commands;

public static class FileIndexer
{
    public record Command(string FileUrl) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IFileService fileService, IMediator mediator, IDocumentIntelligence documentIntelligence) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var command = new DownloadFileByUrl.Command(request.FileUrl);
            var result = await mediator.Send(command);

            if (result is not FileStreamHttpResult okResult)
                return result;

            var isTextDoc = FileHelpers.TextFormats.Contains(okResult.ContentType);
            var chunks = await documentIntelligence.CrackDocumentAsync(okResult.FileStream, isTextDoc);
            return Results.Ok();
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> logger)
        {
        }
    }
}