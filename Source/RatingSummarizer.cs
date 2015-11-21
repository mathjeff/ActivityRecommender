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
            this.ratingsByDate = new StatList<DateTime,Distribution>(new DateComparer(), new DistributionAdder());
            this.participationIntensitiesByDate = new StatList<DateTime, Distribution>(new DateComparer(), new DistributionAdder());
        }
        // TODO: replace the StatList (which has key equal to a DateTime) with an NDFinder (which will have a 2d key: (index, date))
        // This will allow objects to say "I want all items that are after index x (which are the ones I don't yet have) and 
        //      after DateTime d (which are the ones I care about)"
        // Calling this function declares that the time the user spent between startDate and endDate had a constant value equal to the given score
        public void AddRating(DateTime startDate, DateTime endDate, double score)
        {
            if (this.ratingsByDate.NumItems == 0 && this.participationIntensitiesByDate.NumItems == 0)
                this.firstDate = startDate;
            // the integral through startDate
            double startingWeight = this.GetWeightThroughDate(startDate);
            // the integral through endDate
            double endingWeight = this.GetWeightThroughDate(endDate);
            // the total weight between the starting and ending dates
            double overallWeight = endingWeight - startingWeight;
            Distribution additionalDistribution = Distribution.MakeDistribution(score, 0, overallWeight);
            // now add the value
            this.ratingsByDate.Add(startDate, additionalDistribution);
        }
        // Calling this function declares that user put to good use <intensity> (a fraction) of his/her time between startDate and endDate to some meaningful use
        public void AddParticipationIntensity(DateTime startDate, DateTime endDate, double intensity)
        {
            if (this.ratingsByDate.NumItems == 0 && this.participationIntensitiesByDate.NumItems == 0)
                this.firstDate = startDate;
            // the integral through startDate
            double startingWeight = this.GetWeightThroughDate(startDate);
            // the integral through endDate
            double endingWeight = this.GetWeightThroughDate(endDate);
            // the total weight between the starting and ending dates
            double overallWeight = endingWeight - startingWeight;
            Distribution additionalDistribution = Distribution.MakeDistribution(intensity, 0, overallWeight);
            // now add the value
            this.participationIntensitiesByDate.Add(startDate, additionalDistribution); 
        }
        public void RemoveParticipation(DateTime startDate)
        {
            this.participationIntensitiesByDate.Remove(startDate);
        }
        // Returns a distribution of appropriately weighted scores between startDate and endDate
        // In the future, the weight of the resultant distribution might adjusted so that any date equal to referenceDate has weight equal to 1
        public Distribution GetValueDistributionForDates(DateTime startDate, DateTime endDate)
        {
            Distribution ratings = this.ratingsByDate.CombineBetweenKeys(startDate, true, endDate, false);
            Distribution participationIntensities = this.participationIntensitiesByDate.CombineBetweenKeys(startDate, true, endDate, false);
            double usefulFraction = participationIntensities.Mean;
            double averageRating = ratings.Mean;
            double overallValue = usefulFraction * averageRating;
            Distribution result = Distribution.MakeDistribution(overallValue, 0, ratings.Weight);
            return result;
        }
        public DateTime LatestKnownDate
        {
            get
            {
                ListItemStats<DateTime, Distribution> stats = this.ratingsByDate.GetLastValue();
                if (stats == null)
                    return new DateTime();
                return stats.Key;
            }
        }
        // returns the cumulative weight for all dates through the given date
        protected abstract double GetWeightThroughDate(DateTime when);

        private StatList<DateTime, Distribution> ratingsByDate;
        private StatList<DateTime, Distribution> participationIntensitiesByDate;
        protected DateTime firstDate;
        private TimeSpan halfLife;
    }
}
