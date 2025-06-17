using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.DTOs;

public record RelatedPersonTypeCountDto(
    int PersonId,
    Dictionary<RelatedPersonConnection, int> Counts
);
