using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.DTOs;

public record PersonDto(
    int Id,
    string FirstName,
    string LastName,
    Gender Gender,
    string PersonalNumber,
    DateTime DateOfBirth,
    int CityId,
    string? Image,
    List<PhoneNumberDto> PhoneNumbers,
    List<RelatedPersonDto> RelatedPersons
);
