// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DualBrowser.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StudyBrowser.Views;

public sealed partial class BrowserMenu : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MainViewModel),
            typeof(BrowserMenu),
            new(null, (o, e) =>
            {
                if (o is not BrowserMenu a || e.NewValue is not MainViewModel vm) return;

                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == a.Position)
                    {
                        o.SetValue(StateProperty, a.Position switch
                        {
                            "Primary" => vm?.Primary,
                            "Secondary" => vm?.Secondary,
                            _ => vm?.Primary,
                        });
                    }
                };

                o.SetValue(CommandsProperty, vm.Commands);

                o.SetValue(StateProperty, a.Position switch
                {
                    "Primary" => vm?.Primary,
                    "Secondary" => vm?.Secondary,
                    _ => vm?.Primary,
                });

            }));

    public MainViewModel? ViewModel
    {
        get => (MainViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(
            nameof(State),
            typeof(WebViewState),
            typeof(BrowserMenu),
            new(null));

    public WebViewState? State
    {
        get => (WebViewState?)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(
            nameof(Position),
            typeof(string),
            typeof(BrowserMenu),
            new(null, (o, e) =>
            {
                if (o is BrowserMenu menu && e.NewValue is string p)
                {
                    menu.SettingsUri = $"{p}|edge://settings";
                    menu.ProfileUri = $"{p}|edge://profile";
                }
            }));

    public string? Position
    {
        get => (string?)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly DependencyProperty CommandsProperty =
        DependencyProperty.Register(
            nameof(Commands),
            typeof(Commands),
            typeof(BrowserMenu),
            new(null));

    public Commands Commands
    {
        get => (Commands?)GetValue(CommandsProperty) ?? new();
        set => SetValue(CommandsProperty, value);
    }

    public static readonly DependencyProperty SettingsUriProperty =
        DependencyProperty.Register(
            nameof(SettingsUri),
            typeof(string),
            typeof(BrowserMenu),
            new(null));

    public string? SettingsUri
    {
        get => (string?)GetValue(SettingsUriProperty);
        set => SetValue(SettingsUriProperty, value);
    }

    public static readonly DependencyProperty ProfileUriProperty =
        DependencyProperty.Register(
            nameof(ProfileUri),
            typeof(string),
            typeof(BrowserMenu),
            new(null));

    public string? ProfileUri
    {
        get => (string?)GetValue(ProfileUriProperty);
        set => SetValue(ProfileUriProperty, value);
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
