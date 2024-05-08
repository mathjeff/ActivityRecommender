using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Reflection;
using System.IO;
using Java.Lang;
using VisiPlacement;
using Android;
using ActivityRecommendation;

namespace ActRec.Droid
{
    [Activity(Label = "ActRec", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity :  global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Xamarin.Forms.Forms.SetFlags("Brush_Experimental");

            global::Xamarin.Forms.Forms.Init(this, bundle);

            VisiPlacement.Android.AndroidTextMeasurer.Initialize();
            VisiPlacement.Android.AndroidButtonClicker.Initialize();

            PublicFileIo.setBasedir(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "ActivityRecommender"));

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            AppParams parameters = new AppParams(version, new LogcatReader());

            LoadApplication(new App(parameters));

            Xamarin.Essentials.Platform.Init(this, bundle);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            bool storagePermissionsUnneccessary = Xamarin.Essentials.DeviceInfo.Version.Major >= 13;
            if (storagePermissionsUnneccessary)
            {
                // Tell Xamarin that these permissions are unneeded by saying that they were granted
                for (int i = 0; i < permissions.Length; i++)
                {
                    string permission = permissions[i];
                    if (permission.Equals("android.permission.WRITE_EXTERNAL_STORAGE") || permission.Equals("android.permission.READ_EXTERNAL_STORAGE"))
                    {
                        grantResults[i] = Permission.Granted;
                    }
                }
            }

            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class LogcatReader : ValueProvider<StreamReader>
    {
        public StreamReader Get()
        {
            Runtime runtime = Runtime.GetRuntime();
            System.Diagnostics.Debug.WriteLine("ActRec.Android checking for previous errors");
            string[] parameters = new string[] { "timeout", "4", "logcat", "-d" };
            Java.Lang.Process process = new ProcessBuilder(parameters).RedirectErrorStream(true).Start();
            process.WaitFor();
            System.Diagnostics.Debug.WriteLine("logcat return code = " + process.ExitValue());
            Stream stream = process.InputStream;
            return new StreamReader(stream);
        }
    }
}

