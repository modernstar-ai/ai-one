using Agile.Chat.Application.Files.Services;
using Agile.Chat.Domain.Files.Aggregates;
using Agile.Chat.Domain.Files.ValueObjects;
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
        string? FolderName,
        List<string> Tags) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IBlobStorage blobStorage, IFileService fileService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var folderName = request.FolderName;
            if (!string.IsNullOrWhiteSpace(folderName))
                folderName = folderName.Trim().Trim('/');

            var cosmosFile = await fileService.GetFileByFolderAsync(request.File.FileName, request.IndexName, folderName);

            if (cosmosFile != null)
            {
                cosmosFile.Update(FileStatus.QueuedForIndexing, cosmosFile.Url, request.File.ContentType, request.File.Length, request.Tags);
            }
            else
            {
                cosmosFile = CosmosFile.Create(
                    request.File.FileName,
                    request.File.ContentType, 
                    request.File.Length, 
                    request.IndexName, 
                    folderName,
                    request.Tags);
            }
            
            await fileService.UpdateItemByIdAsync(cosmosFile.Id, cosmosFile);
            await blobStorage.UploadAsync(request.File.OpenReadStream(), request.File.ContentType, request.File.FileName, request.IndexName, folderName);
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