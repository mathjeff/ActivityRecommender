using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;
using AdaptiveLinearInterpolation;

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
        #region IComparer<AbsoluteRating>

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
                return null;
                //ProgressionValue defaultValue = new ProgressionValue(when, new Distribution(0, 0, 0));
                //return defaultValue;
            }
            // get some statistics
            Distribution latestDistribution = latestItem.Value;
            DateTime latestDate = latestItem.Key;
            // compute how long ago that rating was given
            TimeSpan duration = when.Subtract(latestDate);
            // create another date that is twice as far in the past
            DateTime earlierDate = latestDate.Subtract(duration);
            // add up everything that occurred between the earlier day and now
            Distribution sum = this.searchHelper.CombineBetweenKeys(earlierDate, true, when, !strictlyEarlier);
            //int previousCount = this.searchHelper.CountBeforeKey(when, strictlyEarlier);
            ProgressionValue result = new ProgressionValue(when, sum);
            return result;
        }
        public ProgressionValue GetCurrentValue(DateTime when)
        {
            Distribution distribution = this.Owner.PredictedScore.Distribution;
            ProgressionValue result = new ProgressionValue(when, distribution);
            return result;
        }
        public IEnumerable<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            //int i = indexInclusive;
            List<ProgressionValue> results = new List<ProgressionValue>();
            foreach (AbsoluteRating rating in this.ratingsInDiscoveryOrder.GetRange(indexInclusive, this.ratingsInDiscoveryOrder.Count - indexInclusive))
            {
                Distribution distribution = Distribution.MakeDistribution(rating.Score, 0, 1);
                ProgressionValue value = new ProgressionValue((DateTime)rating.Date, distribution);
                results.Add(value);
                //i++;
                //ProgressionValue value = new ProgressionValue(
                //results.Add((DateTime)this.ratingsInDiscoveryOrder[i].Date);
            }
            return results;
        }
        public FloatRange EstimateOutputRange()
        {
            return new FloatRange(0, true, 1, true);
        }
        public string Description
        {
            get
            {
                return "How you've rated this recently";
            }
        }

        public Distribution Distribution
        {
            get
            {
                return this.searchHelper.CombineAll();
            }
        }

        public IEnumerable<double> GetNaturalSubdivisions(double minSubdivision, double maxSubdivision)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Member Functions


        #endregion

        //private bool useFutureRatings;   // tells whether we care about ratings where ratingSource.IsBasedOnFutureEvents() is true
        private StatList<DateTime, Distribution> searchHelper;
        private List<AbsoluteRating> ratingsInDiscoveryOrder;
    }
}