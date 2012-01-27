using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// A SkipProgression keeps track of how long it has been since the user said that they did not want to do a particular activity
namespace ActivityRecommendation
{
    class SkipProgression : IProgression, IAdder<ActivitySkip>
    {
        public SkipProgression(Activity newOwner)
        {
            this.searchHelper = new StatList<DateTime, ActivitySkip>(new DateComparer(), this);
            this.valuesInDiscoveryOrder = new List<ActivitySkip>();
            this.owner = newOwner;
        }
        public void AddSkip(ActivitySkip newSkip)
        {
            this.valuesInDiscoveryOrder.Add(newSkip);

            this.searchHelper.Add(newSkip.Date, newSkip);
        }
        #region Functions for IProgression

        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            ListItemStats<DateTime, ActivitySkip> stats = this.searchHelper.FindPreviousItem(when, strictlyEarlier);
            if (stats == null)
                return new ProgressionValue(when, new Distribution(), -1);
            ActivitySkip skip = stats.Value;
            Distribution distribution = Distribution.MakeDistribution(when.Subtract(skip.Date).TotalSeconds, 0, 1);
            ProgressionValue progressionValue = new ProgressionValue(when, distribution, this.searchHelper.CountBeforeKey(when, !strictlyEarlier));
            return progressionValue;
        }
        public List<ProgressionValue> GetValuesAfter(int indexInclusive)
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

        #endregion

        #region Functions for IAdder<Distribution>

        public ActivitySkip Sum(ActivitySkip a, ActivitySkip b)
        {
            return null;
        }
        public ActivitySkip Zero()
        {
            return new ActivitySkip();
        }

        #endregion

        private StatList<DateTime, ActivitySkip> searchHelper;
        private List<ActivitySkip> valuesInDiscoveryOrder;
        private Activity owner;
    }
}
