//extern alias webview2;

using System.Diagnostics;
using System.IO;

using DualBrowser.Models;

using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

using Newtonsoft.Json;

using Border = Microsoft.UI.Xaml.Controls.Border;
using CoreWebView2NewWindowRequestedEventArgs = Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs;
//using CoreWebView2NavigationCompletedEventArgs =
//    webview2::Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs;

namespace DualBrowser;

[ObservableObject]
public sealed partial class DualBrowserController : SimpleControllerBase
{
    private const string DIRECTORY_NAME = ".dualBrowser";
    private const string STATE_FILE_NAME = "state.json";

    ManualResetEventSlim _primaryMre = new(false);
    ManualResetEventSlim _secondaryMre = new(false);

    private readonly MainViewModel _viewModel;

    public DualBrowserController()
    {
        _viewModel = new MainViewModel(this);
    }

    public MainViewModel ViewModel => _viewModel;

    public WebView2 PrimaryWebView { get; internal set; }
    public WebView2 SecondaryWebView { get; internal set; }

    public bool Initialize(Border primaryBorder, WebView2 primary, Border secondaryBorder, WebView2 secondary)
    {
        _viewModel.Initialize(primaryBorder, primary, secondaryBorder, secondary);

        PrimaryWebView = primary;
        SecondaryWebView = secondary;

        PrimaryWebView.NavigationStarting += PrimaryWebView_NavigationStarting;
        SecondaryWebView.NavigationStarting += SecondaryWebView_NavigationStarting;
        PrimaryWebView.NavigationCompleted += PrimaryWebView_NavigationCompleted;
        SecondaryWebView.NavigationCompleted += SecondaryWebView_NavigationCompleted;

        PrimaryWebView.CoreWebView2Initialized += PrimaryWebView_CoreWebView2InitializationCompleted;
        SecondaryWebView.CoreWebView2Initialized += SecondaryWebView_CoreWebView2InitializationCompleted;

        PrimaryWebView.EnsureCoreWebView2Async()
            .AsTask().GetAwaiter().GetResult();
        SecondaryWebView.EnsureCoreWebView2Async()
            .AsTask().GetAwaiter().GetResult();


        Task.Run(async () =>
        {
            _primaryMre.Wait();
            _secondaryMre.Wait();

            await LoadStateAsync();
        });

        return Initialize();
    }

