// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using DualBrowser.ViewModels;

using DualBrowser;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StudyBrowser.Views;

public sealed partial class AddressBar : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MainViewModel),
            typeof(AddressBar),
            new(null, (o, e) =>
            {
                if (o is not AddressBar a || e.NewValue is not MainViewModel vm)
                {
                    return;
                }
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
            typeof(AddressBar),
            new PropertyMetadata(null));

    public WebViewState? State
    {
        get => (WebViewState?)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(
            nameof(Position),
            typeof(string),
            typeof(AddressBar),
            new PropertyMetadata(null));

    public string? Position
    {
        get => (string?)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
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
        if (sender is WebViewState)
        {
        }
    }

    private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is not (VirtualKey.Enter) ||
            Commands is null ||
            State is null)
        {
            return;
        }

        State.Uri = AddressTextBox.Text;
        Commands?.GoCommand.ExecuteAsync(Position);
        e.Handled = true;
    }

    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        AddressTextBox.SelectAll();
    }
}
