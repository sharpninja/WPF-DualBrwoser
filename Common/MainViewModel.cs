using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Input;

using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace DualBrowser;

public sealed partial class MainViewModel : ObservableObject
{
    private const string DEFAULT_SPLIT = "7";
    [ObservableProperty]
    private GridLength _splitLeft = new GridLength(5, GridUnitType.Star);
    [ObservableProperty]
    private GridLength _splitRight = new GridLength(5, GridUnitType.Star);

    [ObservableProperty]
    private bool _isSwapped = false;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(ModeText))]
    private Modes _mode;
    [ObservableProperty]
    private int _columnSpan = 1;
    [ObservableProperty]
    private Visibility _secondaryToolbarVisibility = Visibility.Visible;
    [ObservableProperty]
    private Visibility _secondaryBrowserVisibility = Visibility.Visible;
    [ObservableProperty]
    private string _split = "70:30";

    [ObservableProperty]
    private WebViewState _primary;
    [ObservableProperty]
    private WebViewState _secondary;

    public DualBrowserController Controller { get; }

    public Commands Commands { get; }

    public MainViewModel(DualBrowserController controller)
    {
        Controller = controller;
        Commands = new(this);
    }

    public void Initialize(Border primaryBorder, WebView2 primary, Border secondaryBorder, WebView2 secondary)
    {
        Primary = new(this, WebViewPositions.Primary, primaryBorder, primary);
        Secondary = new(this, WebViewPositions.Secondary, secondaryBorder, secondary);

        PropertyChanged += MainViewModel_PropertyChanged;

        Primary.PropertyChanged += State_PropertyChanged;
        Secondary.PropertyChanged += State_PropertyChanged;

        Commands.SetSplitCommand.Execute(DEFAULT_SPLIT);
    }

    private void State_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Mode):
                SetMode(Mode);
                break;

            case nameof(Split):
                Commands.SetSplitCommand.Execute(Split[0..1]);
                break;
        }
    }

    public string ModeText => $"{Mode}";

    public Modes[] ModeList => Enum.GetValues<Modes>();

    private readonly string[] SPLIT_LIST = new string[] { "70:30", "50:50", "30:70" };

    public string[] SplitList => SPLIT_LIST;

    public string? PrimaryHome { get; set; } = "https://scholar.google.com";
    public string? SecondaryHome { get; set; } = "https://www.noteshub.app/";

    internal void SetMode(Modes mode)
    {
        Mode = mode;
        (SecondaryToolbarVisibility, SecondaryBrowserVisibility, ColumnSpan) =
            (mode switch
            {
                Modes.Dual => Visibility.Visible,
                Modes.Single or Modes.Synced => Visibility.Collapsed,
                _ => Visibility.Collapsed,
            },
            mode switch
            {
                Modes.Dual or Modes.Synced => Visibility.Visible,
                Modes.Single => Visibility.Collapsed,
                _ => Visibility.Collapsed,
            },
            mode switch
            {
                Modes.Dual => 1,
                Modes.Single => 2,
                Modes.Synced => 2,
                _ => 1,
            });

        _primary.SetPosition();
        _secondary.SetPosition();

        if (mode is Modes.Synced)
        {
            SyncBrowsers();
        }
        else if (mode is Modes.Dual)
        {
            Secondary.Navigate(Secondary.LastNavigatedUri);
        }
    }

    public void SyncBrowsers()
    {
        if (Mode is Modes.Synced && Secondary.Uri != Primary.Uri)
        {
            Secondary.WebView.CoreWebView2.Navigate(Primary.Uri);
        }
    }

    public event EventHandler? CloseBrowserRequested;

    public void InvokeCloseBrowser()
    {
        CloseBrowserRequested?.Invoke(this, EventArgs.Empty);
    }

    public void SetState(string json)
    {
        dynamic state;
        try
        {
            state = JsonConvert.DeserializeObject<dynamic>(json)!;
        }
        catch (Exception ex)
        {
            throw new JsonException(json, ex);
        }

        JObject? primary = state.Primary;
        JObject? secondary = state.Secondary;

        if (primary is not null)
        {
            try
            {
                WebViewState? primaryState = primary.ToObject<WebViewState>();

                if (primaryState is not null)
                {
                    primaryState.Initialize(
                        this,
                        Primary.Border,
                        Primary.WebView);

                    Primary = primaryState;
                }
            }
            catch
            {

            }
        }

        if (secondary is not null)
        {
            try
            {
                WebViewState? secondaryState = secondary.ToObject<WebViewState>();

                if (secondaryState is not null)
                {
                    secondaryState.Initialize(
                        this,
                        Secondary.Border,
                        Secondary.WebView);

                    Secondary = secondaryState;
                }
            }
            catch
            {

            }
        }

        Split = state.Split;

        Modes mode = (Modes)state.Mode;
        SetMode(mode);

        Primary.Navigate(Primary.LastNavigatedUri);
        Secondary.Navigate(Secondary.LastNavigatedUri);
    }

    public string GetState()
    {
        return JsonConvert.SerializeObject(new
        {
            Primary,
            Secondary,
            Mode,
            Split,
        }, Formatting.Indented);
    }
}

