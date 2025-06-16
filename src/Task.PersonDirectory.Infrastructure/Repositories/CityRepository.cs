using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Infrastructure.Context;
using Task.PersonDirectory.Infrastructure.Repositories.Segregation;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Infrastructure.Repositories;

public interface ICityRepository : IReadOneRepository<City>;

public class CityRepository(PersonDirectoryContext context) : BaseRepository<City>, ICityRepository
{
    private DbSet<City> Cities => context.Set<City>();

    public Task<City?> GetBySpecificationAsync(ISpecification<City> specification, CancellationToken cancellationToken = default)
        => ApplySpecification(Cities, specification).FirstOrDefaultAsync(cancellationToken);

    public Task<City?> FindAsync(Expression<Func<City, bool>> predicate,
        CancellationToken cancellationToken = default)
        => Cities.FirstOrDefaultAsync(predicate, cancellationToken);
}