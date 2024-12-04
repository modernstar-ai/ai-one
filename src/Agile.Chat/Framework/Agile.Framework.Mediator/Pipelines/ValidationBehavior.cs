using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Agile.Framework.Mediator.Pipelines;

public class ErrorResult
{
    public string Key { get; set; }
    public string Value { get; set; }
    public string Code { get; set; }
}
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, IResult> 
        where TRequest : notnull, IRequest<IResult>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<IResult> Handle(TRequest request, RequestHandlerDelegate<IResult> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);
            var errors = _validators
                .Select(async x => await x.ValidateAsync(context, cancellationToken))
                .SelectMany(x => x.Result.Errors)
                .Where(x => x != null)
                .GroupBy(
                    x => x.PropertyName,
                    x => x,
                    (propertyName, validationFailure) => new ErrorResult
                    {
                        Key = propertyName,
                        Value = validationFailure.FirstOrDefault()?.ErrorMessage,
                        Code = validationFailure.FirstOrDefault()?.ErrorCode
                    })
                .ToList();
            
            if (errors.Count > 0)
                return ConvertErrorToResult(errors[0]);
            
            return await next();
        }

        private IResult ConvertErrorToResult(ErrorResult result)
        {
            return result.Code switch
            {
                "403" => Results.Unauthorized(),
                _ => Results.BadRequest(result.Value)
            };
        }
    }