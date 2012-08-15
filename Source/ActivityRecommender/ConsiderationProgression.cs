using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;
using AdaptiveLinearInterpolation;

// A ConsiderationProgression keeps track of times that the user was considering doing a particular Activity, and times that the user actually did it
// It is intended to estimate the probability that the user will do the given Activity
namespace ActivityRecommendation
{
    class ConsiderationProgression :IProgression, IComparer<DateTime>, ICombiner<Distribution>
    {
        public ConsiderationProgression(Activity newOwner)
        {
            this.searchHelper = new StatList<DateTime, Distribution>(this, this);
            this.valuesInDiscoveryOrder = new List<ProgressionValue>();
            this.owner = newOwner;
        }
        // tells whether we care about this participation at all
        public bool ShouldIncludeParticipation(Participation newParticipation)
        {
            // ignore any participation that we know for sure was not suggested by the engine
            if (newParticipation.Suggested != null)
            {
                if (newParticipation.Suggested.Value == false)
                    return false;
            }
            return true;
        }
        public void AddParticipation(Participation newParticipation)
        {
            if (this.ShouldIncludeParticipation(newParticipation))
            {
                Distribution distribution = newParticipation.TotalIntensity.CopyAndReweightTo(1);
                this.AddValue(newParticipation.StartDate, distribution);
            }
        }
        public void RemoveParticipation(Participation participationToRemove)
        {
            // make sure that we had included it in the first place
            if (this.ShouldIncludeParticipation(participationToRemove))
            {
                this.searchHelper.Remove(participationToRemove.StartDate);
                this.valuesInDiscoveryOrder.RemoveAt(this.valuesInDiscoveryOrder.Count - 1);
            }
        }
        public void AddSkip(ActivitySkip newSkip)
        {
            Distribution distribution = Distribution.MakeDistribution(0, 0, 1);
            this.AddValue(newSkip.Date, distribution);
        }
        public void AddValue(DateTime when, Distribution value)
        {
            this.valuesInDiscoveryOrder.Add(new ProgressionValue(when, value));
            this.searchHelper.Add(when, value);
        }
        #region Functions for IProgression

        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            ListItemStats<DateTime, Distribution> stats = this.searchHelper.FindPreviousItem(when, strictlyEarlier);
            if (stats == null)
                return null;
            //return new ProgressionValue(when, new Distribution(), -1);
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
            Distribution sum = this.searchHelper.CombineBetweenKeys(earlierDate, true, when, !strictlyEarlier);
            //int previousCount = this.searchHelper.CountBeforeKey(when, strictlyEarlier);
            //ProgressionValue result = new ProgressionValue(when, sum, this.searchHelper.CountBeforeKey(when, !strictlyEarlier));
            ProgressionValue result = new ProgressionValue(when, sum);
            return result;

        }
        public IEnumerable<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            List<ProgressionValue> results = this.valuesInDiscoveryOrder.GetRange(indexInclusive, this.NumItems - indexInclusive);
            return results;
        }
        public DateTime? LastDatePresent
        {
            get
            {
                ListItemStats<DateTime, Distribution> stats = this.searchHelper.GetLastValue();
                if (stats != null)
                    return stats.Key;
                return null;
            }
        }
        public DateTime? FirstDatePresent
        {
            get
            {
                ListItemStats<DateTime, Distribution> stats = this.searchHelper.GetFirstValue();
                if (stats != null)
                    return stats.Key;
                return null;
            }
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
        public FloatRange EstimateOutputRange()
        {
            return new FloatRange(0, true, 1, true);
        }
        public IEnumerable<double> GetNaturalSubdivisions(double minSubdivision, double maxSubdivision)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Functions for IComparer<DateTime>
        public int Compare(DateTime a, DateTime b)
        {
            this.numComparisons++;
            return a.CompareTo(b);
        }
        #endregion

        #region Functions for ICombiner<Distribution>

        public Distribution Combine(Distribution a, Distribution b)
        {
            this.numAdditions++;
            Distribution result = a.Plus(b);
            return result;
        }
        public Distribution Default()
        {
            return new Distribution();
        }

        #endregion

        private StatList<DateTime, Distribution> searchHelper;
        private List<ProgressionValue> valuesInDiscoveryOrder;
        private Activity owner;
        int numComparisons;
        int numAdditions;
    }
}
