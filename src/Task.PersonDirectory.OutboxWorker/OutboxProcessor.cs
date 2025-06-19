using System.Text.Json;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Repository.Specifications;

namespace Task.PersonDirectory.OutboxWorker;

public class OutboxProcessor(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var elastic = scope.ServiceProvider.GetRequiredService<IPersonSearchIndexer>();

            var messages = await outboxRepository.GetAllAsync(new GetOutboxMessagesSpecification(), cancellationToken);

            foreach (var message in messages)
            {
                switch (message.Type)
                {
                    case nameof(PersonCreated): await IndexPerson(elastic, message.Payload, cancellationToken); break;
                    case nameof(PersonUpdated): await UpdatePersonIndex(elastic, message.Payload, cancellationToken); break;
                    case nameof(PersonDeleted): await DropPersonIndex(elastic, message.Payload, cancellationToken); break;
                }

                message.ProcessedOn = DateTime.UtcNow;
            }

            var ids = messages.Select(a => a.Id).ToArray(); 
            await outboxRepository.ProcessMessagesOnAsync(DateTime.UtcNow, ids, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    private static async System.Threading.Tasks.Task DropPersonIndex(
        IPersonSearchIndexer elastic,
        string payload,
        CancellationToken cancellationToken
    )
    {
        var personDeleted = JsonSerializer.Deserialize<PersonDeleted>(payload);
        await elastic.DeleteAsync(personDeleted!, cancellationToken);
    }

    private static async System.Threading.Tasks.Task UpdatePersonIndex(
        IPersonSearchIndexer elastic,
        string payload,
        CancellationToken cancellationToken
    )
    {
        var personUpdated = JsonSerializer.Deserialize<PersonUpdated>(payload);
        await elastic.UpdateAsync(personUpdated!, cancellationToken);
    }

    private static async System.Threading.Tasks.Task IndexPerson(
        IPersonSearchIndexer indexer,
        string payload,
        CancellationToken cancellationToken
    )
    {
        var personCreated = JsonSerializer.Deserialize<PersonCreated>(payload);
        await indexer.IndexAsync(personCreated!, cancellationToken);
    }
}