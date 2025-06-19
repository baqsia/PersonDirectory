namespace Task.PersonDirectory.Application.Repository;

public interface IUnitOfWork
{
    IPersonRepository Persons { get; }

    System.Threading.Tasks.Task SaveChangesAsync(CancellationToken cancellationToken = default);
}