using Foundation;
using Microsoft.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using VisiPlacement;

namespace ActRec.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp()
        {
            string version = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
            App.AppParams = new AppParams(version, null);

            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();
            builder.UseSkiaSharp();
            builder.ConfigureEffects(effects =>
            {
                effects.Add<RoutingEffect, ActRec.iOS.ButtonEffect>();
            });

            EffectFactory.Instance.RegisterEffect("ActRec.ButtonEffect", new ConstructorProvider<ButtonEffect, Effect>());
            return builder.Build();
        }
    }
}
