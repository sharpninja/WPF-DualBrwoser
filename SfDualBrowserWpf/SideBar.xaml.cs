using System.Diagnostics;

namespace SfDualBrowserWpf;

/// <summary>
/// Interaction logic for SideBar.xaml
/// </summary>
public partial class SideBar
{
    public static DependencyProperty ViewModelProperty =
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

    public static DependencyProperty StateProperty =
        DependencyProperty.Register(
            nameof(State),
            typeof(WebViewState),
            typeof(SideBar));

    public WebViewState? State
    {
        get => Dispatcher.Invoke(() => (WebViewState?)GetValue(StateProperty));
        set => Dispatcher.Invoke(() => SetValue(StateProperty, value));
    }

    public static DependencyProperty PositionProperty =
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

        IsVisibleChanged += (_, _) =>
        {
            Debug.WriteLine(Visibility);
        };
    }
}
