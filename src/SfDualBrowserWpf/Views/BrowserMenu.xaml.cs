using System.CodeDom;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

using DualBrowser.ViewModels;

using Syncfusion.Data.Extensions;
using Syncfusion.Windows.Shared;

namespace SfDualBrowserWpf;

/// <summary>
/// Interaction logic for BrowserMenu.xaml
/// </summary>
public partial class BrowserMenu
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MainViewModel),
            typeof(BrowserMenu),
            new((o, e) =>
            {
                if (o is not BrowserMenu a || e.NewValue is not MainViewModel vm) return;

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

                o.SetValue(CommandsProperty, vm.Commands);

                o.SetValue(StateProperty, (a.Position) switch
                {
                    "Primary" => vm?.Primary,
                    "Secondary" => vm?.Secondary,
                    _ => vm?.Primary,
                });

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
            typeof(BrowserMenu));

    public WebViewState? State
    {
        get => Dispatcher.Invoke(() => (WebViewState?)GetValue(StateProperty));
        set => Dispatcher.Invoke(() => SetValue(StateProperty, value));
    }

    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(
            nameof(Position),
            typeof(string),
            typeof(BrowserMenu),
            new((o, e) =>
            {
                if (o is BrowserMenu menu && e.NewValue is string p)
                {
                    menu.SettingsUri = $"{p}|edge://settings";
                    menu.ProfileUri = $"{p}|edge://profile";
                }
            }));

    public string? Position
    {
        get => Dispatcher.Invoke<string?>(() => (string?)GetValue(PositionProperty));
        set => Dispatcher.Invoke(() => SetValue(PositionProperty, value));
    }

    public static readonly DependencyProperty CommandsProperty =
        DependencyProperty.Register(
            nameof(Commands),
            typeof(Commands),
            typeof(BrowserMenu));

    public Commands Commands
    {
        get => Dispatcher.Invoke(() => (Commands?)GetValue(CommandsProperty) ?? new());
        set => Dispatcher.Invoke(() => SetValue(CommandsProperty, value));
    }

    public static readonly DependencyProperty SettingsUriProperty =
        DependencyProperty.Register(
            nameof(SettingsUri),
            typeof(string),
            typeof(BrowserMenu));

    public string? SettingsUri
    {
        get => Dispatcher.Invoke(() => (string?)GetValue(SettingsUriProperty));
        set => Dispatcher.Invoke(() => SetValue(SettingsUriProperty, value));
    }

    public static readonly DependencyProperty ProfileUriProperty =
        DependencyProperty.Register(
            nameof(ProfileUri),
            typeof(string),
            typeof(BrowserMenu));

    public string? ProfileUri
    {
        get => Dispatcher.Invoke(() => (string?)GetValue(ProfileUriProperty));
        set => Dispatcher.Invoke(() => SetValue(ProfileUriProperty, value));
    }

    public BrowserMenu()
    {
        InitializeComponent();

        Loaded += BrowserMenu_Loaded;
    }

    private void BrowserMenu_Loaded(object sender, RoutedEventArgs e)
    {
    }
}

internal class DictionaryEntryEquatable : IEqualityComparer<DictionaryEntry>
{
    public bool Equals(DictionaryEntry x, DictionaryEntry y)
    {
        return x.Key.Equals(y.Key);
    }

    public int GetHashCode(DictionaryEntry obj)
    {
        return obj.Key.GetHashCode();
    }
}