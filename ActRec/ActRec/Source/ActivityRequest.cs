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
        public int NumOptionsRequested = 1;

        // The number of suggestions that must be accepted divided by the number of suggestions that will be done.
        // In most cases, this is 1, but an experiment requires consenting to multiple activities and then doing only one.
        public int NumAcceptancesPerParticipation = 1;

        // What type of feedback to compute, if any, while making this suggestion
        // The reason this gets put into a suggestion is because we want to be able to say "If you do this, this is what we expect to say about it"
        // The reason this gets put into an ActivityRequest is because it's based on approximately the same information as the suggestion, and
        // doing both at once helps ensure they're consistent (for example, not affected by any timing differences)
        public ParticipationFeedbackType FeedbackType = ParticipationFeedbackType.NONE;
    }

    public enum ActivityRequestOptimizationProperty
    {
        LONGTERM_HAPPINESS,
        PARTICIPATION_PROBABILITY,
        LONGTERM_EFFICIENCY
    }
}
