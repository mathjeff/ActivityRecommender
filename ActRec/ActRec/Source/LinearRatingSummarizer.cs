using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// An ExpontentialRatingSummarizer computes the (exponentially weighted) average value for a bunch of ratings and participations within a certain period of time
// Note that it takes into account both the scores of the ratings and the fraction of time spent idle
namespace ActivityRecommendation
{
    public class LinearRatingSummarizer : ScoreSummarizer
    {
        // The weight of each rating (continuously) decays exponentially, and cuts in half after a TimeSpan of halfLife
        public LinearRatingSummarizer()
        {
        }

        // returns the cumulative weight for all dates through the given date
        protected override double GetWeightThroughDate(DateTime when)
        {
            TimeSpan duration = when.Subtract(this.firstDate);
            return duration.TotalSeconds;
        }
    }
}