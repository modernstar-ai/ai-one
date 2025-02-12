using System.Net;
using Agile.Chat.Application.Assistants.Services;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.AzureAiSearch;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.AiSearch.Queries;

public static class GetChunkById
{
    public record Query(Guid AssistantId, string ChunkId) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAzureAiSearch azureAiSearch, IAssistantService assistantService) : IRequestHandler<Query, IResult>
    {

        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var assistant = await assistantService.GetItemByIdAsync(request.AssistantId.ToString());
            if(string.IsNullOrWhiteSpace(assistant!.FilterOptions.IndexName)) return Results.NotFound();    
            logger.LogInformation("Found Assistant name: {Name} with Id: {Id}", assistant!.Name, assistant.Id);
            
            var chunk = await azureAiSearch.GetChunkByIdAsync(assistant!.FilterOptions.IndexName, request.ChunkId);
            return Results.Ok(chunk?.Chunk);
        }
    }
    
    public class Validator : AbstractValidator<Query>
    {
        public Validator(IRoleService roleService, IAssistantService assistantService)
        {
            RuleFor(request => request.AssistantId)
                .NotNull()
                .WithMessage("Assistant Id is required");
            
            RuleFor(request => request.ChunkId)
                .NotNull()
                .WithMessage("Chunk Id is required");
            
            RuleFor(request => request)
                .MustAsync(async (request, _) =>
                {
                    var assistant = await assistantService.GetItemByIdAsync(request.AssistantId.ToString());
                    return assistant is not null && roleService.IsUserInGroup(assistant.FilterOptions.Group);
                })
                .WithErrorCode(HttpStatusCode.Forbidden.ToString());
        }
    }
}