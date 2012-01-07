using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

// The RatingEntryView provides a place for the user to tell how much value there was in having done a particular activity
namespace ActivityRecommendation
{
    class RatingEntryView : TitledControl
    {
        public RatingEntryView(string title)
            :base(title)
        {
            // setup the main display grid
            this.displayGrid = new DisplayGrid(3, 1);
            //this.displayGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

            this.SetupTypeSelection();
            this.SetupSubviews();
            this.UpdateSubview();



            this.SetContent(this.displayGrid);
        }
        private void SetupTypeSelection()
        {
            ResizableTextBlock ratingTypeBlock = new ResizableTextBlock();
            ratingTypeBlock.Text = "Rating Type:";
            this.displayGrid.AddItem(ratingTypeBlock);

            List<string> titles = new List<string>();
            titles.Add("Absolute");
            titles.Add("Relative");
            this.typeSelector = new SelectorView(titles);
            this.typeSelector.AddClickHandler(this.UpdateSubview);



            //this.absoluteRadioButton = new RadioButton();
            //this.absoluteRadioButton.Content = "Absolute";
            //this.absoluteRadioButton.HorizontalAlignment = HorizontalAlignment.Center;
            //this.absoluteRadioButton.Click += new System.Windows.RoutedEventHandler(this.UpdateSubview);
            //typeGrid.AddItem(this.absoluteRadioButton);

            //this.relativeRadioButton = new RadioButton();
            //this.relativeRadioButton.Content = "Relative";
            //this.relativeRadioButton.HorizontalAlignment = HorizontalAlignment.Center;
            //this.relativeRadioButton.Click += new System.Windows.RoutedEventHandler(this.UpdateSubview);
            //typeGrid.AddItem(this.relativeRadioButton);

            this.displayGrid.AddItem(this.typeSelector);

        }


        private void SetupSubviews()
        {
            this.absoluteRatingEntryView = new AbsoluteRatingEntryView();
            this.relativeRatingEntryView = new RelativeRatingEntryView();
        }

        public void Clear()
        {
            //this.relativeRatingEntryView
            this.absoluteRatingEntryView.Text = "";
        }
        public Rating GetRating(ActivityDatabase activities, Engine engine, Participation participation)
        {
            if (this.ShowRelativeEntryView())
            {
                return this.relativeRatingEntryView.GetRating(activities, engine, participation);
            }
            else
            {
                return this.absoluteRatingEntryView.GetRating();
            }
        }
        public Participation LatestParticipation
        {
            set
            {
                this.relativeRatingEntryView.LatestParticipation = value;
            }
        }
        private bool ShowRelativeEntryView()
        {
            if (this.typeSelector.SelectedItemText == "Relative")
                return true;
            return false;
        }
        void UpdateSubview(object sender, System.Windows.RoutedEventArgs e)
        {
            this.UpdateSubview();
        }
        void UpdateSubview()
        {
            if (this.ShowRelativeEntryView())
                this.displayGrid.PutItem(this.relativeRatingEntryView, 2, 0);
            else
                this.displayGrid.PutItem(this.absoluteRatingEntryView, 2, 0);
        }

        private SelectorView typeSelector;
        private DisplayGrid displayGrid;
        private AbsoluteRatingEntryView absoluteRatingEntryView;
        private RelativeRatingEntryView relativeRatingEntryView;
    }
}
