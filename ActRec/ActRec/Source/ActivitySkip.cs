using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// An ActivitySkip embodies the statement "I don't want to do that activity right now"
namespace ActivityRecommendation
{
    public class ActivitySkip
    {
        public ActivitySkip(List<ActivityDescriptor> activityDescriptors, DateTime suggestionCreationDate, DateTime consideredSinceDate, DateTime creationDate, DateTime suggestionStartDate)
        {
            this.ActivityDescriptors = activityDescriptors;

            this.SuggestionCreationDate = suggestionCreationDate;
            this.CreationDate = creationDate;
            this.ConsideredSinceDate = consideredSinceDate;
            this.SuggestionStartDate = suggestionStartDate;
        }
        public List<ActivityDescriptor> ActivityDescriptors { get; set; }
        public DateTime CreationDate { get; set; }  // the date that the user skipped the suggestion
        public DateTime SuggestionCreationDate { get; set; }    // the date that the suggestion was given
        public DateTime ConsideredSinceDate { get; set; }    // the date at which the user started considering the idea
        public DateTime SuggestionStartDate { get; set; } // the date that the user is talking about when (s)he says (s)he doesn't want to do this activity on that date
        // how long the user spent considering this suggestion
        public TimeSpan ThinkingTime
        {
            get
            {
                // When determining how long the user spent thinking about what to do, we always want to round up to the next second
                // in case the user dismissed the suggestion during the same second in which the user saw it.
                // So we add 1 second to the recorded duration and use that.
                // TODO: should we also include the time that ActivityRecommender spent creating this skipped suggestion?
                return this.CreationDate.Subtract(this.ConsideredSinceDate).Add(TimeSpan.FromSeconds(1));
            }
        }
    }
}
