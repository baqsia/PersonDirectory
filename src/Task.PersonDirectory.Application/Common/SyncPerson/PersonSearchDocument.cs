using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.Common.SyncPerson;

public class PersonSearchDocument
{
    public int PersonId { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string PersonalNumber { get; init; } = null!;
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public int CityId { get; init; }
    public string? ImageUrl { get; init; }

    public List<PhoneNumberDto> PhoneNumbers { get; init; } = [];
    public List<RelatedPersonDto> Relations { get; init; } = [];
}