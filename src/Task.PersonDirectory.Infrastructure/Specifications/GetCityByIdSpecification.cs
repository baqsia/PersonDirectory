using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;

namespace Task.PersonDirectory.Infrastructure.Specifications;

public class GetCityByIdSpecification : Specification<City>
{
    public GetCityByIdSpecification(int id)
    {
        SetCriteria(p => p.Id == id);
    }
}