using Agile.Chat.Application.Files.Services;
using Agile.Chat.Domain.Files.Aggregates;
using Agile.Framework.BlobStorage.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Files.Commands;

public static class DownloadFileByUrl
{
    public record Command(string Url) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IBlobStorage blobStorage, IHttpContextAccessor contextAccessor) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var (stream, details) = await blobStorage.DownloadAsync(request.Url);
            return Results.Stream(stream, GetContentType(request.Url), Path.GetFileName(request.Url));
        }

        private string GetContentType(string url)
        {
            var file = Path.GetFileName(url).Split(".").Last();
            return file switch
            {
                "bmp" => "image/bmp",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "htm" => "text/htm",
                "html" => "text/html",
                "jpg" => "image/jpg",
                "jpeg" => "image/jpeg",
                "pdf" => "application/pdf",
                "png" => "image/png",
                "ppt" => "application/vnd.ms-powerpoint",
                "pptx" => "applicatiapplication/vnd.openxmlformats-officedocument.presentationml.presentation",
                "tiff" => "image/tiff",
                "txt" => "text/plain",
                "xls" => "application/vnd.ms-excel",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
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