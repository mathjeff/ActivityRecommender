using System;
using System.Collections.Generic;
using System.Linq;
using VisiPlacement;
using Xamarin.Forms;
using ActivityRecommendation;
using System.IO;
using Windows.UI.Xaml;

namespace ActRec.UWP
{
    public partial class UWPMainPage
    {
        public UWPMainPage()
        {
            InitializeComponent();
            AppParams appParams = new AppParams("", new ConstantValueProvider<StreamReader>(null));
            // Uniforms.misc isn't supported on windows, so we disable trying to check for it
            TextFormatter.GetForFontName("").ChooseType(TextFormatterType.INNATE);
            LoadApplication(new ActRec.App(appParams));
        }
    }
}
