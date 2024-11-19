using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Agile.Framework.Mediator.Pipelines;

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
                    x => x.ErrorMessage,
                    (propertyName, errorMessages) => new
                    {
                        Key = propertyName,
                        Values = errorMessages.Distinct().ToArray()
                    })
                .ToList();
            
            if (errors.Count > 0)
            {
                return errors[0].Values.Length > 0
                    ? Results.BadRequest(errors[0].Values[0])
                    : Results.BadRequest("Validation failed");
            }
            
            return await next();
        }
    }