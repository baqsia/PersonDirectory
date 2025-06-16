using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.Events;

public record PersonCreated(
    int Id,
    string FirstName,
    string LastName,
    string PersonalNumber,
    Gender Gender,
    DateTime DateOfBirth,
    int CityId,
    List<PhoneNumberDto> PhoneNumbers,
    List<RelatedPersonDto> Relations
);