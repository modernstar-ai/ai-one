using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Indexes.Commands;

public static class DeleteIndexById
{
    public record Command(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handler executed {Handler} with Id {Id}", typeof(Handler).Namespace, request.Id);
            
            return Results.Ok();
        }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> logger)
        {
            RuleFor(request => request.Id)
                .NotNull()
                .WithMessage("Id is required");
        }
    }
}