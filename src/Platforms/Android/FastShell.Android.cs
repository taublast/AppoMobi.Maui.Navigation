using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.Core.Content;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Navigation.Fragment;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using View = Android.Views.View;

namespace AppoMobi.Maui.Navigation
{
    public partial class FastShell
    {
        bool once = false;
        /// <summary>
        /// Set app background to app splash screen, so we can cross-fade etc. Now if you set fastShell opacity to 0 you will see the splash below.
        /// Lyrics: I fail to understand why app background should be #FFFFFF
        /// and why to destroy the splash background after app launched, not letting us animate app content to fade in after splash screen.
        /// All this code to avoid a blink between splash screen and mainpage showing.
        /// </summary>
        /// <param name="handler"></param>
        public void InitializeNative(IViewHandler handler)
        {
            if (!once && handler.PlatformView is DrawerLayout drawerLayout)
            {
                once = true;
                try
                {
                    drawerLayout.Background = null;
                    var rootView = drawerLayout.Context?.GetActivity()?.Window?.DecorView;
                    if (rootView != null)
                    {
                        var PackageName = Platform.CurrentActivity.PackageName;

                        // Get the color from resources
                        int colorResId = Platform.CurrentActivity.Resources.GetIdentifier("maui_splash_color", "color", PackageName);
                        int color = ContextCompat.GetColor(Platform.AppContext, colorResId);

                        // Get the drawable for the splash image
                        int drawableResId = Platform.CurrentActivity.Resources.GetIdentifier("maui_splash", "drawable", PackageName);
                        Drawable splashDrawable = ContextCompat.GetDrawable(Platform.AppContext, drawableResId);

                        Drawable[] layers = new Drawable[2];
                        layers[0] = new ColorDrawable(new Android.Graphics.Color(color)); // use the color you got from resources
                        layers[1] = splashDrawable;

                        LayerDrawable layerDrawable = new LayerDrawable(layers);

                        // Set the gravity for the image to be centered
                        int inset = 0; // You can adjust this value if you want an inset from the center
                        layerDrawable.SetLayerInset(1, inset, inset, inset, inset);
                        layerDrawable.SetLayerGravity(1, GravityFlags.Center);

                        // Set the drawable as the background of the root view
                        rootView.SetBackground(layerDrawable);

                        //var hostId = _Microsoft.Android.Resource.Designer.Resource.Id.nav_host;
                        //var view = FindViewById(drawerLayout,  hostId);

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
        }




        public static View FindViewById(View parent, int id)
        {
            if (parent != null)
            {
                if (parent.Id == id)
                    return parent;
            }

            if (parent is ViewGroup viewGroup)
            {
                for (int i = 0; i < viewGroup.ChildCount; i++)
                {
                    var result = FindViewById(viewGroup.GetChildAt(i), id);
                    if (result != null)
                        return result;
                }
            }
            return default;
        }


        //void AdaptNavHost()
        //{
        //    drawerLayout.Context

        //    (this Handler.PlatformView as Android.Views.View)
        //    var fm = Platform.CurrentActivity.GetFragmentManager();
        //    var navHostFragment = fm.FindFragmentById(_Microsoft.Android.Resource.Designer.Resource.Id.nav_host) as NavHostFragment;

        //    var nav = Platform.CurrentActivity.FindViewById(Resource.Id.nav_host);
        //    //nav.Background = null;
        //    var framework = AppContext.TargetFrameworkName;

        //}

    }
}