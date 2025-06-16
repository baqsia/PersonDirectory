using Mediator;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using OneOf;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.Commands.CreatePerson;

public record CreatePersonCommand(
    string FirstName,
    string LastName,
    Gender Gender,
    string PersonalNumber,
    DateTime DateOfBirth,
    int CityId,
    List<PhoneNumberDto> PhoneNumbers
) : IRequest<OneOf<CityNotFound, ResponseResult<int>>>;