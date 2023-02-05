using System.Diagnostics.CodeAnalysis;

namespace DualBrowser;

public class HistoryEntryComparer :
    IComparer<HistoryEntry>,
    IEqualityComparer<HistoryEntry>
{
    public int Compare(HistoryEntry x, HistoryEntry y)
        => Comparer<string>.Default.Compare(new Uri(x.Uri).Host, new Uri(y.Uri).Host);

    public bool Equals(HistoryEntry x, HistoryEntry y)
        => string.Equals(new Uri(x.Uri).Host, new Uri(y.Uri).Host);

    public int GetHashCode([DisallowNull] HistoryEntry obj)
        => new Uri(obj.Uri).Host.GetHashCode();
}
