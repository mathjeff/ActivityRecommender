using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveInterpolation;

// A ScoreSummary describes the (exponentially-weighted) average value of some progression after a certain point in time
namespace ActivityRecommendation
{
    class ScoreSummary : LazyDimension_Datapoint<Distribution>
    {
        // If useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint is false, then this summary won't update if `when` is before the RatingSummarizer's first datapoint
        // The reason someone might want this would be if they want to make a bunch of ScoreSummary objects and don't want lots of repeats all saying the same thing
        public ScoreSummary(DateTime when, bool useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint = false)
        {
            this.earliestKnownDate = when;
            this.latestKnownDate = when;
            this.useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint = useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint;
        }

        // Pulls all newer ratings from the RatingSummarizer
        public void Update(ScoreSummarizer summarizer)
        {
            DateTime latestDate = summarizer.LatestKnownDate;
            // fetch any ratings that appeared since our last update
            this.Update(summarizer, this.latestKnownDate, latestDate);
        }

        // Pulls new ratings from the RatingSummarizer within a certain date range and updates metadata (min/max)
        public void Update(ScoreSummarizer summarizer, DateTime earliestDateToInclude, DateTime latestDateToInclude)
        {
            if (!this.useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint)
            {
                bool earlierThanFirstDatapoint = this.earliestKnownDate.CompareTo(summarizer.EarliestKnownDate) < 0;
                if (earlierThanFirstDatapoint)
                    return;
            }

            if (earliestDateToInclude.CompareTo(this.earliestKnownDate) < 0)
            {
                bool endInclusive = (this.values.Weight <= 0);
                this.importData(summarizer, earliestDateToInclude, this.earliestKnownDate, true, endInclusive);
                this.earliestKnownDate = earliestDateToInclude;
            }

            bool startInclusive = (this.values.Weight <= 0);
            int endComparison = latestDateToInclude.CompareTo(this.latestKnownDate);
            if (endComparison > 0 || (startInclusive && endComparison >= 0))
            {
                this.importData(summarizer, this.latestKnownDate, latestDateToInclude, startInclusive, true);
                this.latestKnownDate = latestDateToInclude;
            }
        }

        // implements the pull of new data from the RatingSummarizer
        private void importData(ScoreSummarizer summarizer, DateTime earliestDateToInclude, DateTime latestDateToInclude, bool startInclusive, bool endInclusive)
        {
            this.values = this.values.Plus(summarizer.GetValueDistributionForDates(earliestDateToInclude, latestDateToInclude, startInclusive, endInclusive));
        }


        public Distribution Item
        {
            get
            {
                if (this.values.Weight == 0)
                    return this.values;
                // this.values contains a summary of all of the future happinesses that happen after this.earliestKnownDate
                // So, this.values might have a high stddev because it describes a population
                // However, this.Item describes the net present value of the user's happiness now, which is an average
                // So, we ignore the standard deviation here
                return Distribution.MakeDistribution(this.values.Mean, 0, 1);
            }
        }


        #region Required for lazy datapoint

        public LazyInputs Inputs { get; set; }
        public LazyInputs GetInputs()
        {
            return this.Inputs;
        }
        public Distribution GetOutput()
        {
            return this.Item;
        }

        #endregion

        DateTime earliestKnownDate;  // the date that this RatingSummary describes
        DateTime latestKnownDate;   // the date of the latest rating known to this RatingSummary
        bool useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint;
        Distribution values = Distribution.Zero;
    }
}
