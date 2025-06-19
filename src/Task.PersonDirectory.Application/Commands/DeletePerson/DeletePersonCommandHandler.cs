using Mediator;
using OneOf;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Application.Commands.DeletePerson;

public class DeletePersonCommandHandler(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IOutboxDispatcher dispatcher
) : IRequestHandler<DeletePersonCommand, OneOf<PersonNotFound, ResponseResult<bool>>>
{
    public async ValueTask<OneOf<PersonNotFound, ResponseResult<bool>>> Handle(DeletePersonCommand request,
        CancellationToken cancellationToken)
    {
        var person = await personRepository.GetBySpecificationAsync(new GetPersonByIdSpecification(request.PersonId), cancellationToken);

        if (person is null)
            return new PersonNotFound(request.PersonId);

        personRepository.Remove(person);
        
        await dispatcher.DispatchAsync(
            new PersonDeleted(person.Id),
            cancellationToken
        );
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new ResponseResult<bool>(true);
    }
}