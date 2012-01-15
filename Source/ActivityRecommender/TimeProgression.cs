using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// A TimeProgression is an IParticipation that simply calculates the time of Day
namespace ActivityRecommendation
{
    class TimeProgression : IProgression
    {
        public TimeProgression(DateTime startingDate, TimeSpan period)
        {
            this.startDate = startingDate;
            this.cycleLength = period;
        }
        #region Functions for IProgression

        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            TimeSpan duration = when.Subtract(this.StartDate);
            double numCycles = duration.TotalSeconds / this.cycleLength.TotalSeconds;
            double fraction = numCycles - Math.Floor(numCycles);
            Distribution currentValue = Distribution.MakeDistribution(fraction, 0, 1);
            return new ProgressionValue(when, currentValue, 0);
        }
        public ProgressionValue GetCurrentValue(DateTime when)
        {
            return this.GetValueAt(when, false);
        }
        public List<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            return new List<ProgressionValue>();
        }
        public int NumItems
        {
            get
            {
                return 0;
            }
        }
        #endregion

        public string Description
        {
            get
            {
                return "What time of day it is";
            }
        }
        // Owner must be defined but isn't used
        public Activity Owner
        {
            get
            {
                return null;
            }
        }
        public DateTime StartDate
        {
            get
            {
                return this.startDate;
            }
        }
        private DateTime startDate;
        private TimeSpan cycleLength;

    }
}
