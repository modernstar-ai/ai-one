using Agile.Chat.Application.Files.Services;
using Agile.Chat.Domain.Files.Aggregates;
using Agile.Framework.BlobStorage.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Files.Commands;

public static class UploadFile
{
    public record Command(
        IFormFile File,
        string IndexName, 
        string? FolderName) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IBlobStorage blobStorage, IFileService fileService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var url = await blobStorage.UploadAsync(request.File.OpenReadStream(), request.File.ContentType, request.File.FileName, request.IndexName, request.FolderName);

            var cosmosFile = await fileService.GetFileByFolderAsync(request.File.FileName, request.IndexName,
                request.FolderName);

            if (cosmosFile != null)
            {
                cosmosFile.Update(request.File.ContentType, request.File.Length);
            }
            else
            {
                cosmosFile = CosmosFile.Create(
                    request.File.FileName, 
                    url, 
                    request.File.ContentType, 
                    request.File.Length, 
                    request.IndexName, 
                    request.FolderName);
            }
            
            await fileService.UpdateItemByIdAsync(cosmosFile.Id, cosmosFile);
            return Results.Created(cosmosFile.Id, cosmosFile);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(request => request.File)
                .NotNull()
                .WithMessage("File is required");
            
            RuleFor(request => request.IndexName)
                .NotNull()
                .WithMessage("Index Name is required");
        }
    }
}