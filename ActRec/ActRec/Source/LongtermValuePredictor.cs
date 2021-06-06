using AdaptiveInterpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation
{
    class LongtermValuePredictor
    {
        public LongtermValuePredictor(ScoreSummarizer ratingSummarizer)
        {
            this.ratingSummarizer = ratingSummarizer;
            this.interpolator = new LazyDimension_Interpolator<Distribution>(new DistributionAdder());
            this.ratingSummariesToUpdate = new Queue<ScoreSummary>();
        }
        public AdaptiveInterpolation.Distribution Interpolate(LazyInputs coordinates)
        {
            return this.interpolator.Interpolate(coordinates);
        }
        // specifies that at <when> , the input coordinates were <coordinates> and that this is a datapoint we care to track
        public void AddDatapoint(DateTime when, LazyInputs coordinates)
        {
            ScoreSummary summary = new ScoreSummary(when);
            summary.Inputs = coordinates;
            summary.Update(this.ratingSummarizer);
            if (this.ShouldIncludeSummary(summary))
            {
                this.interpolator.AddDatapoint(summary);
                this.ratingSummariesToUpdate.Enqueue(summary);
            }
        }
        private bool ShouldIncludeSummary(ScoreSummary summary)
        {
            return summary.Item.Weight > 0;
        }
        public void UpdateMany(int count)
        {
            // Even if we have time to update many rating summaries, we don't need to update more than we have
            if (count > this.ratingSummariesToUpdate.Count)
                count = this.ratingSummariesToUpdate.Count;
            for (int i = 0; i < count; i++)
            {
                this.UpdateOne();
            }
        }
        public void UpdateOne()
        {
            if (this.ratingSummariesToUpdate.Count > 0)
            {
                ScoreSummary summary = ratingSummariesToUpdate.Dequeue();
                this.interpolator.RemoveDatapoint(summary);

                summary.Update(this.ratingSummarizer);

                this.interpolator.AddDatapoint(summary);
                this.ratingSummariesToUpdate.Enqueue(summary);
            }
        }
        // gets the average of all points in this interpolator
        public Distribution GetAverage()
        {
            return new Distribution(this.interpolator.GetAverage());
        }
        // gets the averag eof all points in this interpolator before the given DateTime
        public Distribution AverageUntil(DateTime when)
        {
            if (when.CompareTo(this.ratingSummarizer.LatestKnownDate) >= 0)
                return this.GetAverage();
            Distribution result = new Distribution();
            foreach (ScoreSummary summary in this.ratingSummariesToUpdate)
            {
                if (summary.StartDate.CompareTo(when) <= 0)
                {
                    result = result.Plus(summary.Item);
                }
            }
            return result;
        }

        Queue<ScoreSummary> ratingSummariesToUpdate;
        LazyDimension_Interpolator<Distribution> interpolator;
        ScoreSummarizer ratingSummarizer;
    }
}
