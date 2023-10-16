namespace AppoMobi.Maui.Navigation;

public class AMNavigationPage : NavigationPage, IDisposable
{

    //public override bool CanChangeFocus(VisualElement element)
    //{
    //    //var entry = element as AMEntry;
    //    //if (entry != null && entry.UnfocusLocked)
    //    //{
    //    //    return false;
    //    //}

    //    return base.CanChangeFocus(element);
    //}

    private VisualElement _focusedElement;
    public VisualElement FocusedElement
    {
        get { return _focusedElement; }
        set
        {
            if (_focusedElement != value)
            {
                _focusedElement = value;
                OnPropertyChanged();
            }
        }
    }


    public virtual void OnFocusedElementChanged(VisualElement element)
    {

    }

    public virtual bool CanChangeFocus(VisualElement element)
    {

        return true;
    }

    public void Dispose()
    {
        try
        {
            foreach (var page in this.Navigation.NavigationStack)
            {
                if (page is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            this.Handler?.DisconnectHandler();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public AMNavigationPage(Page root) : base(root)
    {
        HandlerChanged += ThisHandlerChanged;

    }

    private void ThisHandlerChanged(object sender, EventArgs e)
    {
        var check = this.Handler;
    }

    public AMNavigationPage()
    {
        HandlerChanged += ThisHandlerChanged;
    }

}