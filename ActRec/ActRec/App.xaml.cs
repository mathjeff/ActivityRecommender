using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

[assembly: ExportFont("SatellaRegular.ttf", Alias = "Satella")]
[assembly: ExportFont("aArushShiny.otf", Alias = "ArushShiny")]
[assembly: ExportFont("HandDrawnShapes.otf", Alias = "Hand Drawn Shapes")]
[assembly: ExportFont("PruistineScript.ttf", Alias = "Pruistine Script")]
[assembly: ExportFont("TitanOne.ttf", Alias = "TitanOne")]
[assembly: ExportFont("BlackChancery.ttf", Alias = "BlackChancery")]
[assembly: ExportFont("MinimalFont5x7.ttf", Alias = "MinimalFont5x7")]
[assembly: ExportFont("Qdbettercomicsans.ttf", Alias = "QDBetterComicSans")]
namespace ActRec
{
    public partial class App : Application
    {
        public App(AppParams appParams)
        {
            InitializeComponent();

            MainPage = new MainPage(appParams);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }

    public class AppParams
    {
        public AppParams(string version, ValueProvider<StreamReader> logReader)
        {
            this.Version = version;
            this.LogReader = logReader;
        }
        public string Version;
        public ValueProvider<StreamReader> LogReader;
    }
}
