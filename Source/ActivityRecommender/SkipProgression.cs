using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;
using AdaptiveLinearInterpolation;

// A SkipProgression keeps track of how long it has been since the user said that they did not want to do a particular activity
namespace ActivityRecommendation
{
    class SkipProgression : IProgression, ICombiner<ActivitySkip>
    {
        public SkipProgression(Activity newOwner)
        {
            this.searchHelper = new StatList<DateTime, ActivitySkip>(new DateComparer(), this);
            this.valuesInDiscoveryOrder = new List<ActivitySkip>();
            this.owner = newOwner;
        }
        public void AddSkip(ActivitySkip newSkip)
        {
            // keep track of the skips in the order they were discovered
            this.valuesInDiscoveryOrder.Add(newSkip);

            // keep track of the skips in chronological order
            this.searchHelper.Add(newSkip.Date, newSkip);
        }
        #region Functions for IProgression

        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            ListItemStats<DateTime, ActivitySkip> stats = this.searchHelper.FindPreviousItem(when, strictlyEarlier);
            if (stats == null)
                return null;
            //return new ProgressionValue(when, new Distribution(), -1);
            ActivitySkip skip = stats.Value;
            Distribution distribution = Distribution.MakeDistribution(when.Subtract(skip.Date).TotalSeconds, 0, 1);
            //ProgressionValue progressionValue = new ProgressionValue(when, distribution, this.searchHelper.CountBeforeKey(when, !strictlyEarlier));
            ProgressionValue progressionValue = new ProgressionValue(when, distribution);
            return progressionValue;
        }
        public IEnumerable<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            throw new NotImplementedException();
        }
        public int NumItems
        {
            get
            {
                return this.valuesInDiscoveryOrder.Count;
            }
        }
        public Activity Owner
        {
            get
            {
                return this.owner;
            }
        }
        public string Description
        {
            get
            {
                return "How long it has been since you last skipped this Activity";
            }
        }

        public FloatRange EstimateOutputRange()
        {
            // default to 1 month
            return new FloatRange(0, true, 24 * 60 * 60, true);
        }
        public IEnumerable<double> GetNaturalSubdivisions(double minSubdivision, double maxSubdivision)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Functions for ICombiner<Distribution>

        public ActivitySkip Combine(ActivitySkip a, ActivitySkip b)
        {
            return null;
        }
        public ActivitySkip Default()
        {
            return new ActivitySkip();
        }

        #endregion

        private StatList<DateTime, ActivitySkip> searchHelper;
        private List<ActivitySkip> valuesInDiscoveryOrder;
        private Activity owner;
    }
}
