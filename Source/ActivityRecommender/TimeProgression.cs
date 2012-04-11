using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveLinearInterpolation;

// A TimeProgression is an IParticipation that simply calculates the time of Day
namespace ActivityRecommendation
{
    class TimeProgression : IProgression
    {
        public static TimeProgression AbsoluteTime
        {
            get
            {
                TimeProgression progression = new TimeProgression(new DateTime(), new TimeSpan());
                progression.Description = "Time";   // the amount of time that has passed since an arbitrary reference point
                return progression;
            }
        }
        public static TimeProgression DayCycle
        {
            get
            {
                TimeProgression progression = new TimeProgression(new DateTime(), new TimeSpan(24, 0, 0));
                progression.Description = "what time of day it is";
                return progression;
            }
        }
        public static TimeProgression WeekCycle
        {
            get
            {
                DateTime Sunday = new DateTime(2011, 1, 2);
                TimeProgression progression = new TimeProgression(Sunday, new TimeSpan(24 * 7, 0, 0));
                progression.Description = "when in the week it is";
                return progression;
            }
        }
        public TimeProgression(DateTime startingDate, TimeSpan period)
        {
            this.startDate = startingDate;
            this.cycleLength = period;
        }
        #region Functions for IProgression

        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            TimeSpan duration = when.Subtract(this.StartDate);
            double value;
            if (this.cycleLength.Ticks == 0)
            {
                value = duration.TotalSeconds; 
            }
            else
            {
                double numCycles = duration.TotalSeconds / this.cycleLength.TotalSeconds;
                value = numCycles - Math.Floor(numCycles);
            }
                
            Distribution currentValue = Distribution.MakeDistribution(value, 0, 1);
            return new ProgressionValue(when, currentValue);
        }
        public ProgressionValue GetCurrentValue(DateTime when)
        {
            return this.GetValueAt(when, false);
        }
        public IEnumerable<ProgressionValue> GetValuesAfter(int indexInclusive)
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
        public FloatRange EstimateOutputRange()
        {
            return new FloatRange(0, true, 1, true);
        }
        // returns a list of the natural values at which to display tick marks
        public IEnumerable<double> GetNaturalSubdivisions(double minSubdivision, double maxSubdivision)
        {
            double startValue = 0;
            double deltaValue = 0;
            if (this.cycleLength != null)
            {
                if (this.cycleLength.Days == 7)
                {
                    // split a week into days
                    deltaValue = (double)1 / (double)7;
                }
                if (this.cycleLength.Days == 1)
                {
                    // split a day into hours
                    deltaValue = (double)1 / (double)24;
                }
            }
            else
            {
                // convert seconds into time, and split into months
                // NOT DONE YET!
            }
            List<double> subdivisions = new List<double>();
            if (deltaValue != 0)
            {
                double subdivision;
                for (subdivision = startValue; subdivision <= maxSubdivision; subdivision += deltaValue)
                {
                    subdivisions.Add(subdivision);
                }
            }
            return subdivisions;
        }
        #endregion

        public string Description
        {
            get; 
            set;
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
