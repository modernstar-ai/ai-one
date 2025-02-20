using System.Net;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.AzureAiSearch;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Queries;

public static class GetChunkById
{
    public record Query(Guid ChunkId) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IChatMessageService chatMessageService) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var message = await chatMessageService.GetItemByIdAsync(request.ChunkId.ToString(), ChatType.Citation.ToString());
            if(message is null) return Results.NotFound();
            return Results.Ok(message.Content);
        }
    }
}