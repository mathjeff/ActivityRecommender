using AdaptiveInterpolation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class ParticipationInputs : LazyInputs
    {
        public ParticipationInputs(DateTime when, Activity participated, List<Activity> otherActivities)
        {
            this.when = when;
            this.participated = participated;
            this.activities = otherActivities;
        }
        public int GetNumCoordinates()
        {
            return this.activities.Count;
        }
        public double GetInput(int index)
        {
            Activity other = this.activities[index];
            ProgressionValue value = this.activities[index].ParticipationProgression.GetValueAt(this.when, false);
            double result = 0;
            if (value != null)
                result = value.Value.Mean;

            if (this.participated != null && this.participated.HasAncestor(other))
            {
                // if we are doing an activity now then we will have done it in a moment
                result *= 2;
            }

            return result;
        }
        public string GetDescription(int index)
        {
            return "Participation of " + this.activities[index] + " at " + when;
        }

        DateTime when;
        Activity participated;
        List<Activity> activities;
    }
}