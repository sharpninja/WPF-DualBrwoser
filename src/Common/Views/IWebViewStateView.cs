using System.Collections.ObjectModel;

using DualBrowser.Models;

namespace DualBrowser.Views;

public interface IWebViewStateView : ISimpleView
{
    int Column { get; set; }
    int ColumnSpan { get; set; }
    WebView2 WebView { get; }
    ObservableCollection<HistoryEntry> History { get; }
    ObservableCollection<StatusEntry> Log { get; }
    WebViewPositions Position { get; }

    void SetPosition();
}
