using System.Collections.ObjectModel;

namespace AppoMobi.Maui.Navigation;

public interface IAppShell
{
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