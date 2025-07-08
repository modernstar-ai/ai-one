using System.Net;
using System.Security.Claims;
using Agile.Chat.Application.Files.Services;
using Agile.Framework.Common.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Files.Queries;

public static class GetFiles
{
    public record Query(QueryDto QueryDto) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IFileService fileService) : IRequestHandler<Query, IResult>
    {

        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var files = await fileService.GetAllAsync(request.QueryDto);
            return Results.Ok(files);
        }
    }
    
    public class Validator : AbstractValidator<Query>
    {
        public Validator(IHttpContextAccessor contextAccessor)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            RuleFor(x => username)
                .NotNull()
                .WithMessage("Username is required")
                .WithErrorCode(HttpStatusCode.Forbidden.ToString());
        }
    }
}