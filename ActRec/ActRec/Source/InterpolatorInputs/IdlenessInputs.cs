using AdaptiveInterpolation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class IdlenessInputs : LazyInputs
    {
        public IdlenessInputs(DateTime when, List<Activity> activities)
        {
            this.when = when;
            this.activities = activities;
        }
        public int GetNumCoordinates()
        {
            return this.activities.Count;
        }
        public double GetInput(int index)
        {
            ProgressionValue value = this.activities[index].IdlenessProgression.GetValueAt(this.when, false);
            if (value != null)
                return value.Value.Mean;
            return 0;
        }
        public string GetDescription(int index)
        {
            return "Idleness of " + this.activities[index] + " at " + when;
        }

        DateTime when;
        List<Activity> activities;
    }
}
