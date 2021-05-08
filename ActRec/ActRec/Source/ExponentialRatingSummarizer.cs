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

        // returns the cumulative weight for all dates through the given date
        protected override double GetWeightBetweenDates(DateTime startDate, DateTime endDate)
        {
            TimeSpan duration = endDate.Subtract(startDate);
            double durationWeight = 1 - Math.Pow(2, -duration.TotalSeconds / this.halfLife.TotalSeconds);

            TimeSpan wait = startDate.Subtract(this.firstDate);
            double waitMultiplier = Math.Pow(2, -wait.TotalSeconds / this.halfLife.TotalSeconds);

            double result = durationWeight * waitMultiplier;
            return result;
        }

        private TimeSpan halfLife;
    }
}