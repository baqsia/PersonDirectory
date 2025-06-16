using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Domain.Aggregates;

public class PhoneNumber
{
    public int Id { get; set; }
    public MobileType Type { get; set; } = default!; // mobile, office, home
    public string Number { get; set; } = default!;
}