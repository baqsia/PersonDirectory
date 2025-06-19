using Mediator;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Infrastructure;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Application.Commands.CreatePerson;

public class CreatePersonCommandHandler(
    IPersonRepository personRepository,
    ICityRepository cityRepository,
    IUnitOfWork unitOfWork,
    IOutboxDispatcher dispatcher
) : IRequestHandler<CreatePersonCommand, OneOf<CityNotFound,  ResponseResult<int>>>
{
    public async ValueTask<OneOf<CityNotFound,  ResponseResult<int>>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        var city = await cityRepository.GetBySpecificationAsync(new GetCityByIdSpecification(request.CityId), cancellationToken: cancellationToken);
        if (city is null)
            return new CityNotFound();

        var phoneNumbers = request.PhoneNumbers
            .Select(p => new PhoneNumber { Type = p.Type, Number = p.Number })
            .ToList();
        
        var person = Person.Create(
            request.FirstName,
            request.LastName,
            request.Gender,
            request.PersonalNumber,
            request.DateOfBirth,
            request.CityId
        ).WithNumbers(phoneNumbers);
        
        await personRepository.AddAsync(person, cancellationToken);
        
        await dispatcher.DispatchAsync(
            new PersonCreated(
                person.Id,
                person.FirstName,
                person.LastName,
                person.PersonalNumber,
                person.Gender,
                person.DateOfBirth,
                person.CityId,
                request.PhoneNumbers,
                []
            ),
            cancellationToken
        );
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new ResponseResult<int>(person.Id);
    }
}