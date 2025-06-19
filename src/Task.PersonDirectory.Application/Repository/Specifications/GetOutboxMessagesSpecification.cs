using Task.PersonDirectory.Domain;

namespace Task.PersonDirectory.Application.Repository.Specifications;

public class GetOutboxMessagesSpecification: Specification<OutboxMessage>
{
    public GetOutboxMessagesSpecification()
    {
        SetCriteria(a => a.ProcessedOn == null);
    }   
}