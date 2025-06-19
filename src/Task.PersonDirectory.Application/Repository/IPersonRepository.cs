
using Task.PersonDirectory.Application.Repository.Specifications;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Application.Repository;

public interface IPersonRepository
{
    Task<Person?> GetBySpecificationAsync(ISpecification<Person> specification, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<Person> specification, CancellationToken cancellationToken);
    System.Threading.Tasks.Task AddAsync(Person person, CancellationToken cancellationToken = default);
    void Update(Person person);

    void Remove(Person person);
    Task<List<Person>> GetAllAsync(ISpecification<Person> specification, CancellationToken cancellationToken = default);
}