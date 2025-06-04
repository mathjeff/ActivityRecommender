using System;
using System.Collections.Generic;
using System.Linq;
using VisiPlacement;
using Microsoft.Maui.Controls;
using ActivityRecommendation;
using System.IO;
using Windows.UI.Xaml;
using Windows.ApplicationModel;

namespace ActRec.UWP
{
    public partial class UWPMainPage
    {
        public UWPMainPage()
        {
            InitializeComponent();
            PackageVersion version = Package.Current.Id.Version;
            string versionText = "" + version.Major + "." + version.Minor + "." + version.Revision + "." + version.Build;
            AppParams appParams = new AppParams(versionText, new ConstantValueProvider<StreamReader>(null));
            VisiPlacement.UWP.UWPTextMeasurer.Initialize();
            VisiPlacement.UWP.UWPButtonClicker.Initialize();
            LoadApplication(new ActRec.App(appParams));
        }
    }
}
