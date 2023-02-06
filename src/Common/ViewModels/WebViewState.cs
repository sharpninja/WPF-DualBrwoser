#pragma warning disable CS8603 // Possible null reference return.
using DualBrowser.Models;
using DualBrowser.Views;

using Microsoft.UI.Dispatching;

using DispatcherQueueHandler = Microsoft.UI.Dispatching.DispatcherQueueHandler;

namespace DualBrowser.ViewModels;

public partial class WebViewState : ObservableObject, IWebViewStateView
{
    private MainViewModel _viewModel;
    private Border _border;
    private Guid _viewKey = Guid.NewGuid();
    private WebView2 _webView;
    private string? _Uri;
    private string? _lastNavigatedUri;
    private int? _StatusCode;
    private CoreWebView2WebErrorStatus _error;
    private string _Status = "";
    private Visibility _sideBarVisibility = Visibility.Collapsed;

    private DispatcherQueue DispatcherQueue => ((IDispatcher)Application.Current).DispatcherQueue;

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

        PropertyChanged += This_PropertyChanged;
    }

    public void SetPosition()
    {
        WebView.DispatcherQueue.TryEnqueue(() =>
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

    private void This_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
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
        WebView.DispatcherQueue.TryEnqueue(() =>
        {
            WebView.GoBack();
        });
    }

    public void GoForward()
    {
        WebView.DispatcherQueue.TryEnqueue(() =>
        {
            WebView.GoForward();
        });
    }

    public void Reload()
    {
        WebView.DispatcherQueue.TryEnqueue(() =>
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

        WebView.DispatcherQueue.TryEnqueue(() =>
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

    public bool GetCanGoBack()
    {
        if (((IDispatcher)Application.Current).DispatcherQueue is null ||
            WebView.CoreWebView2 is null)
        {
            return false;
        }

        ManualResetEvent mre = new(false);
        ((IDispatcher)Application.Current).DispatcherQueue!.TryEnqueue(
            DispatcherQueuePriority.High,
            () =>
        {
            CanGoBack = WebView.CanGoBack;
            mre.Set();
        });

        if (mre.WaitOne(TimeSpan.FromSeconds(10)))
        {
            return CanGoBack;
        }

        return false;
    }

    public bool GetCanGoForward()
    {
        if (((IDispatcher)Application.Current).DispatcherQueue is null ||
            WebView.CoreWebView2 is null)
        {
            return false;
        }

        ManualResetEvent mre = new(false);
        ((IDispatcher)Application.Current).DispatcherQueue!.TryEnqueue(
            DispatcherQueuePriority.High,
            () =>
            {
                CanGoForward = WebView.CanGoForward;
                mre.Set();
            });

        if (mre.WaitOne(TimeSpan.FromSeconds(10)))
        {
            return CanGoForward;
        }

        return false;
    }

    [JsonIgnore]
    public bool CanGoBack { get; set; }
    [JsonIgnore]
    public bool CanGoForward { get; set; }

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
        get => _Uri;
        set => SetProperty(ref _Uri, value);
    }
    [JsonProperty]
    public string? LastNavigatedUri { get => _lastNavigatedUri; set => SetProperty(ref _lastNavigatedUri, value); }
    [JsonProperty]
    public int? StatusCode { get => _StatusCode; set => SetProperty(ref _StatusCode, value); }
    [JsonProperty]
    public string Status { get => _Status; set => SetProperty(ref _Status, value); }
    [JsonProperty]
    public Visibility SideBarVisibility { get => _sideBarVisibility; set => SetProperty(ref _sideBarVisibility, value); }
}
#pragma warning restore CS8603 // Possible null reference return.
