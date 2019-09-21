using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

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
        public AppParams(string version, bool addSoftwareBackButton, ValueProvider<StreamReader> logReader)
        {
            this.Version = version;
            this.AddSoftwareBackButton = addSoftwareBackButton;
            this.LogReader = logReader;
        }
        public string Version;
        public bool AddSoftwareBackButton;
        public ValueProvider<StreamReader> LogReader;
    }
}
