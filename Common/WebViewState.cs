using System.Collections.ObjectModel;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;

using Newtonsoft.Json;

using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace DualBrowser;

public partial class WebViewState : ObservableObject, IWebViewStateView
{
    private MainViewModel _viewModel;
    private Border _border;
    private Guid _viewKey = Guid.NewGuid();
    private WebView2 _webView;
    private string? _uri;
    private string? _lastNavigatedUri;
    private int? _statusCode;
    private CoreWebView2WebErrorStatus _error;
    private string _status = "";
    private Visibility _sideBarVisibility = Visibility.Collapsed;

    public WebViewState(
        MainViewModel viewModel,
        WebViewPositions position,
        Border border,
        WebView2 webView)
    {
        _viewModel = viewModel;
        Position = position;
        _border = border;
        WebView = webView;

        PropertyChanged += this_PropertyChanged;
    }

    public void SetPosition()
    {
        WebView.Dispatcher.Invoke(() =>
        {
            (Column, ColumnSpan) = (_viewModel.IsSwapped, _viewModel.Mode, Position) switch
            {
                (false, Modes.Dual or Modes.Synced, WebViewPositions.Primary) => (0, 1),
                (false, Modes.Single, WebViewPositions.Primary) => (0, 2),
                (false, Modes.Dual or Modes.Synced, WebViewPositions.Secondary) => (1, 1),
                (false, Modes.Single, WebViewPositions.Secondary) => (1, 1),
                (true, Modes.Dual or Modes.Synced, WebViewPositions.Secondary) => (0, 1),
                (true, Modes.Single, WebViewPositions.Secondary) => (0, 2),
                (true, Modes.Dual or Modes.Synced, WebViewPositions.Primary) => (1, 1),
                (true, Modes.Single, WebViewPositions.Primary) => (1, 1),
                (_, _, WebViewPositions.Primary) => (0, 1),
                (_, _, WebViewPositions.Secondary) => (1, 1),
                _ => (0, 1),
            };
        });
    }

    private void this_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Uri):
                _viewModel?.Commands.GoCommand.NotifyCanExecuteChanged();
                break;

            case nameof(Status):
                Log?.Add(new(DateTimeOffset.UtcNow, Status));
                break;
        }
    }

    public void GoBack()
    {
        WebView.Dispatcher.Invoke(() =>
        {
            WebView.GoBack();
        });
    }

    public void GoForward()
    {
        WebView.Dispatcher.Invoke(() =>
        {
            WebView.GoForward();
        });
    }

    public void Reload()
    {
        WebView.Dispatcher.Invoke(() =>
        {
            switch ((Position, _viewModel.Mode))
            {
                case (WebViewPositions.Primary, Modes.Synced):
                    WebView.Reload();
                    _viewModel.Secondary.Reload();
                    break;

                case (WebViewPositions.Secondary, Modes.Synced):
                    //_viewModel.Primary.Reload();
                    WebView.Reload();
                    break;

                default:
                    WebView.Reload();
                    break;
            }
        });
    }

    internal void Navigate(string? url)
    {
        if (url is null or "")
        {
            return;
        }

        WebView.Dispatcher.Invoke(() =>
        {
            if (WebView.CoreWebView2 is null)
            {
                return;
            }

            url ??= "about:blank";
            (bool, bool, bool, bool) matrix = (
                url.StartsWith("http://", StringComparison.OrdinalIgnoreCase),
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase),
                url.StartsWith("about:", StringComparison.OrdinalIgnoreCase),
                url.StartsWith("edge://", StringComparison.OrdinalIgnoreCase)
                );
            string uri = matrix switch
            {
                (false, false, false, false) => $"https://{url}",
                _ => url,
            };

            LastNavigatedUri = uri;
            WebView.CoreWebView2.Navigate(uri);
        });
    }

    internal void Initialize(MainViewModel mainViewModel, Border border, WebView2 webView)
    {
        _viewModel = mainViewModel;
        WebView = webView;
        _border = border;
    }

    internal void ToggleSideBar()
    {
        SideBarVisibility = SideBarVisibility switch
        {
            Visibility.Visible => Visibility.Collapsed,
            _ => Visibility.Visible,
        };
    }

    [JsonIgnore]
    public bool CanGoBack => WebView.Dispatcher.Invoke<bool>(() => WebView.CanGoBack);
    [JsonIgnore]
    public bool CanGoForward => WebView.Dispatcher.Invoke<bool>(() => WebView.CanGoForward);

    [JsonIgnore]
    public Border Border => _border;
    [JsonIgnore]
    public WebView2 WebView { get => _webView; set => SetProperty(ref _webView, value); }
    [JsonIgnore]
    public CoreWebView2WebErrorStatus Error { get => _error; set => SetProperty(ref _error, value); }

    [JsonProperty]
    public int Column
    {
        get => (int)_border.GetValue(Grid.ColumnProperty);
        set => _border?.SetValue(Grid.ColumnProperty, value);
    }

    [JsonProperty]
    public int ColumnSpan
    {
        get => (int)_border.GetValue(Grid.ColumnSpanProperty);
        set => _border?.SetValue(Grid.ColumnSpanProperty, value);
    }

    [JsonProperty]
    public WebViewPositions Position { get; set; }

    [JsonProperty]
    public ObservableCollection<HistoryEntry> History
    {
        get;
        set;
    } = new();

    [JsonIgnore]
    public ObservableCollection<StatusEntry> Log
    {
        get;
        set;
    } = new();

    [JsonProperty]
    public Guid ViewKey
    {
        get => _viewKey;
        set => SetProperty(ref _viewKey, value);
    }

    [JsonProperty]
    public string? Uri
    {
        get => _uri;
        set => SetProperty(ref _uri, value);
    }
    [JsonProperty]
    public string? LastNavigatedUri { get => _lastNavigatedUri; set => SetProperty(ref _lastNavigatedUri, value); }
    [JsonProperty]
    public int? StatusCode { get => _statusCode; set => SetProperty(ref _statusCode, value); }
    [JsonProperty]
    public string Status { get => _status; set => SetProperty(ref _status, value); }
    [JsonProperty]
    public Visibility SideBarVisibility { get => _sideBarVisibility; set => SetProperty(ref _sideBarVisibility, value); }
}
