using System.Linq.Expressions;
using Task.PersonDirectory.Application.Repository.Specifications;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Application.Repository;

public interface ICityRepository
{
    Task<City?> GetBySpecificationAsync(ISpecification<City> specification,
        CancellationToken cancellationToken = default);

    Task<City?> FindAsync(Expression<Func<City, bool>> predicate,
        CancellationToken cancellationToken = default);
}