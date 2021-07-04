using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// An ExpontentialRatingSummarizer computes the (exponentially weighted) average value for a bunch of ratings and participations within a certain period of time
// Note that it takes into account both the scores of the ratings and the fraction of time spent idle
namespace ActivityRecommendation
{
    public class ExponentialRatingSummarizer : ScoreSummarizer
    {
        // The weight of each rating (continuously) decays exponentially, and cuts in half after a TimeSpan of halfLife
        public ExponentialRatingSummarizer(TimeSpan halfLife)
        {
            this.halfLife = halfLife;
        }
        public TimeSpan HalfLife
        {
            get
            {
                return this.halfLife;
            }
        }

        // returns the total weight for all dates through the given date
        public override double GetWeightBetweenDates(DateTime startDate, DateTime endDate)
        {
            double durationWeight = 1 - this.getWeightAfterDuration(endDate.Subtract(startDate));

            TimeSpan wait = startDate.Subtract(this.firstDate);
            double waitMultiplier = this.getWeightAfterDuration(wait);

            double result = durationWeight * waitMultiplier;
            return result;
        }

        // returns the total weight for all dates after the given date
        public double GetWeightAfterDate(DateTime startDate)
        {
            return this.getWeightAfterDuration(startDate.Subtract(this.firstDate));
        }
        // returns the amount of weight that occurs after the given duration divided by the total amount of weight
        private double getWeightAfterDuration(TimeSpan duration)
        {
            return Math.Pow(2, -duration.TotalSeconds / this.halfLife.TotalSeconds);
        }
        private TimeSpan halfLife;
    }
}