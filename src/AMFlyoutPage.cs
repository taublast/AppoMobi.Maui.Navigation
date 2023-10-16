namespace AppoMobi.Maui.Navigation
{
    public class AMFlyoutPage : FlyoutPage
    {
        protected override bool OnBackButtonPressed()
        {
            return base.OnBackButtonPressed();
        }

        public AMFlyoutPage()
        {
            try
            {
                var menu = new ContentPage
                {
                    Title = "...",
                    BindingContext = this
                };

                Flyout = menu;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        //-------------------------------------------------------------
        // IsOpening
        //-------------------------------------------------------------
        private const string nameIsOpening = "IsOpening";
        public static readonly BindableProperty IsOpeningProperty = BindableProperty.Create(nameIsOpening, typeof(bool), typeof(AMFlyoutPage), false);
        public bool IsOpening
        {
            get { return (bool)GetValue(IsOpeningProperty); }
            set { SetValue(IsOpeningProperty, value); }
        }

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();

        //    App.Instance.Messager.Subscribe<string>(this, "Menu", async (sender, arg) =>
        //   {
        //       if (arg == "Disable")
        //       {
        //           IsGestureEnabled = false;
        //       }
        //       else
        //       if (arg == "Enable")
        //       {
        //           IsGestureEnabled = true;
        //       }
        //   });

        //}

        //protected override void OnDisappearing()
        //{
        //    base.OnDisappearing();

        //    App.Instance.Messager.Unsubscribe(this, "Menu");
        //}


        //Cache
        Dictionary<Type, AMNavigationPage> MenuPages = new Dictionary<Type, AMNavigationPage>();


        public AMNavigationPage SetDetail(Type pageType, params object[] parameters)
        {
            AMNavigationPage ret = null;

            //add-create page to cache
            if (!MenuPages.ContainsKey(pageType))
            {
                //todo new create page
                var obj = Activator.CreateInstance(pageType, parameters);
                var page = (Page)obj;
                var navi = new AMNavigationPage(page);

                MenuPages.Add(pageType, navi);
            }

            //take existing page from cache
            var newPage = MenuPages[pageType];

            //change page if not already presented
            if (newPage != null && Detail != newPage)
            {
                newPage = OnCreatingNavigationPage(newPage);

                var kill = Detail;

                Detail = newPage;
                ret = newPage;

                if (kill is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            return ret;
        }

        public Page RootPage
        {
            get
            {
                if (Detail is AMNavigationPage navi)
                {
                    return navi.CurrentPage;
                }
                return null;
            }
        }

        public AMNavigationPage SetDetail(Page page)
        {
            AMNavigationPage ret = null;

            if (Detail is AMNavigationPage navi)
            {
                if (navi.CurrentPage == page)
                    return navi; //nothing changed
            }

            var newPage = new AMNavigationPage(page);

            var kill = Detail;

            newPage = OnCreatingNavigationPage(newPage);

            Detail = newPage;
            ret = newPage;

            if (kill is IDisposable disposable)
            {
                disposable.Dispose();
            }

            return ret;
        }

        protected virtual AMNavigationPage OnCreatingNavigationPage(AMNavigationPage newPage)
        {
            //fixes status bar color
            //newPage.BarTextColor = Colors.White; //StatusBar text color for ios
            //newPage.BarBackgroundColor = Colors.White;

            return newPage;
        }
    }
}