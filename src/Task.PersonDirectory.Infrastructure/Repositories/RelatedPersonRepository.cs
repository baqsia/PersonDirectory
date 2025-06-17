using Microsoft.EntityFrameworkCore;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;
using Task.PersonDirectory.Infrastructure.Context;
using Task.PersonDirectory.Infrastructure.Repositories.Segregation;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Infrastructure.Repositories;

public interface IRelatedPersonRepository : IReadOneRepository<RelatedPerson>, IDeleteRepository<RelatedPerson>,
    ICreateRepository<RelatedPerson>
{
    Task<List<(int PersonId, RelatedPersonConnection Connection, int Count)>> GetGroupedByConnectionTypeAsync(
        CancellationToken cancellationToken
    );
}

public class RelatedPersonRepository(PersonDirectoryContext context)
    : BaseRepository<RelatedPerson>, IRelatedPersonRepository
{
    private DbSet<RelatedPerson> RelatedPersons => context.Set<RelatedPerson>();

    public Task<RelatedPerson?> GetBySpecificationAsync(ISpecification<RelatedPerson> specification,
        CancellationToken cancellationToken = default)
        => ApplySpecification(RelatedPersons, specification).FirstOrDefaultAsync(cancellationToken);

    public System.Threading.Tasks.Task AddAsync(RelatedPerson relatedPerson,
        CancellationToken cancellationToken = default)
        => RelatedPersons.AddAsync(relatedPerson, cancellationToken).AsTask();

    public void Remove(RelatedPerson relatedPerson)
        => RelatedPersons.Remove(relatedPerson);

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