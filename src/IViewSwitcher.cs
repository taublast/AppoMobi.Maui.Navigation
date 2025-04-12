namespace AppoMobi.Maui.Navigation;

public interface IViewSwitcher
{
    Task PopTab(int tab = -1);

    void PopModal(bool animated);

    Task PopTabToRoot();

    void PushPage(ContentPage page, bool animate, int tab = -1, bool isModal = false);

    int SelectedIndex { get; set; }

    void OnAppearing();

    void OnDisappearing();

}