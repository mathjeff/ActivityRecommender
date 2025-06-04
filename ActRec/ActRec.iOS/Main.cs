using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Foundation;
using UIKit;
using VisiPlacement.iOS;

namespace ActRec.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            //Xamarin.Forms.Forms.SetFlags("Brush_Experimental");
            iOSTextMeasurer.Initialize();
            iOSButtonClicker.Initialize();
            System.Diagnostics.Debug.WriteLine("Main debug writeline");
            Console.WriteLine("Main console writeline");
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}
