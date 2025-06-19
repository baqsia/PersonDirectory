using Task.PersonDirectory.Application.Repository.Specifications;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;

namespace Task.PersonDirectory.Infrastructure.Specifications;

public class GetRelatedPersonByPersonAndRelatedPersonIdSpecification: Specification<RelatedPerson>
{
    public GetRelatedPersonByPersonAndRelatedPersonIdSpecification(int personId, int relatedPersonId)
    {
        SetCriteria(p => p.Id == personId && p.RelatedToId == relatedPersonId);
    }
}