using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Infrastructure.Repositories.Segregation;

public interface IReadManyRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> GetAllAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
}