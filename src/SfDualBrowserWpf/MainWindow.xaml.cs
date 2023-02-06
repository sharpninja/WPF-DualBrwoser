using System.Diagnostics;

using DualBrowser.ViewModels;

using SizeMode = Syncfusion.SfSkinManager.SizeMode;

namespace SfDualBrowserWpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    #region Fields
    private string currentVisualStyle;
    private string currentSizeMode;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the current visual style.
    /// </summary>
    /// <value></value>
    /// <remarks></remarks>
    public string CurrentVisualStyle
    {
        get
        {
            return currentVisualStyle;
        }
        set
        {
            currentVisualStyle = value;
            OnVisualStyleChanged();
        }
    }

    /// <summary>
    /// Gets or sets the current Size mode.
    /// </summary>
    /// <value></value>
    /// <remarks></remarks>
    public string CurrentSizeMode
    {
        get
        {
            return currentSizeMode;
        }
        set
        {
            currentSizeMode = value;
            OnSizeModeChanged();
        }
    }

    public MainViewModel ViewModel { get; }
    #endregion

    public MainWindow()
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

        Closed += MainWindow_Closed;
        ViewModel.CloseBrowserRequested += (_, _) => Close();

        CurrentVisualStyle = "FluentDark";
        CurrentSizeMode = "Touch";
    }

    public new void Close()
    {
        base.Close();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        _controller.SaveState();
    }

    /// <summary>
    /// On Visual Style Changed.
    /// </summary>
    /// <remarks></remarks>
    private void OnVisualStyleChanged()
    {
        if (Enum.TryParse(CurrentVisualStyle, out VisualStyles visualStyle)
         && visualStyle != VisualStyles.Default)
        {
            SfSkinManager.ApplyStylesOnApplication = true;
            SfSkinManager.SetVisualStyle(this, visualStyle);
            SfSkinManager.ApplyStylesOnApplication = false;
        }
    }

    /// <summary>
    /// On Size Mode Changed event.
    /// </summary>
    /// <remarks></remarks>
    private void OnSizeModeChanged()
    {
        if(Enum.TryParse(CurrentSizeMode, out SizeMode sizeMode)
         && sizeMode != SizeMode.Default)
        {
            SfSkinManager.ApplyStylesOnApplication = true;
            SfSkinManager.SetSizeMode(this, sizeMode);
            SfSkinManager.ApplyStylesOnApplication = false;
        }
    }

    private void RightToolBar_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        Debug.WriteLine(e.NewValue);
    }
}
