using System.Net;
using Agile.Chat.Application.Files.Services;
using Agile.Chat.Domain.Files.ValueObjects;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.BlobStorage.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Files.Commands;

public static class DeleteFileById
{
    public record Command(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IFileService fileService, IBlobStorage blobStorage) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var file = await fileService.GetItemByIdAsync(request.Id.ToString());
            if (file == null) return Results.NotFound();

            logger.LogInformation("Beginning to delete cosmos file {@File}", file);
            await blobStorage.DeleteAsync(file.Name, file.IndexName, file.FolderName);
            file.Update(FileStatus.QueuedForDeletion, file.Url, file.ContentType, file.Size, file.Tags);
            await fileService.UpdateItemByIdAsync(file.Id, file);
            
            return Results.Ok();
        }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRoleService roleService)
        {
            RuleFor(request => request.Id)
                .NotNull()
                .NotEmpty()
                .WithMessage("Id cannot be empty");
            
            RuleFor(request => roleService.IsSystemAdmin())
                .Must(x => x)
                .WithMessage("Must be a system admin")
                .WithErrorCode(HttpStatusCode.Forbidden.ToString());
        }
    }
}