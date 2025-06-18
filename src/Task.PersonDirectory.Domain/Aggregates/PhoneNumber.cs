using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Domain.Aggregates;

public class PhoneNumber
{
    public int Id { get; init; }
    public MobileType Type { get; init; }
    public string Number { get; init; } = null!;
}