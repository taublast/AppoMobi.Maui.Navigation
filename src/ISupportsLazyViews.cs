namespace AppoMobi.Maui.Navigation;

public interface ISupportsLazyViews
{
	public IViewsContainer ViewsContainer { get; }

	public IViewSwitcher ViewSwitcher { get; }

}