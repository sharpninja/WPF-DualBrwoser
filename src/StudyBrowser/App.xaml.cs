// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DualBrowser;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

using StudyBrowser.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StudyBrowser;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application, IDispatcher
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
    }

    private static MainWindow? m_window;
    public static MainWindow? MainWindow => m_window;
    public static MainPage? MainPage => MainWindow?.MainPage;
    public DispatcherQueue? DispatcherQueue => MainPage?.DispatcherQueue;
}
