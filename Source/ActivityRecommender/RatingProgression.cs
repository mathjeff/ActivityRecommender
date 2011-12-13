using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

namespace ActivityRecommendation
{
    public class RatingProgression : IComparer<AbsoluteRating>, IProgression
    {
        #region Constructor

        public RatingProgression(Activity owner)
        {
            this.searchHelper = new StatList<DateTime, Distribution>(new DateComparer(), new DistributionAdder());
            this.ratingsInDiscoveryOrder = new List<AbsoluteRating>();
            this.Owner = owner;
        }

        #endregion

        #region Public Member Functions

        public void AddRating(AbsoluteRating newRating)
        {
            if (this.ShouldIncludeRating(newRating))
            {
                // get the timestamp
                DateTime when = newRating.Date;
                // get the score
                double score = newRating.Score;
                // make a Distribution
                Distribution newDistribution = new Distribution(score, score * score, 1);
                // record it
                this.searchHelper.Add(when, newDistribution);
                this.ratingsInDiscoveryOrder.Add(newRating);
            }
        }


        // tells whether this RatingProgression cares about the provided rating
        public bool ShouldIncludeRating(AbsoluteRating newRating)
        {
            return true;
            /*
            // If we later wish to filter the ratings that we include, we would do it like this:
            if (this.useFutureRatings)
            {
                return newRating.GetRatingSource().IsBasedOnFutureEvent();
            }
            else
            {
                return newRating.GetRatingSource().IsBasedOnPastEvent();
            }*/
        }

        public void Test()
        {
            DateTime date1;
            AbsoluteRating newRating;

            date1 = new DateTime(100000000);
            newRating = new AbsoluteRating(0, date1, null, null);
            this.AddRating(newRating);
            //this.AddRating(newRating);

            date1 = new DateTime(120000000);
            newRating = new AbsoluteRating(.25, date1, null, null);
            this.AddRating(newRating);
            //this.AddRating(newRating);

            date1 = new DateTime(150000000);
            newRating = new AbsoluteRating(.5, date1, null, null);
            this.AddRating(newRating);
            //this.AddRating(newRating);

            date1 = new DateTime(170000000);
            newRating = new AbsoluteRating(.75, date1, null, null);
            this.AddRating(newRating);
            //this.AddRating(newRating);

            //date1 = new DateTime(120000000);
            //this.GetValueAt(date1, true);

            ListItemStats<DateTime, Distribution> stats;
            stats = this.searchHelper.FindPreviousItem(new DateTime(90000000), true);
            stats = this.searchHelper.FindPreviousItem(new DateTime(90000000), false);

            stats = this.searchHelper.FindPreviousItem(new DateTime(100000000), true);
            stats = this.searchHelper.FindPreviousItem(new DateTime(100000000), false);

            stats = this.searchHelper.FindPreviousItem(new DateTime(110000000), true);
            stats = this.searchHelper.FindPreviousItem(new DateTime(110000000), false);

            stats = this.searchHelper.FindPreviousItem(new DateTime(120000000), true);
            stats = this.searchHelper.FindPreviousItem(new DateTime(120000000), false);

            stats = this.searchHelper.FindPreviousItem(new DateTime(130000000), true);
            stats = this.searchHelper.FindPreviousItem(new DateTime(130000000), false);

            stats = this.searchHelper.FindPreviousItem(new DateTime(150000000), true);
            stats = this.searchHelper.FindPreviousItem(new DateTime(150000000), false);

            stats = this.searchHelper.FindPreviousItem(new DateTime(160000000), true);
            stats = this.searchHelper.FindPreviousItem(new DateTime(160000000), false);

            stats = this.searchHelper.FindPreviousItem(new DateTime(170000000), true);
            stats = this.searchHelper.FindPreviousItem(new DateTime(170000000), false);

            stats = this.searchHelper.FindPreviousItem(new DateTime(180000000), true);
            stats = this.searchHelper.FindPreviousItem(new DateTime(180000000), false);

            this.GetValueAt(new DateTime(190000000), true);
            return;

        }

        // returns a list of ratings, sorted by the date that they were added to this RatingProgression
        public List<AbsoluteRating> GetRatingsInDiscoveryOrder()
        {
            return this.ratingsInDiscoveryOrder;
        }

        public int NumRatings
        {
            get
            {
                return this.searchHelper.NumItems;
            }
        }
        #endregion

        // for binary searches
        #region IComparerer<AbsoluteRating>

        public int Compare(AbsoluteRating rating1, AbsoluteRating rating2)
        {
            DateTime date1 = rating1.Date;
            DateTime date2 = rating2.Date;
            return date1.CompareTo(date2);
        }
        
        #endregion

        #region Functions for IProgression

        public Activity Owner { get; set; }

        // returns basically the average of the recent rating at that date
        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            // find the most recent rating before the given date
            ListItemStats<DateTime, Distribution> latestItem = this.searchHelper.FindPreviousItem(when, true);
            if (latestItem == null)
            {
                ProgressionValue defaultValue = new ProgressionValue(new Distribution(0, 0, 0), -1);
                return defaultValue;
            }
            // get some statistics
            Distribution latestDistribution = latestItem.Value;
            DateTime latestDate = latestItem.Key;
            // compute how long ago that rating was given
            TimeSpan duration = when.Subtract(latestDate);
            // create another date that is twice as far in the past
            DateTime earlierDate = latestDate.Subtract(duration);
            // add up everything that occurred between the earlier day and now
            Distribution sum = this.searchHelper.SumBetweenKeys(earlierDate, true, when, !strictlyEarlier);
            int previousCount = this.searchHelper.CountBeforeKey(when, strictlyEarlier);
            ProgressionValue result = new ProgressionValue(sum, previousCount);
            return result;
        }

        #endregion

        #region Private Member Functions


        #endregion

        //private bool useFutureRatings;   // tells whether we care about ratings where ratingSource.IsBasedOnFutureEvents() is true
        private StatList<DateTime, Distribution> searchHelper;
        private List<AbsoluteRating> ratingsInDiscoveryOrder;
    }
}