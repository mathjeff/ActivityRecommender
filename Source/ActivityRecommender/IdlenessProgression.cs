using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;
using AdaptiveLinearInterpolation;

// An IdlenessProgression simply calculates how long it has been since the user last did something
// It should be able to help model many types of things:
// Perhaps the user needs to wait a couple of days between exercising
// Perhaps a television program only comes out once a week and trying to watch it more often wouldn't work
namespace ActivityRecommendation
{
    class IdlenessProgression : IComparer<DateTime>, ICombiner<Participation>, IProgression
    {
        #region Constructor

        public IdlenessProgression(Activity owner)
        {
            this.Owner = owner;
            this.searchHelper = new StatList<DateTime, Participation>(this, this);
        }

        #endregion

        #region Public Member Functions

        public void AddParticipation(Participation newParticipation)
        {
            if (this.ShouldIncludeParticipation(newParticipation))
            {
                // Eventually, the StatList should be improved to allow intervals as keys
                this.searchHelper.Add(newParticipation.StartDate, newParticipation);
                this.searchHelper.Add(newParticipation.EndDate, newParticipation);
            }
        }
        public void RemoveParticipation(Participation participationToRemove)
        {
            // make sure that we had included it in the first place
            if (this.ShouldIncludeParticipation(participationToRemove))
            {
                this.searchHelper.Remove(participationToRemove.StartDate);
                this.searchHelper.Remove(participationToRemove.EndDate);
            }
        }

        // this function is a filter that tells whether this ParticipationProgression cares about this Participation
        public bool ShouldIncludeParticipation(Participation newParticipation)
        {
            return true;
        }

        #endregion

        #region Functions for IComparer<Participation>

        // Compare based on end date
        public int Compare(DateTime date1, DateTime date2)
        {
            return date1.CompareTo(date2);
        }

        #endregion

        #region Functions for ICombiner<Participation>

        public Participation Combine(Participation participation1, Participation participation2)
        {
            // we don't actually care about conglomerations of Participations so this function can return anything
            return this.Default();
        }

        public Participation Default()
        {
            return new Participation();
        }

        #endregion

        #region Functions for IProgression

        public Activity Owner { get; set; }
        // returns basically the fraction of the user's time that was spent performing that activity recently at that date
        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            // find the most recent Participation
            ListItemStats<DateTime, Participation> previousStats = this.searchHelper.FindPreviousItem(when, strictlyEarlier);
            // make sure that we have enough data
            if (previousStats == null)
            {
                return null;
                //ProgressionValue defaultValue = new ProgressionValue(when, new Distribution(), -1);
                //return defaultValue;
            }
            // figure out whether we're in the middle of a participation or not
            Participation previousParticipation = previousStats.Value;
            //int index = 2 * this.searchHelper.CountBeforeKey(when, !strictlyEarlier);
            if (previousParticipation.EndDate.CompareTo(when) > 0)
            {
                // we're in the middle of a participation, so the value is zero
                //index++;
                return new ProgressionValue(when, new Distribution(0, 0, 1));
            }
            // we're not in the middle of a participation, so the value is the amount of time since the last one ended
            TimeSpan duration = when.Subtract(previousParticipation.EndDate);
            Distribution currentValue = Distribution.MakeDistribution(duration.TotalSeconds, 0, 1);
            ProgressionValue result = new ProgressionValue(when, currentValue);
            return result;
        }

        public ProgressionValue GetCurrentValue(DateTime when)
        {
            return this.GetValueAt(when, false);
        }

        public IEnumerable<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            IEnumerable<ListItemStats<DateTime, Participation>> items = this.searchHelper.ItemsFromIndex(indexInclusive);
            //IEnumerable<ListItemStats<DateTime, Participation>> items = this.searchHelper.AllItems;
            List<ProgressionValue> results = new List<ProgressionValue>();
            //for (i = indexInclusive; i < items.Count;  i++)
            foreach (ListItemStats<DateTime, Participation> item in items)
            {
                DateTime when = item.Key;
                ProgressionValue value = this.GetValueAt(when, false);
                results.Add(value);
            }
            return results;
        }

        public string Description
        {
            get
            {
                return "How long it has been since you've done this";
            }
        }

        public int NumItems
        {
            get
            {
                return this.searchHelper.NumItems;
            }
        }

        public FloatRange EstimateOutputRange()
        {
            // This should be improved eventually
            // default to one day
            return new FloatRange(0, true, 24 * 60 * 60, true);
        }
        public IEnumerable<double> GetNaturalSubdivisions(double minSubdivision, double maxSubdivision)
        {
            throw new NotImplementedException();
        }


        #endregion


        private StatList<DateTime, Participation> searchHelper; // in the future the StatList may be improved to properly support intervals.
    }
}
