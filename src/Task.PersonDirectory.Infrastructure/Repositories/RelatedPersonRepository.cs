using Microsoft.EntityFrameworkCore;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Infrastructure.Context;
using Task.PersonDirectory.Infrastructure.Repositories.Segregation;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Infrastructure.Repositories;

public interface IRelatedPersonRepository : IReadOneRepository<RelatedPerson>, IDeleteRepository<RelatedPerson>,
    ICreateRepository<RelatedPerson>;

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
}