using Mediator;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using OneOf;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Infrastructure.Repositories;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Application.Queries.GetPerson;

public class GetPersonByIdQueryHandler(
    IPersonRepository personRepository,
    IImageStorage imageStorage
) : IRequestHandler<GetPersonByIdQuery, OneOf<PersonNotFound, ResponseResult<PersonDto>>>
{
    public async ValueTask<OneOf<PersonNotFound, ResponseResult<PersonDto>>> Handle(GetPersonByIdQuery request,
        CancellationToken cancellationToken)
    {
        var person = await personRepository.GetBySpecificationAsync(
            new GetPersonByIdSpecification(request.PersonId)
                .IncludeRelatedPersons(),
            cancellationToken
        );

        if (person is null)
            return new PersonNotFound(request.PersonId);

        var dto = new PersonDto(
            Id: person.Id,
            FirstName: person.FirstName,
            LastName: person.LastName,
            Gender: person.Gender,
            PersonalNumber: person.PersonalNumber,
            DateOfBirth: person.DateOfBirth,
            CityId: person.CityId,
            Image: await imageStorage.LoadBase64Async(person.ImagePath, cancellationToken),
            PhoneNumbers: person.PhoneNumbers
                .Select(pn => pn.ToDto())
                .ToList(),
            RelatedPersons: person.RelatedPersons
                .Select(rp => rp.ToDto())
                .ToList()
        );

        return new ResponseResult<PersonDto>(dto);
    }
}