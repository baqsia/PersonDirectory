using Task.PersonDirectory.Infrastructure.Context;

namespace Task.PersonDirectory.Infrastructure.Repositories;

public interface IUnitOfWork
{
    IPersonRepository Persons { get; }

    System.Threading.Tasks.Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork(
    PersonDirectoryContext context,
    IPersonRepository persons
) : IUnitOfWork
{
    public IPersonRepository Persons { get; } = persons;

    public System.Threading.Tasks.Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}