using AppoMobi.Specials.Extensions;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AppoMobi.Maui.Navigation;

/// <summary>
/// Simulating Xamarin/Maui Shell.
/// Uses usual Shell DI for routing.
/// </summary>
public partial class FastShell : AMFlyoutPage, IAppShell, INavigation
{
    public FastShell(IServiceProvider services)
    {
        Debug.WriteLine($"[STARTUP] FAST Shell Created.");

        //_navigation = base.Navigation;

        _services = services;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler != null)
        {
#if ANDROID
            //we replace some layout visual parts with our own, this is totally optional
            InitializeNative(Handler);
#endif
        }
    }

    public bool FlyoutEnabled
    {
        get
        {
            return this.IsGestureEnabled;
        }
        set
        {
            this.IsGestureEnabled = value;
        }
    }

    public void Start(string route)
    {
        var startupRoute = ParseRoute(route);

        //_navigation = new InternalNavigation(this, _flyoutPage.Navigation);

        SetRoot(startupRoute.Parts[0]);

        OrderedRoute = route;

        this.Navigated?.Invoke(this, new ShellNavigatedEventArgs(
            previous: "",
            OrderedRoute,
            ShellNavigationSource.Insert));

        OnStarted();

        if (startupRoute.Parts.Count > 0)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await GoToAsync(route, false);
            });
        }
    }

    protected virtual void OnStarted()
    {

    }

    private INavigation _navigationRoot;
    protected INavigation NavigationRoot
    {
        get
        {
            return _navigationRoot;//base.Navigation;
        }
        set
        {
            _navigationRoot = value;
        }
    }

    public static void RegisterActionRoute(string route, Action switchToTab)
    {
        if (string.IsNullOrEmpty(route))
            return;

        tab_routes[route] = switchToTab;
    }

    public static bool ExecuteActionRoute(string route)
    {
        if (tab_routes.TryGetValue(route, out var action))
        {
            action?.Invoke();
            return true;
        }
        return false;
    }

    static Dictionary<string, Action> tab_routes = new Dictionary<string, Action>();

    #region ROUTER

    static Dictionary<string, TypeRouteFactory> s_routes = new();

    public static void RegisterRoute(string route, Type type)
    {
        RegisterRoute(route, new TypeRouteFactory(type));
    }

    public BindableObject GetOrCreateContent(string route)
    {
        BindableObject result = null;

        if (s_routes.TryGetValue(route, out var content))
        {
            //var createContent = content.GetOrCreate(_services);

            var createContent = content.GetOrCreateObject(_services);

            result = createContent;
        }

        if (result == null)
        {
            // okay maybe its a type, we'll try that just to be nice to the user
            var type = Type.GetType(route);
            if (type != null)
                result = Activator.CreateInstance(type) as Element;
        }

        if (result != null)
            SetRoute(result, route);

        return result;
    }

    public static void SetRoute(BindableObject obj, string value)
    {
        obj.SetValue(RouteProperty, value);
    }

    public static readonly BindableProperty RouteProperty =
        BindableProperty.CreateAttached("Route", typeof(string), typeof(Routing), null,
            defaultValueCreator: CreateDefaultRoute);

    static object CreateDefaultRoute(BindableObject bindable)
    {
        return $"{DefaultPrefix}{bindable.GetType().Name}{++s_routeCount}";
    }

    static int s_routeCount = 0;

    public class TypeRouteFactory : RouteFactory
    {
        readonly Type _type;

        public TypeRouteFactory(Type type)
        {
            _type = type;
        }

        public override Element GetOrCreate()
        {
            return (Element)Activator.CreateInstance(_type);
        }

        public BindableObject GetOrCreateObject(IServiceProvider services)
        {
            if (services != null)
            {
                var o = services.GetService(_type);
                if (o == null)
                {
                    o = Activator.CreateInstance(_type);
                }

                return (BindableObject)o;
            }
            return (BindableObject)Activator.CreateInstance(_type);
        }

        public override Element GetOrCreate(IServiceProvider services)
        {
            if (services != null)
            {
                var o = services.GetService(_type);
                if (o == null)
                {
                    o = Activator.CreateInstance(_type);
                }

                return (Element)o;
            }
            return (Element)Activator.CreateInstance(_type);
        }

        public override bool Equals(object obj)
        {
            if ((obj is TypeRouteFactory typeRouteFactory))
                return typeRouteFactory._type == _type;

            return false;
        }

        public override int GetHashCode()
        {
            return _type.GetHashCode();
        }
    }
    static void ValidateRoute(string route, RouteFactory routeFactory)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentNullException(nameof(route), "Route cannot be an empty string");

        routeFactory = routeFactory ?? throw new ArgumentNullException(nameof(routeFactory), "Route Factory cannot be null");

        var uri = new Uri(route, UriKind.RelativeOrAbsolute);

        var parts = uri.OriginalString.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (IsImplicit(part))
                throw new ArgumentException($"Route contains invalid characters in \"{part}\"");
        }

        TypeRouteFactory existingRegistration = null;

        if (s_routes.TryGetValue(route, out existingRegistration) && !existingRegistration.Equals(routeFactory))
            throw new ArgumentException($"Duplicated Route: \"{route}\"");
    }
    internal static bool IsImplicit(string source)
    {
        return source.StartsWith(ImplicitPrefix, StringComparison.Ordinal);
    }
    public static string FormatRoute(List<string> segments)
    {
        var route = FormatRoute(String.Join(PathSeparator, segments));
        return route;
    }

    public static string FormatRoute(string route)
    {
        return route;
    }

    public static void RegisterRoute(string route, TypeRouteFactory factory)
    {
        if (!String.IsNullOrWhiteSpace(route))
            route = FormatRoute(route);
        ValidateRoute(route, factory);

        s_routes[route] = factory;
    }

    const string ImplicitPrefix = "IMPL_";
    const string DefaultPrefix = "D_FAULT_";
    internal const string PathSeparator = "/";


    #endregion


    #region INavigation

    public void InsertPageBefore(Page page, Page before)
    {
        NavigationRoot.InsertPageBefore(page, before);
    }

    public async Task<Page> PopAsync()
    {
        return await this.PopAsync(true);
    }

    public async Task<Page> PopAsync(bool animated)
    {
        var inStack = ShellNavigationStack.LastOrDefault();
        if (inStack != null)
        {
            if (RootPage is ISupportsLazyViews switcher && inStack.Page is ILazyPage)
            {
                switcher.ViewSwitcher.PopTab();
            }
            else
            {
                await NavigationRoot.PopAsync(animated);
            }
            ShellNavigationStack.RemoveLast();

            return inStack.Page as Page;
        }

        return null;
    }

    public async Task<Page> PopModalAsync()
    {
        return await this.PopModalAsync(true);
    }

    public async Task<Page> PopModalAsync(bool animated)
    {
        //return await NavigationRoot.PopModalAsync(animated);

        var inStack = ShellModalNavigationStack.LastOrDefault();
        if (inStack != null)
        {
            if (RootPage is ISupportsLazyViews switcher && inStack.Page is ILazyPage)
            {
                switcher.ViewSwitcher.PopModal(animated);
            }
            else
            {
                await NavigationRoot.PopAsync(animated);
            }
            ShellModalNavigationStack.RemoveLast();

            return inStack.Page as Page;
        }

        return null;
    }

    public async Task PopToRootAsync()
    {
        await NavigationRoot.PopToRootAsync();
    }

    public async Task PopToRootAsync(bool animated)
    {
        await NavigationRoot.PopToRootAsync(animated);
    }

    public async Task PushAsync(Page page)
    {
        await this.PushAsync(page, true);
    }

    public async Task PushAsync(Page page, bool animated)
    {
        if (RootPage is ISupportsLazyViews switcher && page is ILazyPage)
        {
            switcher.ViewSwitcher.PushPage((ContentPage)page, animated);
        }
        else
        {
            await NavigationRoot.PushAsync(page, animated);
        }

        ShellNavigationStack.AddLast(new PageInStack
        {
            Page = page
        });
    }

    public async Task PushModalAsync(Page page)
    {
        await NavigationRoot.PushModalAsync(page);
    }

    public async Task PushModalAsync(Page page, bool animated)
    {
        if (RootPage is ISupportsLazyViews switcher && page is ILazyPage)
        {
            switcher.ViewSwitcher.PushPage((ContentPage)page, animated, -1, true);
        }
        else
        {
            await NavigationRoot.PushModalAsync(page, animated);
        }

        ShellModalNavigationStack.AddLast(new PageInStack
        {
            Page = page
        });
    }

    public void RemovePage(Page page)
    {
        NavigationRoot.RemovePage(page);
    }

    public IReadOnlyList<Page> ModalStack
    {
        get
        {
            return ShellModalNavigationStack.Select(s => s.Page as Page).ToList();
            //            return _navigation.ModalStack;
        }
    }

    public IReadOnlyList<Page> NavigationStack
    {
        get
        {
            return ShellNavigationStack.Select(s => s.Page as Page).ToList();
            //return _navigation.NavigationStack;
        }
    }

    public class PageInStack
    {
        public string Route { get; set; }
        public IDictionary<string, object> Arguments { get; set; }
        public BindableObject Page { get; set; }
    }

    protected LinkedList<PageInStack> ShellNavigationStack { get; } = new LinkedList<PageInStack>();
    protected LinkedList<PageInStack> ShellModalNavigationStack { get; } = new LinkedList<PageInStack>();

    #endregion



    #region IAppShell

    public string OrderedRoute { get; protected set; }


    public virtual async Task GoToAsync(ShellNavigationState state)
    {

        await GoToAsync(state, false);

    }

    protected string _rootRoute;
    private readonly IServiceProvider _services;

    public static string BuildRoute(string host, IDictionary<string, object> arguments = null)
    {
        var ret = host;
        if (arguments != null)
        {
            ret += "?";
            foreach (var key in arguments)
            {
                ret += $"{key.Key}={key.Value}&";
            }
        }
        return ret;
    }

    public virtual void SetRoot(string host, IDictionary<string, object> arguments = null)
    {
        var currentRoute = BuildRoute(host, arguments);
        if (currentRoute == _rootRoute)
            return;

        var page = GetOrCreateContent(host) as Page;
        if (page != null)
        {
            SetArguments(page, arguments);
            var navi = SetDetail(page);
            NavigationRoot = navi.Navigation;

            _rootRoute = currentRoute;
        }
        else
        {
            throw new Exception($"FastShell failed to create page for '{currentRoute}'!");
        }
    }

    public virtual async Task PushRegisteredPageAsync(string registered, bool animate, IDictionary<string, object> arguments = null)
    {
        var page = GetOrCreateContent(registered) as Page;

        if (page != null)
        {
            SetArguments(page, arguments);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await this.PushAsync(page, animate);
                ShellNavigationStack.Last.Value.Route = registered;
                ShellNavigationStack.Last.Value.Arguments = arguments;
            });

        }
    }

    protected virtual void SetArguments(BindableObject page, IDictionary<string, object> arguments)
    {
        if (page != null && arguments != null)
        {
            if (page.BindingContext is IQueryAttributable needQuery)
            {
                needQuery.ApplyQueryAttributes(arguments);
            }
            else
            if (page.BindingContext != null)
            {
                var type = page.BindingContext.GetType();
                var t = type.GetAttribute<QueryPropertyAttribute>();
                if (t is QueryPropertyAttribute attribute)
                {
                    Reflection.TrySetPropertyValue(page.BindingContext, attribute.Name,
                        arguments[attribute.QueryId]);
                }
            }
        }
    }

    public class ParsedRoute
    {
        public List<string> Parts { get; set; }
        public IDictionary<string, object> Arguments { get; set; }
    }

    public static ParsedRoute ParseState(ShellNavigationState state)
    {
        if (!state.Location.IsAbsoluteUri)
        {
            var route = state.Location.OriginalString.Trim();
            return ParseRoute(route);
        }
        return null;
    }

    public static ParsedRoute ParseRoute(string route)
    {

        var fix = new Uri("fix://" + route.Trim('/'));

        var arguments = System.Web.HttpUtility.ParseQueryString(fix.Query);
        var dict = new Dictionary<string, object>();
        foreach (string key in arguments.AllKeys)
        {
            dict.Add(key, arguments[key]);
        }

        List<string> parts = new List<string>
            {
                fix.Host
            };
        foreach (var segment in fix.Segments)
        {
            var part = segment.Replace("/", "");
            if (!string.IsNullOrEmpty(part))
            {
                parts.Add(part);
            }
        }

        return new ParsedRoute
        {
            Parts = parts,
            Arguments = dict
        };

    }

    public virtual T GetOrCreateContentSetArguments<T>(ShellNavigationState state) where T : BindableObject
    {
        return GetOrCreateContentSetArguments<T>(state, null);
    }

    public virtual T GetOrCreateContentSetArguments<T>(ShellNavigationState state, IDictionary<string, object> arguments) where T : BindableObject
    {
        var route = state.Location.OriginalString.Trim();

        if (!state.Location.IsAbsoluteUri)
        {
            var parsed = ParseRoute(route);
            if (parsed != null)
            {
                IDictionary<string, object> passArguments = null;
                int index = 1;
                foreach (var part in parsed.Parts)
                {
                    if (index == parsed.Parts.Count && parsed.Arguments.Count > 0)
                    {
                        passArguments = parsed.Arguments;
                    }

                    if (index == 1 && route.Left(2) == "//")
                    {
                        var content = GetOrCreateContent(part) as T; //that was ROOT
                        if (content != null)
                        {
                            if (arguments != null)
                            {
                                SetArguments(content, arguments);
                            }
                            SetArguments(content, passArguments);
                        }
                        return content;
                    }
                    else
                    {
                        if (part == "..")
                        {
                            return null; //that was POP
                        }

                        if (tab_routes.TryGetValue(part, out var action))
                        {
                            return null; //that was ACTION
                        }

                        var content = GetOrCreateContent(part) as T;
                        if (content != null)
                        {
                            if (arguments != null)
                            {
                                SetArguments(content, arguments);
                            }
                            SetArguments(content, passArguments);
                        }

                        return content;
                    }
                    index++;
                }
            }
        }

        Console.WriteLine($"[FastShell] Unsupported URI {route}");

        return null;
    }

    public virtual async Task GoToAsync(ShellNavigationState state, bool animate)
    {
        var route = state.Location.OriginalString.Trim();

        if (!state.Location.IsAbsoluteUri)
        {
            var parsed = ParseRoute(route);
            if (parsed != null)
            {
                IDictionary<string, object> passArguments = null;
                int index = 1;
                foreach (var part in parsed.Parts)
                {
                    if (index == parsed.Parts.Count && parsed.Arguments.Count > 0)
                    {
                        passArguments = parsed.Arguments;
                    }

                    if (index == 1 && route.Left(2) == "//")
                    {
                        SetRoot(part, passArguments);
                    }
                    else
                    {
                        if (part == "..")
                        {
                            await PopAsync(animate);
                        }
                        else
                        if (!ExecuteActionRoute(part))
                        {
                            await PushRegisteredPageAsync(part, animate, passArguments);
                        }
                        else
                        {
                            //update UI
                            await Task.Delay(50);
                        }
                    }

                    index++;
                }

                var backup = OrderedRoute;

                OrderedRoute = route;

                this.Navigated?.Invoke(this, new ShellNavigatedEventArgs(
                    previous: backup,
                    OrderedRoute,
                    ShellNavigationSource.Push));

                return;
            }

        }

        Console.WriteLine($"[FastShell] Unsupported URI {route}");
    }

    public new INavigation Navigation => this;

    public event EventHandler<ShellNavigatedEventArgs> Navigated;

    public event EventHandler<ShellNavigatingEventArgs> Navigating;

    public bool FlyoutIsPresented
    {
        get
        {
            return IsPresented;
        }
        set
        {
            IsPresented = value;
        }
    }


    public void InvalidateNavBar()
    {
        OnNavBarInvalidated();

    }

    public virtual void OnNavBarInvalidated()
    {

    }

    public event EventHandler<RotationEventArgs> OnRotation;

    public event EventHandler<IndexArgs> TabReselected;
    public ObservableCollection<MenuPageItem> MenuItems { get; } = new ObservableCollection<MenuPageItem>();
    public async Task PopTabToRoot()
    {
        var inStack = ShellNavigationStack.LastOrDefault();
        if (inStack != null)
        {
            if (RootPage is ISupportsLazyViews switcher)
            {
                switcher.ViewSwitcher.PopTabToRoot();
            }
        }
    }

    #endregion


    #region BUFFER

    /// <summary>
    /// Can use to pass items as models between viewmodels
    /// </summary>
    public static ConcurrentDictionary<string, object> Buffer { get; } = new();

    #endregion

}