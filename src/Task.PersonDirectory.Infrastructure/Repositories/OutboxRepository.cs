using Microsoft.EntityFrameworkCore;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Repository.Specifications;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Infrastructure.Context;

namespace Task.PersonDirectory.Infrastructure.Repositories;

public class OutboxRepository(PersonDirectoryContext context) : BaseRepository<OutboxMessage>, IOutboxRepository
{
    private DbSet<OutboxMessage> OutboxMessages => context.Set<OutboxMessage>();

    public Task<List<OutboxMessage>> GetAllAsync(ISpecification<OutboxMessage> specification,
        CancellationToken cancellationToken = default)
    {
        return ApplySpecification(OutboxMessages, specification)
            .OrderBy(a => a.OccurredOn)
            .Take(20)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task ProcessMessagesOnAsync(
        DateTime utcNow,
        Guid[] ids,
        CancellationToken cancellationToken
    )
    {
        await OutboxMessages
            .Where(a => ids.Contains(a.Id))
            .ExecuteUpdateAsync(a =>
                    a.SetProperty(p => p.ProcessedOn, DateTime.UtcNow),
                cancellationToken
            );
    }

    public async System.Threading.Tasks.Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        await OutboxMessages.AddAsync(outboxMessage, cancellationToken);
    }
}