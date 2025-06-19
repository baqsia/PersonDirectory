using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.Repository;

public interface IRelatedPersonRepository
{
    Task<List<(int PersonId, RelatedPersonConnection Connection, int Count)>> GetGroupedByConnectionTypeAsync(
        CancellationToken cancellationToken
    );
}