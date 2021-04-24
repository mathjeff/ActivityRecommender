using AdaptiveInterpolation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class ActivityInList_Inputs : LazyInputs
    {
        public ActivityInList_Inputs(Activity activity, List<Activity> activities)
        {
            this.activity = activity;
            this.activities = activities;
        }
        public int GetNumCoordinates()
        {
            return this.activities.Count;
        }
        public double GetInput(int index)
        {
            if (this.activity.HasAncestor(this.activities[index]))
                return 1;
            return 0;
        }
        Activity activity;
        List<Activity> activities;
    }
}