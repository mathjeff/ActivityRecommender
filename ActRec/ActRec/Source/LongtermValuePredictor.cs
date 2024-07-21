using AdaptiveInterpolation;
using StatLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation
{
    class LongtermValuePredictor
    {
        public LongtermValuePredictor(ExponentialRatingSummarizer ratingSummarizer)
        {
            this.ratingSummarizer = ratingSummarizer;
            this.interpolator = new LazyDimension_Interpolator<Distribution>(new DistributionAdder());
            this.ratingSummariesToUpdate = new List<ScoreSummary>();
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
                this.ratingSummariesToUpdate.Add(summary);
            }
        }
        private bool ShouldIncludeSummary(ScoreSummary summary)
        {
            return summary.Item.Weight > 0;
        }
        public void UpdateMany(int numGroups)
        {
            if (this.ratingSummariesToUpdate.Count < 1)
                return; // nothing to update
            int numUpdates = numGroups * Math.Max(this.waves.Count, 1);

            // If we have enough time to update all points, then just do that
            int count = this.ratingSummariesToUpdate.Count;
            if (numUpdates >= count)
            {
                this.updateAll();
                return;
            }
            // make sure there's a wave at the end
            if (this.waves.Count < 1 || this.waves[this.waves.Count - 1] < this.ratingSummariesToUpdate.Count)
                this.waves.Add(this.ratingSummariesToUpdate.Count - 1);
            // advance the next few waves
            for (int i = 0; i < numUpdates; i++)
            {
                this.waveIndex++;
                if (this.waveIndex >= this.waves.Count)
                    this.waveIndex = 0;
                int waveLocation = this.waves[this.waveIndex];
                if (waveLocation >= 0)
                {
                    this.UpdateAtIndex(this.waves[this.waveIndex]);
                    this.waves[this.waveIndex]--;
                }
            }
            // delete any waves that moved too far
            for (int i = this.waves.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    // If the first wave fell off the end, remove it
                    if (this.waves[i] < 0)
                    {
                        this.waves.RemoveAt(i);
                    }
                }
                else
                {
                    // If a subsequent wave passed half of its previous wave, remove it
                    int thisAdvancement = this.ratingSummariesToUpdate.Count - this.waves[i];
                    int nextAdvancement = this.ratingSummariesToUpdate.Count - this.waves[i - 1];
                    if (thisAdvancement * 2 > nextAdvancement)
                        this.waves.RemoveAt(i);
                }
            }
        }
        private void updateAll()
        {
            for (int i = 0; i < this.ratingSummariesToUpdate.Count; i++)
            {
                this.UpdateAtIndex(i);
            }
            this.waves = new List<int>();
        }
        private void UpdateAtIndex(int index)
        {
            ScoreSummary summary = this.ratingSummariesToUpdate[index];
            this.interpolator.RemoveDatapoint(summary);
            summary.Update(this.ratingSummarizer);
            this.interpolator.AddDatapoint(summary);
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

        List<ScoreSummary> ratingSummariesToUpdate;
        LazyDimension_Interpolator<Distribution> interpolator;
        ExponentialRatingSummarizer ratingSummarizer;
        List<int> waves = new List<int>();
        int waveIndex;
    }
}
