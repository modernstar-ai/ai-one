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
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Files.Commands;

public static class FileIndexer
{
    public record Command(Uri FileUrl) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IFileService fileService, IDocumentIntelligence documentIntelligence) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            await documentIntelligence.CrackDocumentAsync(request.FileUrl);
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