using System.Linq.Expressions;

namespace Task.PersonDirectory.Infrastructure.Repositories.Segregation;

public interface ICreateRepository<T> where T : class
{
    System.Threading.Tasks.Task AddAsync(T entity, CancellationToken cancellationToken = default);
}