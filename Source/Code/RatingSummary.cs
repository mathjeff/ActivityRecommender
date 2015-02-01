using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveLinearInterpolation;

// A RatingSummary describes the (exponentially-weighted) average rating of ratings after a certain point in time
namespace ActivityRecommendation
{
    class RatingSummary : IDatapoint<Distribution>
    {
        public RatingSummary(DateTime when)
        {
            this.earliestKnownDate = when;
            this.latestKnownDate = when;
            this.ratings = new Distribution();
        }

        // Pulls all newer ratings from the RatingSummarizer
        public void Update(RatingSummarizer summarizer)
        {
            DateTime latestDate = summarizer.LatestKnownDate;
            // fetch any ratings that appeared since our last update
            this.Update(summarizer, this.latestKnownDate, latestDate);
            //this.ratings = this.ratings.Plus(summarizer.GetValueDistributionForDates(this.date, this.latestKnownDate, latestDate));
            //this.ratings = this.ratings.Plus(summarizer.GetValueDistributionForDates(this.latestKnownDate, latestDate));
            // keep track of the date of our last update
            //this.latestKnownDate = latestDate;
        }

        // Pulls new ratings from the RatingSummarizer within a certain date range
        public void Update(RatingSummarizer summarizer, DateTime earliestDateToInclude, DateTime latestDateToInclude)
        {
            if (earliestDateToInclude.CompareTo(this.earliestKnownDate) < 0)
            {
                this.ratings = this.ratings.Plus(summarizer.GetValueDistributionForDates(earliestDateToInclude, this.earliestKnownDate));
                this.earliestKnownDate = earliestDateToInclude;
            }
            if (latestDateToInclude.CompareTo(this.latestKnownDate) > 0)
            {
                this.ratings = this.ratings.Plus(summarizer.GetValueDistributionForDates(this.latestKnownDate, latestDateToInclude));
                this.latestKnownDate = latestDateToInclude;
            }
        }

        #region Required for IDatapoint

        public double[] InputCoordinates { get; set; }
        public int NumInputDimensions
        {
            get
            {
                if (this.InputCoordinates == null)
                    return 0;
                return this.InputCoordinates.Length;
            }
        }
        public Distribution Score
        {
            get
            {
                return this.ratings.CopyAndReweightTo(1);
                //Distribution result = this.ratings.CopyAndReweightTo(1);
                //Distribution reverse = Distribution.MakeDistribution(1 - result.Mean, result.StdDev, result.Weight);
                //return reverse;
            }
        }
        public double[] OutputCoordinates
        {
            get
            {
                return null;
            }
        }

        #endregion
        
        DateTime earliestKnownDate;  // the date that this RatingSummary describes
        DateTime latestKnownDate;   // the date of the latest rating known to this RatingSummary
        Distribution ratings;
    }
}
