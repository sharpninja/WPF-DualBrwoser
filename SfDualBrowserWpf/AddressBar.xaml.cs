using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SfDualBrowserWpf;

/// <summary>
/// Interaction logic for AddressBar.xaml
/// </summary>
public partial class AddressBar
{
    public static DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MainViewModel),
            typeof(AddressBar),
            new((o, e) =>
            {
                if (o is AddressBar a && e.NewValue is MainViewModel vm)
                {
                    vm.PropertyChanged += (_, e) =>
                    {
                        if(e.PropertyName == a.Position)
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
            typeof(AddressBar));

    public WebViewState? State
    {
        get => Dispatcher.Invoke(() => (WebViewState?)GetValue(StateProperty));
        set => Dispatcher.Invoke(() => SetValue(StateProperty, value));
    }

    public static DependencyProperty PositionProperty =
        DependencyProperty.Register(
            nameof(Position),
            typeof(string),
            typeof(AddressBar));

    public string? Position
    {
        get => Dispatcher.Invoke<string?>(()=>(string?)GetValue(PositionProperty));
        set => Dispatcher.Invoke(() => SetValue(PositionProperty, value));
    }

    public Commands? Commands => ViewModel?.Commands;

    public AddressBar()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            if (State is not null)
            {
                State.PropertyChanged += State_PropertyChanged;
            }
        };

    }

    private void State_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is WebViewState state)
        {
        }
    }

    private void SfTextBoxExt_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return &&
            Commands is not null &&
            State is not null)
        {
            State.Uri = AddressTextBox.Text;
            Commands.GoCommand.ExecuteAsync(Position);
            e.Handled = true;
        }
    }

    private void SfTextBoxExt_GotFocus(object sender, RoutedEventArgs e)
    {
        AddressTextBox.SelectAll();
        e.Handled = true;
    }
}
