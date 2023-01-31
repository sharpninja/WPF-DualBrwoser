using System.Collections.ObjectModel;

using Microsoft.Web.WebView2.Wpf;

namespace DualBrowser;

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
