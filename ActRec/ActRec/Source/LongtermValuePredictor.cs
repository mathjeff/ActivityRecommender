using AdaptiveLinearInterpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation
{
    class LongtermValuePredictor
    {
        public LongtermValuePredictor(HyperBox<Distribution> inputBoundary, ScoreSummarizer ratingSummarizer)
        {
            this.ratingSummarizer = ratingSummarizer;
            this.interpolator = new AdaptiveLinearInterpolator<Distribution>(inputBoundary, new DistributionAdder());
            this.ratingSummariesToUpdate = new Queue<ScoreSummary>();
        }
        public AdaptiveLinearInterpolation.Distribution Interpolate(double[] coordinates)
        {
            return this.interpolator.Interpolate(coordinates);
        }
        public HyperBox<Distribution> FindNeighborhoodCoordinates(double[] coordinates)
        {
            return this.interpolator.FindNeighborhoodCoordinates(coordinates);
        }
        // specifies that at <when> , the input coordinates were <coordinates> and that this is a datapoint we care to track
        public void AddDatapoint(DateTime when, double[] coordinates)
        {
            ScoreSummary summary = new ScoreSummary(when);
            summary.InputCoordinates = coordinates;
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
        public AdaptiveLinearInterpolation.Distribution GetAverage()
        {
            return this.interpolator.GetAverage();
        }
        Queue<ScoreSummary> ratingSummariesToUpdate;
        AdaptiveLinearInterpolator<Distribution> interpolator;
        ScoreSummarizer ratingSummarizer;
    }
}
