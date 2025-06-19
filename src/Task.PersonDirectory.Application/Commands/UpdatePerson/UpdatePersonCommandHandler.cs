using Mediator;
using OneOf;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Application.Commands.UpdatePerson;

public class UpdatePersonCommandHandler(
    IPersonRepository personRepository,
    ICityRepository cityRepository,
    IUnitOfWork unitOfWork,
    IOutboxDispatcher dispatcher
) : IRequestHandler<UpdatePersonCommand, OneOf<PersonNotFound, CityNotFound, ResponseResult<bool>>>
{
    public async ValueTask<OneOf<PersonNotFound, CityNotFound, ResponseResult<bool>>> Handle(
        UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = await personRepository.GetBySpecificationAsync(
            new GetPersonByIdSpecification(request.PersonId)
                .IncludePhoneNumbers(),
            cancellationToken
        );

        if (person is null)
            return new PersonNotFound(request.PersonId);

        var city = await cityRepository.GetBySpecificationAsync(new GetCityByIdSpecification(request.CityId),
            cancellationToken: cancellationToken);
        if (city is null)
            return new CityNotFound();

        person = person.Modify(
            request.FirstName,
            request.LastName,
            request.Gender,
            request.PersonalNumber,
            request.DateOfBirth,
            request.CityId
        ).WithNumbers(request.PhoneNumbers.Select(pn => pn.ToPhoneNumber()).ToList());

        personRepository.Update(person);

        await dispatcher.DispatchAsync(
            new PersonUpdated(
                person.Id,
                person.FirstName,
                person.LastName,
                person.PersonalNumber,
                person.Gender,
                person.DateOfBirth,
                person.CityId,
                person.ImagePath,
                request.PhoneNumbers,
                person.RelatedPersons.Select(rp => rp.ToDto()).ToList()
            ),
            cancellationToken
        );
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ResponseResult<bool>(true);
        ;
    }
}