using System.Security.Policy;

using Microsoft.Web.WebView2.Core;

namespace DualBrowser;

public record struct HistoryEntry(DateTimeOffset Timestamp, string Uri, int StatusCode, string Status)
{
    public HistoryEntry(string url, CoreWebView2NavigationCompletedEventArgs e) :
        this(DateTimeOffset.UtcNow,
            url,
            e.HttpStatusCode,
            e.WebErrorStatus.ToString())
    {
    }

    public string? Hostname => Uri is not (null or "") ? new Uri(Uri).Host : null;

    public static implicit operator (DateTimeOffset Timestamp, string Uri, int StatusCode, string Status)(HistoryEntry value)
    {
        return (value.Timestamp, value.Uri, value.StatusCode, value.Status);
    }

    public static implicit operator HistoryEntry((DateTimeOffset Timestamp, string Uri, int StatusCode, string Status) value)
    {
        return new HistoryEntry(value.Timestamp, value.Uri, value.StatusCode, value.Status);
    }
}
