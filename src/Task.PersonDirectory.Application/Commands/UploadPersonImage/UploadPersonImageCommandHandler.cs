using Mediator;
using OneOf;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Application.Commands.UploadPersonImage;

public class UploadPersonImageCommandHandler(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IImageStorage imageStorage,
    IOutboxDispatcher dispatcher
) : IRequestHandler<UploadPersonImageCommand, OneOf<PersonNotFound, ResponseResult<bool>>>
{
    public async ValueTask<OneOf<PersonNotFound, ResponseResult<bool>>> Handle(UploadPersonImageCommand request,
        CancellationToken cancellationToken)
    {
        var person = await personRepository.GetBySpecificationAsync(new GetPersonByIdSpecification(request.PersonId), cancellationToken);
        if (person is null)
            return new PersonNotFound(request.PersonId);
        
        var relativePath = await imageStorage.SaveAsync(person.Id, request.File, cancellationToken);
        person.ImagePath = relativePath;
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
                relativePath,
                person.PhoneNumbers.Select(pn => pn.ToDto()).ToList(),
                person.RelatedPersons.Select(rp => rp.ToDto()).ToList()
            ),
            cancellationToken
        );
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new ResponseResult<bool>(true);
    }
}