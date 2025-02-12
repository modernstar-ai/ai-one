using System.Net;
using Agile.Chat.Application.AiSearch.Dtos;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.AzureAiSearch;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.AiSearch.Queries;

public static class GetIndexReport
{
    public record Query(string indexName) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, IAzureAiSearch azureAiSearch) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
        {
            IndexReportDto indexReport = new IndexReportDto();

            indexReport.SearchIndexStatistics = await azureAiSearch.GetIndexStatisticsByNameAsync(request.indexName);
            return Results.Ok(indexReport);
        }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator(IRoleService roleService)
        {
            RuleFor(request => roleService.IsContentManager())
                .Must(contentManager => contentManager)
                .WithMessage("Unauthorized to perform action")
                .WithErrorCode(HttpStatusCode.Forbidden.ToString());
        }
    }
}