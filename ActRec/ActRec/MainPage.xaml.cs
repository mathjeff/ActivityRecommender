using System;
using System.Collections.Generic;
using System.Linq;
using VisiPlacement;
using Microsoft.Maui.Controls;
using ActivityRecommendation;

namespace ActRec
{
    public partial class MainPage : ContentPage
    {
        public MainPage(AppParams appParams)
        {
            InitializeComponent();

            ContentView view = new ContentView();
            Label label = new Label();
            label.Text = "ActivityRecommender is starting!";
            view.Content = label;
            this.Content = view;
            Console.WriteLine("MainPage.xaml.cs MainPage()");

            this.activityRecommender = new ActivityRecommender(view, appParams.Version, appParams.LogReader);
        }

        protected override bool OnBackButtonPressed()
        {
            return this.activityRecommender.GoBack();            
        }

        private ActivityRecommender activityRecommender;
    }
}
