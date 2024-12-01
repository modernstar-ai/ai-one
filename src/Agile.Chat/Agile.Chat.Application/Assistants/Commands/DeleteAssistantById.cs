using Agile.Chat.Application.Assistants.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.Assistants.Commands;

public static class DeleteAssistantById
{
    public record Command(Guid Id) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAssistantService assistantService) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handler executed {Handler} with Id {Id}", typeof(Handler).Namespace, request.Id);
            await assistantService.DeleteItemByIdAsync(request.Id.ToString());
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