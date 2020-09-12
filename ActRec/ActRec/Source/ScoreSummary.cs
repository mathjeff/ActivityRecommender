﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveLinearInterpolation;

// A ScoreSummary describes the (exponentially-weighted) average value of some progression after a certain point in time
namespace ActivityRecommendation
{
    class ScoreSummary : IDatapoint<Distribution>
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
        public Distribution Item
        {
            get
            {
                if (this.values.Weight == 0)
                    return this.values;
                return this.values.CopyAndReweightTo(1);
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
        bool useNonzeroWeightEvenIfEarlierThanFirstSummarizerDatapoint;
        Distribution values = Distribution.Zero;
    }
}
