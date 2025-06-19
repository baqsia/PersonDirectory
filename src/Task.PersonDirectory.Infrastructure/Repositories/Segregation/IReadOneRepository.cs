using Task.PersonDirectory.Application.Repository.Specifications;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Infrastructure.Repositories.Segregation;

public interface IReadOneRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetBySpecificationAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
}