using System.Collections.ObjectModel;
using System.Diagnostics;
using DualBrowser.ViewModels;

namespace SfDualBrowserWpf;

/// <summary>
/// Interaction logic for SideBar.xaml
/// </summary>
public partial class SideBar
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MainViewModel),
            typeof(SideBar),
            new((o, e) =>
            {
                if (o is SideBar a && e.NewValue is MainViewModel vm)
                {
                    vm.PropertyChanged += (_, e) =>
                    {
                        if (e.PropertyName == a.Position)
                        {
                            o.SetValue(StateProperty, (a.Position) switch
                            {
                                "Primary" => vm?.Primary,
                                "Secondary" => vm?.Secondary,
                                _ => vm?.Primary,
                            });
                        }
                    };

                    o.SetValue(StateProperty, (a.Position) switch
                    {
                        "Primary" => vm?.Primary,
                        "Secondary" => vm?.Secondary,
                        _ => vm?.Primary,
                    });
                }
            }));

    public MainViewModel? ViewModel
    {
        get => Dispatcher.Invoke(() => (MainViewModel?)GetValue(ViewModelProperty));
        set => Dispatcher.Invoke(() => SetValue(ViewModelProperty, value));
    }

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(
            nameof(State),
            typeof(WebViewState),
            typeof(SideBar));

    public WebViewState? State
    {
        get => Dispatcher.Invoke(() => (WebViewState?)GetValue(StateProperty));
        set => Dispatcher.Invoke(() => SetValue(StateProperty, value));
    }

    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(
            nameof(Position),
            typeof(string),
            typeof(SideBar));

    public string? Position
    {
        get => Dispatcher.Invoke<string?>(() => (string?)GetValue(PositionProperty));
        set => Dispatcher.Invoke(() => SetValue(PositionProperty, value));
    }

    public Commands? Commands => ViewModel?.Commands;

    public SideBar()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            SetHistory();

            State!.History.CollectionChanged += (o, e) =>
            {
                Debugger.Break();
                SetHistory();
            };
        };
    }

    private ObservableCollection<HistoryEntry>? SetHistory()
    {
        if (LatestHistory is not null)
        {
            var history = new ObservableCollection<HistoryEntry>(LatestHistory);

            if (history is not null)
            {
                History = history;

                Console.WriteLine($"History.Count: {History.Count}");

                return history;
            }
        }
        else{
            Debug.WriteLine($"State not yet initialized.");
        }

        return null;
    }

    public static readonly DependencyProperty HistoryProperty =
        DependencyProperty.Register(nameof(History), typeof(ObservableCollection<HistoryEntry>), typeof(SideBar));

    public ObservableCollection<HistoryEntry>? History
    {
        get => (ObservableCollection<HistoryEntry>?)GetValue(HistoryProperty) ?? SetHistory();
        set => SetValue(HistoryProperty, value);
    }

    public static readonly DependencyProperty SelectedHistoryProperty =
        DependencyProperty.Register(nameof(SelectedHistory), typeof(HistoryEntry), typeof(SideBar),
        new PropertyMetadata((o,e)=>{
            if(o is SideBar sb && e.NewValue is HistoryEntry he)
            {
                sb.State!.Uri = he.Uri;
                sb.Commands?.GoCommand.ExecuteAsync(sb.Position);
            }
        }));

    public HistoryEntry SelectedHistory
    {
        get => (HistoryEntry)GetValue(SelectedHistoryProperty);
        set => SetValue(SelectedHistoryProperty, value);
    }

    public IEnumerable<HistoryEntry>? LatestHistory
    {
        get
        {
            var history = State?
                .History
                .Where(h => h.Uri is not (null or ""))
                .OrderBy(h => new Uri(h.Uri).Host)
                .ThenBy(h => h.Timestamp)
                .Distinct(new HistoryEntryComparer());

#if DEBUG
            history?.ToList().ForEach(h => Debug.WriteLine(h));
#endif

            return history;
        }
    }
}
