namespace AppoMobi.Maui.Navigation;

public interface ILazyPage : ILazyAware
{
    bool IsDisposed { get; }
}