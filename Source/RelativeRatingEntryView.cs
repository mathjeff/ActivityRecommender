using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using VisiPlacement;
using System.Windows.Media;

namespace ActivityRecommendation
{
    class RelativeRatingEntryView : TitledControl
    {
        public RelativeRatingEntryView() : base("Relative Score (Optional)")
        {
            this.mainDisplayGrid = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);

            this.scaleBlock = new TextBox();
            this.scaleBlock.TextChanged += this.ScaleBlock_TextChanged;
            this.Clear();
            this.mainDisplayGrid.AddLayout(new TextboxLayout(this.scaleBlock));


            this.nameBlock = new TextBlock();
            this.mainDisplayGrid.AddLayout(new TextblockLayout(this.nameBlock));

            this.SetContent(this.mainDisplayGrid);
        }

        private void ScaleBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateDateColor();
        }
        private bool IsRatioValid()
        {
            return (this.GetRatio() != null);
        }
        private double? GetRatio()
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
            this.scaleBlock.Background = new SolidColorBrush(Colors.White);
        }

        // alters the appearance to indicate that the given date is not valid
        void AppearInvalid()
        {
            this.scaleBlock.Background = new SolidColorBrush(Colors.Red);
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
                    this.nameBlock.Text = "times the score of your latest participation in " + value.ActivityDescriptor.ActivityName + " from " + value.StartDate.ToString(dateFormatString) + " to " + value.EndDate.ToString(dateFormatString);
                }
                else
                {
                    this.nameBlock.Text = "You haven't done anything to compare to!";
                }
            }
        }
        // creates the rating to assign to the given Participation
        public Rating GetRating(ActivityDatabase activities, Engine engine, Participation participation)
        {
            double? maybeScale = this.GetRatio();
            if (maybeScale == null)
                return null;
            double scale = maybeScale.Value;
            
            
            if (this.latestParticipation == null)
                return null;
            // make the RelativeRating
            RelativeRating rating = new RelativeRating();
            // make an AbsoluteRating for the other Activity
            AbsoluteRating otherRating = new AbsoluteRating();
            otherRating.ActivityDescriptor = this.latestParticipation.ActivityDescriptor;
            otherRating.Date = this.latestParticipation.StartDate;
            Activity otherActivity = activities.ResolveDescriptor(otherRating.ActivityDescriptor);
            engine.EstimateRating(otherActivity, (DateTime)otherRating.Date);
            Distribution otherEstimate = otherActivity.PredictedScore.Distribution;

            // make an AbsoluteRating for this Activity
            AbsoluteRating thisRating = new AbsoluteRating();
            // figure out which activity this one is
            Activity thisActivity = activities.ResolveDescriptor(participation.ActivityDescriptor);
            // calculate the predicted rating for this activity
            engine.MakeRecommendation(thisActivity, participation.StartDate, TimeSpan.FromSeconds(0));
            Distribution thisEstimate = thisActivity.PredictedScore.Distribution;

            // now we compute updated scores for the new activities
            thisEstimate = thisEstimate.CopyAndReweightTo(1);
            otherEstimate = otherEstimate.CopyAndReweightTo(1);
            Distribution combinedDistribution = thisEstimate.Plus(otherEstimate);
            double averageScore = combinedDistribution.Mean;
            // compute the better and worse scores
            double otherScore = 2 * averageScore / (scale + 1);
            double thisScore = 2 * averageScore - otherScore;
            

            // clamp to no more than 1
            if (thisScore > 1)
            {
                thisScore = 1;
                otherScore = thisScore / scale;
            }
            if (otherScore > 1)
            {
                otherScore = 1;
                thisScore = otherScore * scale;
            }

            thisRating.Score = thisScore;
            otherRating.Score = otherScore;
            if (scale >= 1)
            {
                rating.BetterRating = thisRating;
                rating.WorseRating = otherRating;
            }
            else
            {
                rating.BetterRating = otherRating;
                rating.WorseRating = thisRating;
            }

            rating.RawScoreScale = scale;
            return rating;
        }

        public void Clear()
        {
            this.scaleBlock.Text = "";
        }

        private GridLayout mainDisplayGrid;
        private Participation latestParticipation;
        private TextBlock nameBlock;
        private TextBox scaleBlock;
    }
}
