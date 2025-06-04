using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VisiPlacement;

namespace ActRec
{
    public partial class App : Application
    {
        public static AppParams AppParams { get; set; }
        public App()
        {
            InitializeComponent();
            Console.WriteLine("App.xaml.cs constructor");
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            Console.WriteLine("App.xaml.cs CreateWindow");
            return new Window(new MainPage(AppParams));
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
