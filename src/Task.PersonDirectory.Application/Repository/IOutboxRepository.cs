using Task.PersonDirectory.Application.Repository.Specifications;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Application.Repository;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> GetAllAsync(ISpecification<OutboxMessage> specification, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task ProcessMessagesOnAsync(DateTime utcNow, Guid[] ids, CancellationToken cancellationToken);
    System.Threading.Tasks.Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);
}