using System.Text.Json;
using Task.PersonDirectory.Infrastructure.Context;

namespace Task.PersonDirectory.Application.Services;

public interface IOutboxDispatcher
{
    System.Threading.Tasks.Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class;
}

public class OutboxDispatcher(PersonDirectoryContext context) : IOutboxDispatcher
{
    public async System.Threading.Tasks.Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class
    {
        await context.Set<OutboxMessage>()
            .AddAsync(new OutboxMessage
            {
                Type = @event.GetType().Name,
                OccurredOn = DateTime.Now,
                Payload = JsonSerializer.Serialize(@event),
            }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}