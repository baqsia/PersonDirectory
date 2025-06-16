using Mediator;
using OneOf;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.Commands.UpdatePerson;

public record UpdatePersonCommand(
    int PersonId,
    string FirstName,
    string LastName,
    Gender Gender,
    string PersonalNumber,
    DateTime DateOfBirth,
    int CityId,
    List<PhoneNumberDto> PhoneNumbers
) : IRequest<OneOf<PersonNotFound, CityNotFound, ResponseResult<bool>>>;