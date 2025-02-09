﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveInterpolation;

// A ScoreSummary describes the (exponentially-weighted) average value of some progression after a certain point in time
namespace ActivityRecommendation
{
    class ScoreSummary : ILazyDatapoint<Distribution>
    {
        // If useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint is false, then this summary won't update if `when` is before the RatingSummarizer's first datapoint
        // The reason someone might want this would be if they want to make a bunch of ScoreSummary objects and don't want lots of repeats all saying the same thing
        public ScoreSummary(DateTime when, bool useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint = false)
        {
            this.startDate = when;
            this.endDate = when;
            this.useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint = useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint;
        }

        // Pulls all newer ratings from the RatingSummarizer
        public void Update(ExponentialRatingSummarizer summarizer)
        {
            DateTime latestDate = summarizer.LatestKnownDate;
            // fetch any ratings that appeared since our last update
            this.Update(summarizer, this.endDate, latestDate);
        }

        // Pulls new ratings from the RatingSummarizer within a certain date range and updates metadata (min/max)
        public void Update(ExponentialRatingSummarizer summarizer, DateTime earliestDateToInclude, DateTime latestDateToInclude)
        {
            if (!this.useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint)
            {
                bool earlierThanFirstDatapoint = this.startDate.CompareTo(summarizer.EarliestKnownDate) < 0;
                if (earlierThanFirstDatapoint)
                    return;
            }

            if (earliestDateToInclude.CompareTo(this.startDate) < 0)
            {
                bool endInclusive = (this.values.Weight <= 0);
                this.importData(summarizer, earliestDateToInclude, this.startDate, true, endInclusive);
                this.startDate = earliestDateToInclude;
            }

            bool startInclusive = (this.values.Weight <= 0);
            int endComparison = latestDateToInclude.CompareTo(this.endDate);
            if (endComparison > 0 || (startInclusive && endComparison >= 0))
            {
                this.importData(summarizer, this.endDate, latestDateToInclude, startInclusive, true);
                this.endDate = latestDateToInclude;
            }
            this.timeWeight = 1 - (summarizer.GetWeightAfterDate(this.endDate) / summarizer.GetWeightAfterDate(this.startDate));
        }

        // implements the pull of new data from the RatingSummarizer
        private void importData(ExponentialRatingSummarizer summarizer, DateTime earliestDateToInclude, DateTime latestDateToInclude, bool startInclusive, bool endInclusive)
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
                return Distribution.MakeDistribution(this.values.Mean, 0, this.timeWeight);
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

        public DateTime StartDate
        {
            get
            {
                return this.startDate;
            }
        }

        DateTime startDate;  // the date that this RatingSummary describes
        DateTime endDate;   // the date of the latest rating known to this RatingSummary
        bool useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint;

        // The weight assigned to these values is the weight of the ratings we know about
        // If a lot of time elapsed with very few ratings, the weight of this value will be small
        Distribution values = Distribution.Zero;

        // This weight is the weight of the time that this summary encompasses
        // If a lot of time elapsed with very few ratings, the weight of this value will be high
        double timeWeight;
    }
}
