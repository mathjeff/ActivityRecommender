using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Reflection;

namespace ActRec.Droid
{
    [Activity(Label = "ActRec", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            Uniforms.Misc.Droid.ScreenUtils.Init();
            Uniforms.Misc.Droid.ImageUtils.Init();
            Uniforms.Misc.Droid.TextUtils.Init();

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            LoadApplication(new App(version));
        }
    }
}

