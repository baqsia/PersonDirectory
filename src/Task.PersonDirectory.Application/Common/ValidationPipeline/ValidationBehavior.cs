using FluentValidation;
using Mediator;

namespace Task.PersonDirectory.Application.Common.ValidationPipeline;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TRequest, TResponse> next
    )
    {
        if (!validators.Any())
            return await next(message, cancellationToken);

        var context = new ValidationContext<TRequest>(message);

        var failures = (await System.Threading.Tasks.Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next(message, cancellationToken);
    }
}