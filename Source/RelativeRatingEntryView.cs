using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using VisiPlacement;

namespace ActivityRecommendation
{
    class RelativeRatingEntryView : TitledControl
    {
        public RelativeRatingEntryView() : base("Relative Score (Optional)")
        {
            this.mainDisplayGrid = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);

            //TextBlock titleBox = new TextBlock();
            //titleBox.Text = "Score equals";
            //this.mainDisplayGrid.AddLayout(new TextblockLayout(titleBox));
            this.scaleBlock = new TextBox();
            this.Clear();
            this.mainDisplayGrid.AddLayout(new TextboxLayout(this.scaleBlock));


            this.nameBlock = new TextBlock();
            this.mainDisplayGrid.AddLayout(new TextblockLayout(this.nameBlock));

            this.SetContent(this.mainDisplayGrid);
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
            double scale = 1;
            try
            {
                scale = double.Parse(this.scaleBlock.Text);
            }
            catch
            {
                // if they didn't type a number, they don't get a RelativeRating
                return null;
            }

            
            
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
            engine.MakeRecommendation(thisActivity, participation.StartDate);
            Distribution thisEstimate = thisActivity.PredictedScore.Distribution;

            // make sure the scale is in a valid range
            if (scale < 0)
                return null;

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

        //private SelectorView comparisonSelector;
        //private ActivityNameEntryBox nameBox;
        private GridLayout mainDisplayGrid;
        private Participation latestParticipation;
        private TextBlock nameBlock;
        private TextBox scaleBlock;
    }
}
