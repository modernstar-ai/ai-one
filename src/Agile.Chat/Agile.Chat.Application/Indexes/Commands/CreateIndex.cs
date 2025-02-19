using System.Net;
using Agile.Chat.Application.Indexes.Services;
using Agile.Chat.Domain.Indexes.Aggregates;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.AzureAiSearch;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Indexes.Commands;

public static class CreateIndex
{
    public record Command(string Name, string Description, int ChunkSize, int ChunkOverlap, string? Group) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IIndexService indexService, IAzureAiSearch azureAiSearch) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            if(indexService.Exists(request.Name))
                return Results.BadRequest("Index already exists");
            
            var index = CosmosIndex.Create(
                request.Name, 
                request.Description, 
                request.ChunkSize,
                request.ChunkOverlap,
                request.Group);

            if (await azureAiSearch.IndexExistsAsync(index.Name))
                return Results.BadRequest("Indexer on Azure AI Search already exists.");

            await azureAiSearch.CreateIndexIfNotExistsAsync(index.Name);
            await indexService.AddItemAsync(index);
            
            return Results.Created(index.Id, index);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRoleService roleService)
        {
            RuleFor(request => roleService.IsSystemAdmin())
                .Must(admin => admin)
                .WithErrorCode(HttpStatusCode.Forbidden.ToString());
        }
    }
}