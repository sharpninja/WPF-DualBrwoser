using System.Security.Policy;

using Microsoft.Web.WebView2.Core;

namespace DualBrowser;

public record struct HistoryEntry(DateTimeOffset timestamp, string uri, int statusCode, string status)
{
    public HistoryEntry(string url, CoreWebView2NavigationCompletedEventArgs e) :
        this(DateTimeOffset.UtcNow,
            url,
            e.HttpStatusCode,
            e.WebErrorStatus.ToString())
    {
    }

    public static implicit operator (DateTimeOffset timestamp, string uri, int statusCode, string status)(HistoryEntry value)
    {
        return (value.timestamp, value.uri, value.statusCode, value.status);
    }

    public static implicit operator HistoryEntry((DateTimeOffset timestamp, string uri, int statusCode, string status) value)
    {
        return new HistoryEntry(value.timestamp, value.uri, value.statusCode, value.status);
    }
}