using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// A ConsiderationProgression keeps track of times that the user was considering doing a particular Activity, and times that the user actually did it
// It is intended to estimate the probability that the user will do the given Activity
namespace ActivityRecommendation
{
    class ConsiderationProgression :IProgression, IAdder<Distribution>
    {
        public ConsiderationProgression(Activity newOwner)
        {
            this.searchHelper = new StatList<DateTime, Distribution>(new DateComparer(), this);
            this.valuesInDiscoveryOrder = new List<ProgressionValue>();
            this.owner = newOwner;
        }
        public void AddParticipation(Participation newParticipation)
        {
            Distribution distribution = newParticipation.TotalIntensity.CopyAndReweightTo(1);
            this.AddValue(newParticipation.StartDate, distribution);
        }
        public void AddSkip(ActivitySkip newSkip)
        {
            Distribution distribution = Distribution.MakeDistribution(0, 0, 1);
            this.AddValue(newSkip.Date, distribution);
        }
        public void AddValue(DateTime when, Distribution value)
        {
            this.valuesInDiscoveryOrder.Add(new ProgressionValue(when, value, this.valuesInDiscoveryOrder.Count));
            this.searchHelper.Add(when, value);
        }
        #region Functions for IProgression

        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            ListItemStats<DateTime, Distribution> stats = this.searchHelper.FindPreviousItem(when, strictlyEarlier);
            if (stats == null)
                return new ProgressionValue(when, new Distribution(), -1);
            //Distribution distribution = stats.Value;
            //ProgressionValue progressionValue = new ProgressionValue(when, distribution, this.searchHelper.CountBeforeKey(when, !strictlyEarlier));
            //return progressionValue;



            // get some statistics
            DateTime latestDate = stats.Key;
            // compute how long ago that rating was given
            TimeSpan duration = when.Subtract(latestDate);
            // create another date that is twice as far in the past
            DateTime earlierDate = latestDate.Subtract(duration);
            // add up everything that occurred between the earlier day and now
            Distribution sum = this.searchHelper.SumBetweenKeys(earlierDate, true, when, !strictlyEarlier);
            int previousCount = this.searchHelper.CountBeforeKey(when, strictlyEarlier);
            ProgressionValue result = new ProgressionValue(when, sum, this.searchHelper.CountBeforeKey(when, !strictlyEarlier));
            return result;

        }
        public List<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            List<ProgressionValue> results = this.valuesInDiscoveryOrder.GetRange(indexInclusive, this.NumItems - indexInclusive);
            return results;
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
                return "How willing you have been recently to do this";
            }
        }

        #endregion

        #region Functions for IAdder<Distribution>

        public Distribution Sum(Distribution a, Distribution b)
        {
            Distribution result = a.Plus(b);
            return result;
        }
        public Distribution Zero()
        {
            return new Distribution();
        }

        #endregion

        private StatList<DateTime, Distribution> searchHelper;
        private List<ProgressionValue> valuesInDiscoveryOrder;
        private Activity owner;
    }
}
