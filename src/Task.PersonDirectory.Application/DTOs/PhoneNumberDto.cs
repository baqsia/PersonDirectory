using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.DTOs;

public record PhoneNumberDto(
    MobileType Type,
    string Number
);