namespace DualBrowser;

public record struct StatusEntry(DateTimeOffset timestamp, string status)
{
    public static implicit operator (DateTimeOffset timestamp, string status)(StatusEntry value)
    {
        return (value.timestamp, value.status);
    }

    public static implicit operator StatusEntry((DateTimeOffset timestamp, string status) value)
    {
        return new StatusEntry(value.timestamp, value.status);
    }
}