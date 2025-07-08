using System.Net;
using Agile.Chat.Application.Indexes.Services;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.AzureAiSearch;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Indexes.Commands;

public static class SyncIndexes
{
    public record Command() : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAzureAiSearch azureAiSearch, IIndexService indexService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var indexes = await indexService.GetAllAsync();

            foreach (var index in indexes)
                await azureAiSearch.SyncIndexAsync(index.Name);
            
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