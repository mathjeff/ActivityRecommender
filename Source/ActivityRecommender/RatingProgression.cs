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
                if (newRating.Date != null)
                {
                    // get the timestamp
                    DateTime when = (DateTime)newRating.Date;
                    // get the score
                    double score = newRating.Score;
                    // make a Distribution
                    Distribution newDistribution = new Distribution(score, score * score, 1);
                    // record it
                    this.searchHelper.Add(when, newDistribution);
                    this.ratingsInDiscoveryOrder.Add(newRating);
                }
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

        // returns a list of ratings, sorted by the date that they were added to this RatingProgression
        public List<AbsoluteRating> GetRatingsInDiscoveryOrder()
        {
            return this.ratingsInDiscoveryOrder;
        }

        public int NumItems
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
            if (rating1.Date == null || rating2.Date == null)
            {
                throw new ArgumentException("Cannot compare ratings whose dates are null");
            }
            DateTime date1 = (DateTime)rating1.Date;
            DateTime date2 = (DateTime)rating2.Date;
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
                ProgressionValue defaultValue = new ProgressionValue(when, new Distribution(0, 0, 0), -1);
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
            ProgressionValue result = new ProgressionValue(when, sum, previousCount);
            return result;
        }
        public ProgressionValue GetCurrentValue(DateTime when)
        {
            Distribution distribution = this.Owner.PredictedScore.Distribution;
            ProgressionValue result = new ProgressionValue(when, distribution, this.NumItems);
            return result;
        }
        public List<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            int i;
            List<ProgressionValue> results = new List<ProgressionValue>();
            for (i = indexInclusive; i < this.ratingsInDiscoveryOrder.Count; i++)
            {
                AbsoluteRating rating = this.ratingsInDiscoveryOrder[i];
                Distribution distribution = Distribution.MakeDistribution(rating.Score, 0, 1);
                ProgressionValue value = new ProgressionValue((DateTime)rating.Date, distribution, i);
                results.Add(value);
                //ProgressionValue value = new ProgressionValue(
                //results.Add((DateTime)this.ratingsInDiscoveryOrder[i].Date);
            }
            return results;
        }

        public string Description
        {
            get
            {
                return "How you've rated this recently";
            }
        }

        #endregion

        #region Private Member Functions


        #endregion

        //private bool useFutureRatings;   // tells whether we care about ratings where ratingSource.IsBasedOnFutureEvents() is true
        private StatList<DateTime, Distribution> searchHelper;
        private List<AbsoluteRating> ratingsInDiscoveryOrder;
    }
}