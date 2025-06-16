using Mediator;
using OneOf;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Infrastructure.Repositories;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Application.Commands.AddRelatedPerson;

public class AddRelatedPersonCommandHandler(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IOutboxDispatcher dispatcher
) : IRequestHandler<AddRelatedPersonCommand, OneOf<PersonNotFound, bool>>
{
    public async ValueTask<OneOf<PersonNotFound, bool>> Handle(
        AddRelatedPersonCommand request,
        CancellationToken cancellationToken)
    {
        var person = await personRepository.GetBySpecificationAsync(
            new GetPersonByIdSpecification(request.PersonId)
                .IncludeRelatedPersons(),
            cancellationToken
        );
        if (person is null)
            return new PersonNotFound(request.PersonId);

        var relatedPerson = await personRepository.GetBySpecificationAsync(
            new GetPersonByIdSpecification(request.Relate.PersonId),
            cancellationToken
        );
        if (relatedPerson is null)
            return new PersonNotFound(request.Relate.PersonId);

        var result = person.ApplyRelation(
            relatedPerson,
            request.Relate.Connection
        );

        personRepository.Update(person);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
                person.PhoneNumbers.Select(pn => pn.ToDto()).ToList(),
                person.RelatedPersons.Select(rp => rp.ToDto()).ToList()
            ),
            cancellationToken
        );

        return result;
    }
}