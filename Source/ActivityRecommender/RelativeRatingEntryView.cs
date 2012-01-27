using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    class RelativeRatingEntryView : DisplayGrid, IResizable
    {
        public RelativeRatingEntryView()
            : base(3, 1)
        {
            ResizableTextBlock titleBlock = new ResizableTextBlock();
            titleBlock.Text = "Comparison:";
            this.AddItem(titleBlock);

            List<string> titles = new List<string>();
            titles.Add("Better than:");
            titles.Add("Worse than:");
            this.comparisonSelector = new SelectorView(titles);
            this.AddItem(comparisonSelector);

            //this.nameBox = new ActivityNameEntryBox("the most recent instance of:");
            //this.AddItem(this.nameBox);

            this.nameBlock = new ResizableTextBlock();
            this.nameBlock.SetResizability(new Resizability(1, 1));
            this.AddItem(this.nameBlock);
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
                    this.nameBlock.Text = "latest participation in " + value.ActivityDescriptor.ActivityName + " from " + value.StartDate.ToString(dateFormatString) + " to " + value.EndDate.ToString(dateFormatString);
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

            Distribution betterDistribution;
            Distribution worseDistribution;
            if (this.comparisonSelector.SelectedItemText == "Better than:")
            {
                betterDistribution = thisEstimate;
                rating.BetterRating = thisRating;

                worseDistribution = otherEstimate;
                rating.WorseRating = otherRating;

            }
            else
            {
                betterDistribution = otherEstimate;
                rating.BetterRating = otherRating;

                worseDistribution = thisEstimate;
                rating.WorseRating = thisRating;
            }
            // the Participation currently being entered was worse than the other one
            double betterScore = betterDistribution.Mean + betterDistribution.StdDev;
            if (betterScore > 1)
                betterScore = 1;
            rating.BetterRating.Score = betterScore;

            double worseScore = worseDistribution.Mean - worseDistribution.StdDev;
            if (betterScore < 0)
                betterScore = 0;
            rating.WorseRating.Score = worseScore;
            return rating;
        }

        private SelectorView comparisonSelector;
        //private ActivityNameEntryBox nameBox;
        private ResizableTextBlock nameBlock;
        private Participation latestParticipation;
    }
}
