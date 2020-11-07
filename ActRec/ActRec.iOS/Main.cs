using System;
using System.Collections.Generic;
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
            iOSTextMeasurer.Initialize();
            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}
