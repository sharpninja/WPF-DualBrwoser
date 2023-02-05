namespace DualBrowser;

public record struct StatusEntry(DateTimeOffset Timestamp, string Status)
{
    public static implicit operator (DateTimeOffset Timestamp, string Status)(StatusEntry value)
    {
        return (value.Timestamp, value.Status);
    }

    public static implicit operator StatusEntry((DateTimeOffset Timestamp, string Status) value)
    {
        return new StatusEntry(value.Timestamp, value.Status);
    }
}
