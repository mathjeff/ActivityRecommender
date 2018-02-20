using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// An ActivitySkip embodies the statement "I don't want to do that activity right now"
namespace ActivityRecommendation
{
    public class ActivitySkip
    {
        public ActivitySkip(ActivityDescriptor activityDescriptor, DateTime suggestionCreationDate, DateTime consideredSinceDate, DateTime creationDate, DateTime suggestionStartDate)
        {
            this.ActivityDescriptor = activityDescriptor;

            this.SuggestionCreationDate = suggestionCreationDate;
            this.CreationDate = creationDate;
            this.ConsideredSinceDate = consideredSinceDate;
            this.SuggestionStartDate = suggestionStartDate;
        }
        public ActivityDescriptor ActivityDescriptor { get; set; }
        public DateTime CreationDate { get; set; }  // the date that the user skipped the suggestion
        public DateTime SuggestionCreationDate { get; set; }    // the date that the suggestion was given
        public DateTime ConsideredSinceDate { get; set; }    // the date at which the user started considering the idea
        public DateTime SuggestionStartDate { get; set; } // the date that the user is talking about when (s)he says (s)he doesn't want to do this activity on that date
    }
}
