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
        public MainPage()
        {
            InitializeComponent();

            //Label label = new Label();
            //label.Text = "Hi Jeff from Xamarin cs!";
            //this.Content = label;

            /*GridLayout gridLayout = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(2), LayoutScore.Zero);
            LayoutChoice_Set boxLayout = new TextboxLayout(new Editor());
            gridLayout.AddLayout(boxLayout);
            for (int i = 0; i < 3; i++)
            {
                //LayoutChoice_Set layout = new TextblockLayout("VisiPlacement.ViewManager running in Xamarin!\nVisiPlacement.ViewManager running in Xamarin!\nVisiPlacement.ViewManager running in Xamarin!\nVisiPlacement.ViewManager running in Xamarin!");
                LayoutChoice_Set layout = new TextblockLayout("Grid element here");
                gridLayout.AddLayout(layout);

            }*/

            //ContentView subview = new ContentView();
            //SingleItem_Layout sublayout = new SingleItem_Layout(subview, layout, new Thickness(), LayoutScore.Zero);
            //LayoutChoice_Set layout = new TextblockLayout("VisiPlacement ViewManager running!");

            ContentView view = new ContentView();
            this.Content = view;

            this.activityRecommender = new ActivityRecommender(view);

            //Label label = new Label();
            //label.Text = "VisiPlacement ViewManager notrunning";
            //label.WidthRequest = 100;
            //label.HeightRequest = 100;
            //view.Content = label;

            //ManageableView managedView = new ManageableView(null);
            //managedView.Content = label;
            //this.Content = managedView;



            //ViewManager viewManager = new ViewManager(view, gridLayout);
        }

        protected override bool OnBackButtonPressed()
        {
            return this.activityRecommender.GoBack();            
        }

        private ActivityRecommender activityRecommender;
    }
}
