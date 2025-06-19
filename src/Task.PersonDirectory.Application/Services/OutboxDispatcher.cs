using System.Text.Json;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Domain;

namespace Task.PersonDirectory.Application.Services;

public interface IOutboxDispatcher
{
    System.Threading.Tasks.Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class;
}

public class OutboxDispatcher(IOutboxRepository outboxRepository) : IOutboxDispatcher
{
    public async System.Threading.Tasks.Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class
    {
        await outboxRepository.AddAsync(
            new OutboxMessage
            {
                Type = @event.GetType().Name,
                OccurredOn = DateTime.Now,
                Payload = JsonSerializer.Serialize(@event),
            }, cancellationToken
        );
    }
}