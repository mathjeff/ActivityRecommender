using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    // an ActivityHappinessContribution keeps track of the total happiness contributed by an activity (potentially during certain circumstances)
    public class ActivityHappinessContribution
    {
        public ActivityHappinessContribution()
        {
        }
        // the activity being referred to
        public Activity Activity { get; set; }
        // the total number of seconds of happiness added by the given activity
        public double TotalHappinessIncreaseInSeconds { get; set; }
    }

    public class ActivityHappinessContributionComparer : IComparer<ActivityHappinessContribution>
    {
        public int Compare(ActivityHappinessContribution a, ActivityHappinessContribution b)
        {
            return a.TotalHappinessIncreaseInSeconds.CompareTo(b.TotalHappinessIncreaseInSeconds);
        }
    }
}
