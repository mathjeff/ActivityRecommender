using System;
using System.Collections.Generic;
using System.Linq;
using VisiPlacement;
using Xamarin.Forms;
using ActivityRecommendation;

namespace ActRec
{
    public partial class MainPage : ContentPage
    {
        public MainPage(AppParams appParams)
        {
            InitializeComponent();

            ContentView view = new ContentView();
            this.Content = view;

            this.activityRecommender = new ActivityRecommender(view, appParams.Version, appParams.LogReader);
        }

        protected override bool OnBackButtonPressed()
        {
            return this.activityRecommender.GoBack();
        }

        private ActivityRecommender activityRecommender;
    }
}
