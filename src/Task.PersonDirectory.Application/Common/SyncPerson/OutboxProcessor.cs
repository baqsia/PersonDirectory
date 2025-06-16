using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Infrastructure;
using Task.PersonDirectory.Infrastructure.Context;

namespace Task.PersonDirectory.Application.Common.SyncPerson;

public class OutboxProcessor(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PersonDirectoryContext>();
            var elastic = scope.ServiceProvider.GetRequiredService<IPersonSearchIndexer>();

            var messages = await db.Set<OutboxMessage>()
                .Where(m => m.ProcessedOn == null)
                .OrderBy(m => m.OccurredOn)
                .Take(20)
                .AsNoTracking()
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                switch (message.Type)
                {
                    case nameof(PersonCreated): await IndexPerson(elastic, message.Payload, stoppingToken); break;
                    case nameof(PersonUpdated): await UpdatePersonIndex(elastic, message.Payload, stoppingToken); break;
                    case nameof(PersonDeleted): await DropPersonIndex(elastic, message.Payload, stoppingToken); break;
                }

                message.ProcessedOn = DateTime.UtcNow;
            }

            var ids = messages.Select(a => a.Id).ToArray();
            await db.Set<OutboxMessage>()
                .Where(a => ids.Contains(a.Id))
                .ExecuteUpdateAsync(a =>
                        a.SetProperty(p => p.ProcessedOn, DateTime.UtcNow),
                    cancellationToken: stoppingToken
                );

            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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