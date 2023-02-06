// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DualBrowser.Models;
using DualBrowser.ViewModels;
using DualBrowser;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Diagnostics;

using Microsoft.UI.Xaml.Controls;

namespace StudyBrowser.Views;

public sealed partial class SideBar : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MainViewModel),
            typeof(SideBar),
            new(null, (o, e) =>
            {
                if (o is not SideBar a || e.NewValue is not MainViewModel vm)
                {
                    return;
                }
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
            typeof(SideBar),
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
            typeof(SideBar),
            new(null));

    public string? Position
    {
        get => (string?)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public Commands? Commands => ViewModel?.Commands;

    public SideBar()
    {
        //InitializeComponent();

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
        else
        {
            Debug.WriteLine($"State not yet initialized.");
        }

        return null;
    }

    public static readonly DependencyProperty HistoryProperty =
        DependencyProperty.Register(
            nameof(History),
            typeof(ObservableCollection<HistoryEntry>),
            typeof(SideBar),
            new(null));

    public ObservableCollection<HistoryEntry>? History
    {
        get => (ObservableCollection<HistoryEntry>?)GetValue(HistoryProperty) ?? SetHistory();
        set => SetValue(HistoryProperty, value);
    }

    public static readonly DependencyProperty SelectedHistoryProperty =
        DependencyProperty.Register(nameof(SelectedHistory), typeof(HistoryEntry), typeof(SideBar),
        new PropertyMetadata(null, (o, e) => {
            if (o is SideBar sb && e.NewValue is HistoryEntry he)
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
