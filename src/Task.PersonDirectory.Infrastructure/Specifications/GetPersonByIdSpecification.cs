using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;

namespace Task.PersonDirectory.Infrastructure.Specifications;

public class GetPersonByIdSpecification : Specification<Person>
{
    public GetPersonByIdSpecification(int id)
    {
        SetCriteria(p => p.Id == id);
    }

    public ISpecification<Person> IncludePhoneNumbers()
    {
        AddInclude(a => a.PhoneNumbers);
        return this;
    }
    
    public ISpecification<Person> IncludeRelatedPersons()
    {
        AddInclude(a => a.RelatedPersons);
        AddInclude("RelatedPersons.RelatedTo");
        return this;
    }
}