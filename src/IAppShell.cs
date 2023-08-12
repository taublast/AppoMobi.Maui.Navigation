using System.Collections.ObjectModel;

namespace AppoMobi.Maui.Navigation;

/// <summary>
/// All the methods and properties that are required for the AppShell to work.
/// </summary>
public interface IAppShell
{
    public T GetOrCreateContent<T>(ShellNavigationState state) where T : BindableObject;

    //xamarin shell
    public Task GoToAsync(ShellNavigationState state);

    public Task GoToAsync(ShellNavigationState state, bool animate);

    public INavigation Navigation { get; }

    public event EventHandler<ShellNavigatedEventArgs> Navigated;

    public event EventHandler<ShellNavigatingEventArgs> Navigating;

    public bool FlyoutIsPresented { get; set; }

    public bool FlyoutEnabled { get; set; }

    //custom
    public void Start(string route);

    public void InvalidateNavBar();

    public event EventHandler<RotationEventArgs> OnRotation;

    public event EventHandler<IndexArgs> TabReselected;

    public ObservableCollection<MenuPageItem> MenuItems { get; }

    public Task PopTabToRoot();
}