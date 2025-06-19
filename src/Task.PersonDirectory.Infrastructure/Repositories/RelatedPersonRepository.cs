using Microsoft.EntityFrameworkCore;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;
using Task.PersonDirectory.Infrastructure.Context;

namespace Task.PersonDirectory.Infrastructure.Repositories;

public class RelatedPersonRepository(PersonDirectoryContext context)
    : BaseRepository<RelatedPerson>, IRelatedPersonRepository
{
    private DbSet<RelatedPerson> RelatedPersons => context.Set<RelatedPerson>();

    public async Task<List<(int PersonId, RelatedPersonConnection Connection, int Count)>>
        GetGroupedByConnectionTypeAsync(
            CancellationToken cancellationToken
        )
    {
        var query = context.Set<RelatedPerson>().AsQueryable();

        var result = await query
            .GroupBy(rp => new { rp.PersonId, rp.ConnectionType })
            .Select(g => new
            {
                g.Key.PersonId,
                g.Key.ConnectionType,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        return result.Select(a => (
                a.PersonId,
                a.ConnectionType,
                a.Count
            )
        ).ToList();
    }
}