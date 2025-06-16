using Task.PersonDirectory.Domain;

namespace Task.PersonDirectory.Infrastructure.Specifications;

public class GetCityByIdSpecification : Specification<City>
{
    public GetCityByIdSpecification(int id)
    {
        SetCriteria(p => p.Id == id);
    }
}