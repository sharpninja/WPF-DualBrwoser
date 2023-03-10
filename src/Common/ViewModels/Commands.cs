namespace DualBrowser.ViewModels;

public sealed partial class Commands
{
    private const string PRIMARY = "Primary";
    private const string SECONDARY = "Secondary";
    private MainViewModel? _viewModel;

    public Commands(MainViewModel? viewModel = null)
    {
        if (viewModel is not null)
        {
            _viewModel = viewModel;
        }
    }

    public bool CanGoBack(string? side)
    {
        if (_viewModel is null) return false;

        return side switch
        {
            PRIMARY => _viewModel.Primary.GetCanGoBack(),
            SECONDARY => _viewModel.Secondary.GetCanGoBack(),
            _ => false
        };
    }

    private Task Back(string? side)
    {
        if (_viewModel is null)
        {
            return Task.CompletedTask;
        }

        switch (side)
        {
            case PRIMARY: _viewModel.Primary?.GoBack(); break;
            case SECONDARY: _viewModel.Secondary?.GoBack(); break;
        };
        return Task.CompletedTask;
    }

    private IAsyncRelayCommand<string>? _backCommand;

    public IAsyncRelayCommand<string> BackCommand
        => _backCommand ??=
            new AsyncRelayCommand<string>(Back, CanGoBack);

    public bool CanGoForward(string? side)
    {
        if (_viewModel is null) return false;

        return side switch
        {
            PRIMARY => _viewModel.Primary.GetCanGoForward(),
            SECONDARY => _viewModel.Secondary.GetCanGoForward(),
            _ => false
        };
    }

    private Task Forward(string? side, CancellationToken cancellationToken)
    {
        if (_viewModel is null)
        {
            return Task.CompletedTask;
        }

        switch (side)
        {
            case PRIMARY: _viewModel.Primary.GoForward(); break;
            case SECONDARY: _viewModel.Secondary.GoForward(); break;
        };
        return Task.CompletedTask;
    }

    private IAsyncRelayCommand<string>? _forwardCommand;

    public IAsyncRelayCommand<string> ForwardCommand
        => _forwardCommand ??=
            new AsyncRelayCommand<string>(Forward, CanGoForward);


    private Task GoTo(string? path, CancellationToken cancellationToken)
    {
        string? side = path?.Split('|').First();
        string? uri = path?.Split('|').Last();
        switch (side)
        {
            case PRIMARY: _viewModel?.Primary?.Navigate(uri); break;
            case SECONDARY: _viewModel?.Secondary?.Navigate(uri); break;
        };
        return Task.CompletedTask;
    }

    private IAsyncRelayCommand<string>? _goToCommand;

    public IAsyncRelayCommand<string> GoToCommand
        => _goToCommand ??=
            new AsyncRelayCommand<string>(GoTo);


    public bool CanGo(string? side)
    {
        return side switch
        {
            PRIMARY => _viewModel?.Primary?.Uri?.Length > 0,
            SECONDARY => _viewModel?.Secondary?.Uri?.Length > 0,
            _ => false
        };
    }

    private Task Go(string? side, CancellationToken cancellationToken)
    {
        switch (side)
        {
            case PRIMARY: _viewModel?.Primary?.Navigate(_viewModel.Primary?.Uri); break;
            case SECONDARY: _viewModel?.Secondary?.Navigate(_viewModel.Secondary?.Uri); break;
        };
        return Task.CompletedTask;
    }

    private IAsyncRelayCommand<string>? _goCommand;

    public IAsyncRelayCommand<string> GoCommand
        => _goCommand ??=
            new AsyncRelayCommand<string>(Go, CanGo);


    private Task Reload(string? side, CancellationToken cancellationToken)
    {
        switch (side)
        {
            case PRIMARY: _viewModel?.Primary?.Reload(); break;
            case SECONDARY: _viewModel?.Secondary?.Reload(); break;
        };
        return Task.CompletedTask;
    }

    private IAsyncRelayCommand<string>? _reloadCommand;

    public IAsyncRelayCommand<string> ReloadCommand
        => _reloadCommand ??=
            new AsyncRelayCommand<string>(Reload);

    private Task ToggleSideBar(string? side, CancellationToken cancellationToken)
    {
        switch (side)
        {
            case PRIMARY: _viewModel?.Primary?.ToggleSideBar(); break;
            case SECONDARY: _viewModel?.Secondary?.ToggleSideBar(); break;
        };
        return Task.CompletedTask;
    }

    private IAsyncRelayCommand<string>? _toggleSideBarCommand;

    public IAsyncRelayCommand<string> ToggleSideBarCommand
        => _toggleSideBarCommand ??=
            new AsyncRelayCommand<string>(ToggleSideBar);

    private Task SetSplit(string? split, CancellationToken cancellationToken)
    {
        if (_viewModel is null)
        {
            return Task.CompletedTask;
        }

        (_viewModel.SplitLeft, _viewModel.SplitRight) = split switch
        {
            "7" => (new GridLength(7, GridUnitType.Star), new GridLength(3, GridUnitType.Star)),
            "3" => (new GridLength(3, GridUnitType.Star), new GridLength(7, GridUnitType.Star)),
            _ => (new GridLength(5, GridUnitType.Star), new GridLength(5, GridUnitType.Star)),
        };
        return Task.CompletedTask;
    }

    private IAsyncRelayCommand<string>? _setSplitCommand;

    public IAsyncRelayCommand<string> SetSplitCommand
        => _setSplitCommand ??=
            new AsyncRelayCommand<string>(SetSplit);

    private Task GoHome(string? side, CancellationToken cancellationToken)
    {
        switch (side)
        {
            case PRIMARY: _viewModel?.Primary.Navigate(_viewModel.PrimaryHome); break;
            case SECONDARY: _viewModel?.Secondary.Navigate(_viewModel.SecondaryHome); break;
        };

        return Task.CompletedTask;
    }

    private IAsyncRelayCommand<string>? _goHomeCommand;

    public IAsyncRelayCommand<string> GoHomeCommand
        => _goHomeCommand ??=
            new AsyncRelayCommand<string>(GoHome);

    private void ToggleMode()
    {
        _viewModel?.SetMode(_viewModel.Mode switch
        {
            Modes.Single => Modes.Dual,
            Modes.Dual => Modes.Synced,
            _ => Modes.Single
        });
    }

    private IRelayCommand? _toggleModeCommand;

    public IRelayCommand ToggleModeCommand
        => _toggleModeCommand ??=
            new RelayCommand(ToggleMode);

    private void CloseBrowser()
    {
        _viewModel?.InvokeCloseBrowser();
    }

    private IRelayCommand? _closeBrowserCommand;

    public IRelayCommand CloseBrowserCommand
        => _closeBrowserCommand ??=
            new RelayCommand(CloseBrowser);
}
