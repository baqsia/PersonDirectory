using Microsoft.Extensions.Logging;
using Nest;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.Events;

namespace Task.PersonDirectory.Application.Services;

public interface IPersonSearchIndexer
{
    System.Threading.Tasks.Task IndexAsync(PersonCreated created, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(PersonUpdated updated, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAsync(PersonDeleted deleted, CancellationToken cancellationToken = default);
}

public class PersonSearchIndexer(
    IElasticClient elasticClient,
    ILogger<PersonSearchIndexer> logger
)
    : IPersonSearchIndexer
{
    public async System.Threading.Tasks.Task IndexAsync(PersonCreated created,
        CancellationToken cancellationToken = default)
    {
        var doc = MapToDocument(created);

        var response = await elasticClient.IndexAsync(doc, i => i
                .Index("persons")
                .Id(doc.PersonId),
            cancellationToken
        );

        if (!response.IsValid)
        {
            logger.LogError("Failed to index PersonCreated: {Reason}", response.OriginalException?.Message);
        }
    }

    public async System.Threading.Tasks.Task UpdateAsync(PersonUpdated updated,
        CancellationToken cancellationToken = default)
    {
        var doc = MapToDocument(updated);

        var response = await elasticClient.IndexAsync(doc, i => i
                .Index("persons")
                .Id(doc.PersonId),
            cancellationToken
        );

        if (!response.IsValid)
        {
            logger.LogError("Failed to update Person: {Reason}", response.OriginalException?.Message);
        }
    }

    public async System.Threading.Tasks.Task DeleteAsync(PersonDeleted deleted,
        CancellationToken cancellationToken = default)
    {
        var response = await elasticClient.DeleteAsync<PersonSearchDocument>(
            deleted.Id,
            d => d.Index("persons"),
            cancellationToken
        );

        if (!response.IsValid && response.Result != Result.NotFound)
        {
            logger.LogError("Failed to delete person {Id} from index: {Reason}", deleted.Id,
                response.OriginalException?.Message);
        }
    }

    private static PersonSearchDocument MapToDocument(PersonCreated source) => new()
    {
        PersonId = source.Id,
        FirstName = source.FirstName,
        LastName = source.LastName,
        PersonalNumber = source.PersonalNumber,
        Gender = source.Gender,
        DateOfBirth = source.DateOfBirth,
        CityId = source.CityId,
        Relations = source.Relations,
        PhoneNumbers = source.PhoneNumbers
    };

    private static PersonSearchDocument MapToDocument(PersonUpdated source) => new()
    {
        PersonId = source.Id,
        FirstName = source.FirstName,
        LastName = source.LastName,
        PersonalNumber = source.PersonalNumber,
        Gender = source.Gender,
        DateOfBirth = source.DateOfBirth,
        CityId = source.CityId,
        Relations = source.Relations,
        ImageUrl = source.ImagerUrl,
        PhoneNumbers = source.PhoneNumbers
    };
}