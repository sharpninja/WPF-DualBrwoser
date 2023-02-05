using Application = System.Windows.Application;

namespace SfDualBrowserWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string KEY = "MTAyNTMzN0AzMjMwMmUzNDJlMzBNaUZKdUdzUjFOd25rZC9zN2p3Z3hDTzlLOEZJeEVBUWNWT3dzci9JQ1cwPQ==";
    static App()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(KEY);
    }
}
