using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    class RelativeRatingEntryView : TitledControl, IResizable
    {
        public RelativeRatingEntryView() : base("Comparison")
        {
            this.mainDisplayGrid = new DisplayGrid(2, 1);

            this.scaleBlock = new TitledTextbox("Score equals");
            this.scaleBlock.Text = "2.0";
            this.mainDisplayGrid.AddItem(this.scaleBlock);

            /*
            List<string> titles = new List<string>();
            titles.Add("Better than:");
            titles.Add("Worse than:");
            this.comparisonSelector = new SelectorView(titles);
            this.AddItem(comparisonSelector);

            */

            //this.nameBox = new ActivityNameEntryBox("the most recent instance of:");
            //this.AddItem(this.nameBox);

            this.nameBlock = new ResizableTextBlock();
            this.nameBlock.SetResizability(new Resizability(1, 1));
            this.mainDisplayGrid.AddItem(this.nameBlock);

            this.SetContent(this.mainDisplayGrid);
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            Size result = base.MeasureOverride(constraint);
            return result;
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Size result = base.ArrangeOverride(arrangeSize);
            return result;
        }
        public Participation LatestParticipation
        {
            set
            {
                this.latestParticipation = value;

                if (this.latestParticipation != null)
                {
                    string dateFormatString = "yyyy-MM-ddTHH:mm:ss";
                    this.nameBlock.Text = "times the score of the latest participation in " + value.ActivityDescriptor.ActivityName + " from " + value.StartDate.ToString(dateFormatString) + " to " + value.EndDate.ToString(dateFormatString);
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
            if (this.latestParticipation == null)
                return null;
            // make the RelativeRating
            RelativeRating rating = new RelativeRating();
            // make an AbsoluteRating for the other Activity
            AbsoluteRating otherRating = new AbsoluteRating();
            otherRating.ActivityDescriptor = this.latestParticipation.ActivityDescriptor;
            otherRating.Date = this.latestParticipation.StartDate;
            Activity otherActivity = activities.ResolveDescriptor(otherRating.ActivityDescriptor);
            engine.EstimateValue(otherActivity, (DateTime)otherRating.Date);
            Distribution otherEstimate = otherActivity.PredictedScore.Distribution;

            // make an AbsoluteRating for this Activity
            AbsoluteRating thisRating = new AbsoluteRating();
            // figure out which activity this one is
            Activity thisActivity = activities.ResolveDescriptor(participation.ActivityDescriptor);
            // calculate the predicted rating for this activity
            engine.MakeRecommendation(thisActivity, participation.StartDate);
            Distribution thisEstimate = thisActivity.PredictedScore.Distribution;

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

        //private SelectorView comparisonSelector;
        //private ActivityNameEntryBox nameBox;
        private DisplayGrid mainDisplayGrid;
        private ResizableTextBlock nameBlock;
        private Participation latestParticipation;
        private TitledTextbox scaleBlock;
    }
}
