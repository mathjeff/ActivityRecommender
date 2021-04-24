using AdaptiveInterpolation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class ConsiderationInputs : LazyInputs
    {
        public ConsiderationInputs(DateTime when, List<Activity> activities)
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
            ProgressionValue value = this.activities[index].ConsiderationProgression.GetValueAt(this.when, false);
            if (value != null)
                return value.Value.Mean;
            return 0;
        }
        DateTime when;
        List<Activity> activities;
    }
}