using AdaptiveInterpolation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class IdlenessInputs : LazyInputs
    {
        public IdlenessInputs(DateTime when, Activity considered, List<Activity> allActivities)
        {
            this.when = when;
            this.allActivities = allActivities;
            this.considered = considered;
        }
        public int GetNumCoordinates()
        {
            return this.allActivities.Count;
        }
        public double GetInput(int index)
        {
            Activity otherActivity = this.allActivities[index];

            // Once we do an activity, its idle duration is 0
            if (this.considered != null && this.considered.HasAncestor(otherActivity))
                return 0;

            ProgressionValue value = this.allActivities[index].IdlenessProgression.GetValueAt(this.when, false);
            if (value != null)
                return value.Value.Mean;
            return 0;
        }
        public string GetDescription(int index)
        {
            return "Idleness of " + this.allActivities[index] + " at " + when;
        }

        DateTime when;
        Activity considered;
        List<Activity> allActivities;
    }
}
