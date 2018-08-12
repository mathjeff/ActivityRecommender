using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// A RatingSummarizer computes the average value for a bunch of ratings and participations within a certain period of time
// Note that it takes into account both the scores of the ratings and the fraction of time spent idle
namespace ActivityRecommendation
{
    public abstract class RatingSummarizer
    {
        // The weight of each rating (continuously) decays exponentially, and cuts in half after a TimeSpan of halfLife
        public RatingSummarizer()
        {
            this.valuesByDate = new StatList<DateTime,Distribution>(new DateComparer(), new DistributionAdder());
        }
        // TODO: replace the StatList (which has key equal to a DateTime) with an NDFinder (which will have a 2d key: (index, date))
        // This will allow objects to say "I want all items that are after index x (which are the ones I don't yet have) and 
        //      after DateTime d (which are the ones I care about)"
        // Calling this function declares that the time the user spent between startDate and endDate had a constant value equal to the given score
        public void AddRating(DateTime startDate, DateTime endDate, double score)
        {
            if (this.valuesByDate.NumItems == 0)
                this.firstDate = startDate;
            // the integral through startDate
            double startingWeight = this.GetWeightThroughDate(startDate);
            // the integral through endDate
            double endingWeight = this.GetWeightThroughDate(endDate);
            // the total weight between the starting and ending dates
            double overallWeight = endingWeight - startingWeight;
            Distribution additionalDistribution = Distribution.MakeDistribution(score, 0, overallWeight);
            this.AddValue(startDate, additionalDistribution);
        }
        // Calling this function declares that user put to good use <intensity> (a fraction) of his/her time between startDate and endDate to some meaningful use
        public void AddParticipationIntensity(DateTime startDate, DateTime endDate, double intensity)
        {
            if (this.valuesByDate.NumItems == 0)
                return; // any Skip or Participation before the first Rating is ignored (because the weight of each Skip is so small, and we don't want it to accidentally dominate)

            // the integral through startDate
            double startingWeight = this.GetWeightThroughDate(startDate);
            // the integral through endDate
            double endingWeight = this.GetWeightThroughDate(endDate);
            // the total weight between the starting and ending dates
            double maxPossibleWeight = endingWeight - startingWeight;
            // We record the total time the user declared to have put to good use (by giving a rating in this.AddRating)
            // We record the total time the user declared to have put to bad use (by giving a rating, or by skipping)
            // The rating contributes both a good and a bad portion; the skip contributes only a bad portion
            // This should cause the engine to suggest activities that are more likely to be rated
            
            // Probably a better system to use would be to assign a fake, estimated rating to each unrated participation
            // Using estimated ratings would require making sure that a lack of confidence in the guesses translates into lower weights
            // It would also require recomputing estimated ratings (slow) or caching them long-term (less accurate)

            double waste = (1 - intensity) * maxPossibleWeight;
            if (waste > 0)
            {
                Distribution additionalDistribution = Distribution.MakeDistribution(0, 0, waste);
                this.AddValue(startDate, additionalDistribution);
            }
        }
        private void AddValue(DateTime when, Distribution value)
        {
            this.valuesByDate.Add(when, value);
        }
        public void RemoveParticipation(DateTime startDate)
        {
            this.valuesByDate.Remove(startDate);
        }
        // Returns a distribution of scores (possibly weighted by time) between startDate and endDate
        // In the future, the weight of the resultant distribution might adjusted so that any date equal to referenceDate has weight equal to 1
        public Distribution GetValueDistributionForDates(DateTime startDate, DateTime endDate)
        {
            return this.valuesByDate.CombineBetweenKeys(startDate, true, endDate, false);
        }
        public DateTime LatestKnownDate
        {
            get
            {
                ListItemStats<DateTime, Distribution> stats = this.valuesByDate.GetLastValue();
                if (stats == null)
                    return new DateTime();
                return stats.Key;
            }
        }
        public DateTime EarliestKnownDate
        {
            get
            {
                ListItemStats<DateTime, Distribution> stats = this.valuesByDate.GetFirstValue();
                if (stats == null)
                    return new DateTime();
                return stats.Key;
            }
        }
        // returns the cumulative weight for all dates through the given date
        protected abstract double GetWeightThroughDate(DateTime when);

        private StatList<DateTime, Distribution> valuesByDate;
        protected DateTime firstDate;
        
    }
}
