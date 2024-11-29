using System.Net;
using Agile.Chat.Application.Files.Services;
using Agile.Chat.Application.Indexes.Services;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.AzureAiSearch.Interfaces;
using Agile.Framework.BlobStorage.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Indexes.Commands;

public static class DeleteIndexById
{
    public record Command(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IIndexService indexService, IBlobStorage blobStorage, IFileService fileService, IAzureAiSearch azureAiSearch) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handler executed {Handler} with Id {Id}", typeof(Handler).Namespace, request.Id);
            var index = await indexService.GetItemByIdAsync(request.Id.ToString());
            if (index is null) return Results.NotFound();

            //Delete index from cosmos
            await indexService.DeleteItemByIdAsync(request.Id.ToString());
            
            //Delete all files on blob first
            await blobStorage.DeleteIndexFilesAsync(index.Name);
            //Delete all files in cosmos next
            await fileService.DeleteAllByIndexAsync(index.Name);
            //Delete index/indexer/skillset/datasource on ai search
            await azureAiSearch.DeleteIndexerAsync(index.Name);
            
            return Results.Ok();
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