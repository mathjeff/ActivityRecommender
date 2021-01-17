using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// An ActivityRequest embodies the statement "I want to do an activity in the category Fun" (or some other category)
// The fact that the user requested a suggestion from that category means that that category gets a better rating
namespace ActivityRecommendation
{
    public class ActivityRequest
    {
        public ActivityRequest()
        {
            this.Date = DateTime.Now;
        }
        public ActivityRequest(DateTime when)
        {
            this.Date = when;
        }
        public ActivityRequest(ActivityDescriptor fromCategory, ActivityDescriptor activityToBeat, DateTime when)
        {
            this.FromCategory = fromCategory;
            this.ActivityToBeat = activityToBeat;
            this.Date = when;
        }
        public ActivityRequest(Activity fromCategory, Activity activityToBeat, DateTime when, ActivityRequestOptimizationProperty optimize = ActivityRequestOptimizationProperty.LONGTERM_HAPPINESS)
        {
            if (fromCategory != null)
                this.FromCategory = fromCategory.MakeDescriptor();
            if (activityToBeat != null)
                this.ActivityToBeat = activityToBeat.MakeDescriptor();
            this.Date = when;
            this.Optimize = optimize;
        }
        public ActivityRequestOptimizationProperty Optimize = ActivityRequestOptimizationProperty.LONGTERM_HAPPINESS;

        public ActivityDescriptor FromCategory { get; set; }
        public List<Activity> LeafActivitiesToConsider { get; set; }
        public ActivityDescriptor ActivityToBeat { get; set; }
        public DateTime Date { get; set; }
        public Rating RawRawing { get; set; }
        public Rating UserPredictedRating { get; set; }
        public TimeSpan? RequestedProcessingTime { get; set; }

        // The number of suggestions that must be accepted divided by the number of suggestions that will be done.
        // In most cases, this is 1, but an experiment requires consenting to multiple activities and then doing only one.
        public int NumAcceptancesPerParticipation = 1;
    }

    public enum ActivityRequestOptimizationProperty
    {
        LONGTERM_HAPPINESS,
        PARTICIPATION_PROBABILITY,
        LONGTERM_EFFICIENCY
    }
}
