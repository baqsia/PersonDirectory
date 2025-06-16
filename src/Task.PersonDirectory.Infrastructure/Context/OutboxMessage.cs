namespace Task.PersonDirectory.Infrastructure.Context;

public class OutboxMessage
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
}