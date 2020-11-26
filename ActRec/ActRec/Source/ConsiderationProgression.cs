using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;
using AdaptiveLinearInterpolation;

// A ConsiderationProgression keeps track of times that the user was considering doing a particular Doable, and times that the user actually did it
// It is intended to estimate the probability that the user will do the given Doable
namespace ActivityRecommendation
{
    public class ConsiderationProgression :IProgression, IComparer<DateTime>, ICombiner<WillingnessSummary>
    {
        public ConsiderationProgression(Activity newOwner)
        {
            this.searchHelper = new StatList<DateTime, WillingnessSummary>(this, this);
            this.owner = newOwner;
        }
        // tells whether we care about this participation at all
        public bool ShouldIncludeParticipation(Participation newParticipation)
        {
            return true;
        }
        public void AddParticipation(Participation newParticipation)
        {
            if (this.ShouldIncludeParticipation(newParticipation))
            {
                WillingnessSummary willingness;
                if (newParticipation.Suggested)
                    willingness = WillingnessSummary.Prompted;
                else
                    willingness = WillingnessSummary.Unprompted;
                this.AddValue(newParticipation.StartDate, willingness);
            }
        }
        public void RemoveParticipation(Participation participationToRemove)
        {
            // make sure that we had included it in the first place
            if (this.ShouldIncludeParticipation(participationToRemove))
            {
                this.searchHelper.Remove(participationToRemove.StartDate);
            }
        }
        public void AddSkip(ActivitySkip newSkip)
        {
            WillingnessSummary willingness = WillingnessSummary.Skipped;
            this.AddValue(newSkip.SuggestionStartDate, willingness);
        }
        public void AddValue(DateTime when, WillingnessSummary value)
        {
            this.searchHelper.Add(when, value);
        }
        #region Functions for IProgression

        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            ListItemStats<DateTime, WillingnessSummary> stats = this.searchHelper.FindPreviousItem(when, strictlyEarlier);
            if (stats == null)
                return null;

            // get some statistics
            DateTime latestDate = stats.Key;
            // compute how long ago that rating was given
            TimeSpan duration = when.Subtract(latestDate);
            // create another date that is twice as far in the past
            DateTime earlierDate = latestDate.Subtract(duration);
            // add up everything that occurred between the earlier day and now
            WillingnessSummary sum = this.searchHelper.CombineBetweenKeys(earlierDate, true, when, !strictlyEarlier);
            double numParticipations = sum.NumUnpromptedParticipations + sum.NumPromptedParticipations;
            double numSkips = sum.NumSkips;
            double mean = numParticipations / (numParticipations + numSkips);
            double weight = numParticipations + numSkips;
            Distribution distribution = Distribution.MakeDistribution(mean, 0, weight);
            ProgressionValue result = new ProgressionValue(when, distribution);
            return result;

        }
        public IEnumerable<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<ListItemStats<DateTime, WillingnessSummary>> AllItems
        {
            get
            {
                return this.searchHelper.AllItems;
            }
        }
        public DateTime? LastDatePresent
        {
            get
            {
                ListItemStats<DateTime, WillingnessSummary> stats = this.searchHelper.GetLastValue();
                if (stats != null)
                    return stats.Key;
                return null;
            }
        }
        public DateTime? FirstDatePresent
        {
            get
            {
                ListItemStats<DateTime, WillingnessSummary> stats = this.searchHelper.GetFirstValue();
                if (stats != null)
                    return stats.Key;
                return null;
            }
        }
        public int NumItems
        {
            get
            {
                return this.searchHelper.NumItems;
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

        public WillingnessSummary Combine(WillingnessSummary a, WillingnessSummary b)
        {
            this.numAdditions++;
            WillingnessSummary result = a.Plus(b);
            return result;
        }
        public WillingnessSummary Default()
        {
            return WillingnessSummary.Empty;
        }

        #endregion

        private StatList<DateTime, WillingnessSummary> searchHelper;
        private Activity owner;
        int numComparisons;
        int numAdditions;
    }
}
