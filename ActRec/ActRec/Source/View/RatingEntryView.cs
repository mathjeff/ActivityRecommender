using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using VisiPlacement;

// The RatingEntryView provides a place for the user to tell how much value there was in having done a particular activity
namespace ActivityRecommendation
{
    class RatingEntryView : TitledControl
    {
        public RatingEntryView(string title)
            :base(title)
        {
            this.mainDisplay = new TitledControl("Rating Type:");
            // setup the main display grid
            this.displayGrid = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);
            //this.displayGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

            //this.SetupTypeSelection();
            this.SetupSubviews();
            this.UpdateSubview();


            this.mainDisplay.SetContent(new LayoutCache(this.displayGrid));
            this.SetContent(this.mainDisplay);
        }
#if false
        private void SetupTypeSelection()
        {
            List<string> titles = new List<string>();
            titles.Add("Absolute");
            titles.Add("Relative");
            this.typeSelector = new SelectorView(titles);
            this.typeSelector.AddClickHandler(this.UpdateSubview);
            this.typeSelector.SelectIndex(1);



            //this.absoluteRadioButton = new RadioButton();
            //this.absoluteRadioButton.Content = "Absolute";
            //this.absoluteRadioButton.HorizontalAlignment = HorizontalAlignment.Center;
            //this.absoluteRadioButton.Clicked += new System.Windows.EventHandler(this.UpdateSubview);
            //typeGrid.AddItem(this.absoluteRadioButton);

            //this.relativeRadioButton = new RadioButton();
            //this.relativeRadioButton.Content = "Relative";
            //this.relativeRadioButton.HorizontalAlignment = HorizontalAlignment.Center;
            //this.relativeRadioButton.Clicked += new System.Windows.EventHandler(this.UpdateSubview);
            //typeGrid.AddItem(this.relativeRadioButton);

            this.displayGrid.AddItem(this.typeSelector);

        }
#endif

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
            /*if (this.typeSelector.SelectedItemText == "Relative")
                return true;
            */
            return false;
        }
        void UpdateSubview(object sender, EventArgs e)
        {
            this.UpdateSubview();
        }
        void UpdateSubview()
        {
            if (this.ShowRelativeEntryView())
                this.displayGrid.PutLayout(this.relativeRatingEntryView, 1, 0);
            else
                this.displayGrid.PutLayout(this.absoluteRatingEntryView, 1, 0);
        }

        //private SelectorView typeSelector; // TODO: make this work
        private TitledControl mainDisplay;
        private GridLayout displayGrid;
        private AbsoluteRatingEntryView absoluteRatingEntryView;
        private RelativeRatingEntryView relativeRatingEntryView;
    }
}
