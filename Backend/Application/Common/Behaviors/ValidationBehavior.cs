
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Common.Behaviors;

// A MediatR pipeline behavior that runs every registered FluentValidation validator
// for the incoming request BEFORE the handler executes. If any rule fails, it throws

public class ValidationBehavior<TRequest, TResponse> :
                            IPipelineBehavior<TRequest, TResponse>
                            where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();
        
        // Wraps the request in the object FluentValidation expects.
        var context = new ValidationContext<TRequest>(request);

        // Run all validators, collect every failure into one flat list
        var failures = new List<ValidationFailure>();
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
