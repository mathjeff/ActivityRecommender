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

    public class Activities_HappinessContributions
    {
        // The top N best activities
        public List<ActivityHappinessContribution> Best;
        // The worst N activities
        public List<ActivityHappinessContribution> Worst;
        // Whether there are any activities worse than the top N and better than the bottom N
        public bool ActivitiesRemain;
    }
}
