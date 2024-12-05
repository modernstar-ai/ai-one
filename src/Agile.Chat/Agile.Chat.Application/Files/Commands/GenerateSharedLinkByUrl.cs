using Agile.Chat.Application.Files.Services;
using Agile.Chat.Domain.Files.Aggregates;
using Agile.Framework.BlobStorage.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Files.Commands;

public static class GenerateSharedLinkByUrl
{
    public record Command(string Url) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IBlobStorage blobStorage) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var url = await blobStorage.GetShareableLinkByUrlAsync(request.Url);
            return Results.Ok(url);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(request => request.Url)
                .NotNull()
                .WithMessage("Url is required");
        }
    }
}