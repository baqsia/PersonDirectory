using Microsoft.EntityFrameworkCore;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Repository.Specifications;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Infrastructure.Context;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Infrastructure.Repositories;

public class PersonRepository(PersonDirectoryContext context) : BaseRepository<Person>, IPersonRepository
{
    private DbSet<Person> Persons => context.Set<Person>();

    public Task<Person?> GetBySpecificationAsync(ISpecification<Person> specification,
        CancellationToken cancellationToken = default)
        => ApplySpecification(Persons, specification).FirstOrDefaultAsync(cancellationToken);

    public System.Threading.Tasks.Task AddAsync(Person person, CancellationToken cancellationToken = default)
        => Persons.AddAsync(person, cancellationToken).AsTask();

    public void Update(Person person)
        => Persons.Update(person);

    public void Remove(Person person)
        => Persons.Remove(person);

    public Task<int> CountAsync(ISpecification<Person> specification, CancellationToken cancellationToken)
        => ApplySpecification(Persons, specification).CountAsync(cancellationToken);
    
    public Task<List<Person>> GetAllAsync(ISpecification<Person> specification,
        CancellationToken cancellationToken = default)
        => ApplySpecification(Persons, specification).ToListAsync(cancellationToken);
}