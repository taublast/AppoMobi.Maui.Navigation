namespace AppoMobi.Maui.Navigation;

public interface ILazyAware : IDisposable
{
    void UpdateControls(DeviceRotation orientation);

    void OnViewAppearing();

    void OnViewDisappearing();

    void KeyboardResized(double size);

}