    public void SaveState()
    {
        string json = ViewModel.GetState();

        if (json is { Length: > 0 })
        {
            FileStream? writeStream = null;
            StreamWriter? writer = null;
            try
            {
                DirectoryInfo? dualBrowserDir = null;
                DirectoryInfo profile = new(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

                if (!profile.GetDirectories().Select(d => d.Name).Contains(DIRECTORY_NAME))
                {
                    dualBrowserDir = profile.CreateSubdirectory(DIRECTORY_NAME);
                }

                dualBrowserDir ??=
                 profile.GetDirectories().FirstOrDefault(d => d.Name == DIRECTORY_NAME);

                _ = dualBrowserDir ??
                    throw new IOException(
                        $"could not create {Path.Combine(profile.FullName, DIRECTORY_NAME)}");

                FileInfo? stateFile = dualBrowserDir.GetFiles(STATE_FILE_NAME).FirstOrDefault();
                stateFile ??= new FileInfo(Path.Combine(dualBrowserDir.FullName, STATE_FILE_NAME));

                writeStream = stateFile.OpenWrite();
                writer = new(writeStream);

                writer.Write(json);
                writer.Flush();
                writer.Close();
            }
            finally
            {
                writer?.Dispose();
                writeStream?.Dispose();
            }
        }
    }


    private void CoreWebView2_NewWindowRequested(
        object? sender,
        CoreWebView2NewWindowRequestedEventArgs e)
    {
        if (sender is not CoreWebView2 core) return;

        //switch (e.)
        //{
        //    case "_parent" or "primary" or "_primary" or "_blank":
        //        _viewModel.Primary.Navigate(e.Uri);
        //        e.Handled = true;
        //        break;
        //    default:
        //        core.Navigate(e.Uri);
        //        e.Handled = true;
        //        break;
        //}
    }

    public override bool Initialize()
    {
        return _viewModel is not null;
    }

    private async Task LoadStateAsync()
    {
        DirectoryInfo? dualBrowserDir = null;
        DirectoryInfo profile = new(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        if (!profile.GetDirectories().Select(d => d.Name).Contains(DIRECTORY_NAME))
        {
            dualBrowserDir = profile.CreateSubdirectory(DIRECTORY_NAME);
        }

        dualBrowserDir ??=
         profile.GetDirectories().FirstOrDefault(d => d.Name == DIRECTORY_NAME);

        _ = dualBrowserDir ??
            throw new IOException(
                $"could not create {Path.Combine(profile.FullName, DIRECTORY_NAME)}");

        FileInfo? stateFile = dualBrowserDir.GetFiles(STATE_FILE_NAME).FirstOrDefault();

        if (stateFile is not null)
        {
            StreamReader reader = stateFile.OpenText();
            string? json;
            try
            {
                json = reader.ReadToEnd();
            }
            finally
            {
                reader.Dispose();
            }

            if (json is { Length: > 0 })
            {
                try
                {
                    await ViewModel.Primary.WebView.Dispatcher.TryRunAsync(
                        Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () =>
                    {
                        ViewModel.SetState(json);
                    });
                }
                catch(JsonException je)
                {
                    Debug.WriteLine(je);
                    stateFile.Delete();
                }
            }
        }
    }


    public void SetMode(Modes mode)
    {
        _viewModel.SetMode(mode);
    }

    //private void CoreWebView2_StatusBarTextChanged(object? sender, object e)
    //{
    //    if (sender is CoreWebView2 coreWebView)
    //    {
    //        switch (PrimaryWebView.CoreWebView2.Equals(coreWebView))
    //        {
    //            case true:
    //                _viewModel.Primary.Status = coreWebView.StatusBarText;
    //                break;
    //            case false:
    //                _viewModel.Secondary.Status = coreWebView.StatusBarText;
    //                break;
    //        }
    //    }
    //}

    private void PrimaryWebView_CoreWebView2InitializationCompleted(object? sender,
        CoreWebView2InitializedEventArgs e)
    {
        if (e.Exception is null)
        {

            //PrimaryWebView StatusBarTextChanged += CoreWebView2_StatusBarTextChanged;
            //PrimaryWebView.NewWindowRequested += CoreWebView2_NewWindowRequested;

            _primaryMre.Set();
        }
    }

    private void SecondaryWebView_CoreWebView2InitializationCompleted(object? sender,
        CoreWebView2InitializedEventArgs e)
    {
        if (e.Exception is null)
        {
            _secondaryMre.Set();
            _primaryMre.Wait();

            //SecondaryWebView.CoreWebView2.StatusBarTextChanged += CoreWebView2_StatusBarTextChanged;
            //SecondaryWebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            //SecondaryWebView.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;

            if (_viewModel.Primary.LastNavigatedUri is null or "")
            {
                _viewModel.Commands.GoHomeCommand.Execute("Primary");
            }

            if (_viewModel.Mode is Modes.Dual &&
                _viewModel.Secondary.LastNavigatedUri is null or "")
            {
                _viewModel.Commands.GoHomeCommand.Execute("Secondary");
            }
        }
    }

    //private void CoreWebView2_HistoryChanged(object? sender, object e)
    //{
    //    if (sender is not CoreWebView2) return;
    //}

    private void NotifyButtons()
    {
        _viewModel.Commands.BackCommand.NotifyCanExecuteChanged();
        _viewModel.Commands.ForwardCommand.NotifyCanExecuteChanged();
    }

    private void PrimaryWebView_NavigationStarting(
        object? sender,
        CoreWebView2NavigationStartingEventArgs e)
    {
        _viewModel.Primary.Uri = e.Uri;
        _viewModel.Primary.LastNavigatedUri = e.Uri;

        _viewModel.SyncBrowsers();

        NotifyButtons();
    }

    private void PrimaryWebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        //_viewModel.Primary.StatusCode = e.;
        //_viewModel.Primary.Error = e.WebErrorStatus;

        if (e.IsSuccess)
        {
            _viewModel.Primary.History.Add(
                new HistoryEntry(
                    _viewModel.Primary.Uri ??
                        _viewModel.Primary.LastNavigatedUri ??
                        "",
                    e));
        }

        NotifyButtons();
    }

    private void SecondaryWebView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        _viewModel.Secondary.Uri = e.Uri;

        if (_viewModel.Mode is Modes.Dual)
        {
            _viewModel.Secondary.LastNavigatedUri = e.Uri;
        }

        NotifyButtons();
    }

    private void SecondaryWebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        //_viewModel.Secondary.StatusCode = e.HttpStatusCode;
        //_viewModel.Secondary.Error = e.WebErrorStatus;

        if (e.IsSuccess)
        {
            _viewModel.Secondary.History.Add(
                new HistoryEntry(
                    _viewModel.Secondary.Uri ??
                        _viewModel.Secondary.LastNavigatedUri ??
                        "",
                    e));
        }

        NotifyButtons();
    }
}
