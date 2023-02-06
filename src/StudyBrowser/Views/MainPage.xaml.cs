// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DualBrowser.ViewModels;
using DualBrowser;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.IO;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StudyBrowser.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{

    #region Properties
    public MainViewModel ViewModel { get; }
    #endregion

    public MainPage()
    {
        _controller = new();
        ViewModel = _controller.ViewModel;
        InitializeComponent();

        _controller.Initialize(PrimaryBorder, Primary, SecondarBorder, Secondary);
        _controller.SetMode(Modes.Dual);

        DataContext = ViewModel;

        this.Loaded += OnLoaded;
    }

    private DualBrowserController _controller;
    public WebView2 Primary => PrimaryWebView;
    public WebView2 Secondary => SecondaryWebView;

    public Commands Commands => ViewModel.Commands;

    /// <summary>
    /// Called when [loaded].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.CloseBrowserRequested += (_, _) => Close();

    }

    public void Close()
    {
        MainWindow_Closed();
        App.Current.Exit();
    }

    private void MainWindow_Closed()
    {
        _controller.SaveState();
    }



    private void RightToolBar_OnIsVisibleChanged(
        object sender,
        DependencyPropertyChangedEventArgs e)
    {
        Debug.WriteLine(e.NewValue);
    }
}
