using Task.PersonDirectory.Application.Events;

namespace Task.PersonDirectory.OutboxWorker;

public interface IPersonSearchIndexer
{
    System.Threading.Tasks.Task IndexAsync(PersonCreated created, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(PersonUpdated updated, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAsync(PersonDeleted deleted, CancellationToken cancellationToken = default);
}