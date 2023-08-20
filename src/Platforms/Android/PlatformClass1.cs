﻿using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.Core.Content;
using AndroidX.DrawerLayout.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Platform;

namespace AppoMobi.Maui.Navigation
{
    // All the code in this file is only included on Android.
    public partial class FastShell
    {
        bool once = false;
        /// <summary>
        /// I fail to understand why app background should be #FFFFFF
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
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
        }
    }
}