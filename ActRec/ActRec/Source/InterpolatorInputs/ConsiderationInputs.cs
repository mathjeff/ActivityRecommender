using AdaptiveInterpolation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class ConsiderationInputs : LazyInputs
    {
        public ConsiderationInputs(DateTime when, Activity participated, Activity suggested, List<Activity> activities)
        {
            this.when = when;
            this.participated = participated;
            this.suggested = suggested;
            this.activities = activities;
        }
        public int GetNumCoordinates()
        {
            return this.activities.Count;
        }
        public double GetInput(int index)
        {
            Activity other = this.activities[index];
            ProgressionValue value = this.activities[index].ConsiderationProgression.GetValueAt(this.when, false);
            double result = 0;
            if (value != null)
                result = value.Value.Mean;
            if (this.participated != null && this.participated.HasAncestor(other))
            {
                // If we are doing an activity then we will have done it in a moment
                result *= 2;
            }
            if (this.suggested != null && this.suggested.HasAncestor(other))
            {
                // If we are suggesting an activity, then most likely we will have skipped it in a moment
                result /= 2;
            }

            return result;
        }
        public string GetDescription(int index)
        {
            return "Consideration of " + this.activities[index] + " at " + when;
        }

        DateTime when;
        Activity participated;
        Activity suggested;
        List<Activity> activities;
    }
}