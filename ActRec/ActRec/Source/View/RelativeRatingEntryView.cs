using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using VisiPlacement;

using System.Windows.Input;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class RelativeRatingEntryView : TitledControl
    {
        public RelativeRatingEntryView() : base("Relative Score")
        {
            this.mainDisplayGrid = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);

            this.scaleBlock = new Editor();
            this.scaleBlock.Keyboard = Keyboard.Numeric;
            this.scaleBlock.TextChanged += this.ScaleBlock_TextChanged;

            this.Clear();
            this.mainDisplayGrid.AddLayout(new TextboxLayout(this.scaleBlock));

            this.nameBlock = new TextblockLayout();
            this.mainDisplayGrid.AddLayout(this.nameBlock);

            this.SetContent(this.mainDisplayGrid);
            this.Placeholder("(Optional)");
        }

        private void ScaleBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateDateColor();
        }
        private bool IsRatioValid()
        {
            return (this.GetRatio() != null);
        }
        public double? GetRatio()
        {
            string text = this.scaleBlock.Text;
            try
            {
                double value = double.Parse(this.scaleBlock.Text);
                if (value < 0)
                    return null;
                return value;
            }
            catch (FormatException)
            {
                return null;
            }
        }
        public void Placeholder(string text)
        {
            this.scaleBlock.Placeholder = text;
        }
        void UpdateDateColor()
        {
            if (this.IsRatioValid() || this.scaleBlock.Text == "")
                this.AppearValid();
            else
                this.AppearInvalid();
        }

        // alters the appearance to indicate that the given date is not valid
        void AppearValid()
        {
            this.scaleBlock.BackgroundColor = Color.LightGray;
        }

        // alters the appearance to indicate that the given date is not valid
        void AppearInvalid()
        {
            this.scaleBlock.BackgroundColor = Color.Red;
        }

        public Participation LatestParticipation
        {
            set
            {
                this.latestParticipation = value;

                if (this.latestParticipation != null)
                {
                    DateTime now = DateTime.Now;
                    string dateFormatString;
                    // Show the day if it happened more than 24 hours ago
                    if (now.Subtract(value.StartDate).CompareTo(new TimeSpan(24, 0, 0)) > 0)
                        dateFormatString = "yyyy-MM-ddTHH:mm:ss";
                    else
                        dateFormatString = "HH:mm:ss";
                    this.nameBlock.setText("times the score of your latest participation in " + value.ActivityDescriptor.ActivityName + " from " + value.StartDate.ToString(dateFormatString) + " to " + value.EndDate.ToString(dateFormatString));
                }
                else
                {
                    this.nameBlock.setText("You haven't done anything to compare to!");
                }
            }
            get
            {
                return this.latestParticipation;
            }
        }
        // creates the rating to assign to the given Participation
        public Rating GetRating(Engine engine, Participation participation)
        {
            // abort if null input
            double? maybeScale = this.GetRatio();
            if (maybeScale == null)
                return engine.MakeEstimatedRating(participation);
            double scale = maybeScale.Value;
            if (this.latestParticipation == null)
                return null;
            RelativeRating rating = engine.MakeRelativeRating(participation, scale, this.latestParticipation);
            return rating;
        }

        public void Clear()
        {
            this.scaleBlock.Text = "";
        }

        private GridLayout mainDisplayGrid;
        private Participation latestParticipation;
        private TextblockLayout nameBlock;
        private Editor scaleBlock;
    }
}
