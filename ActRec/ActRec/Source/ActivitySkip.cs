using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// An ActivitySkip embodies the statement "I don't want to do that activity right now"
namespace ActivityRecommendation
{
    public class ActivitySkip
    {
        public ActivitySkip()
        {
        }
        public ActivitySkip(DateTime when, ActivityDescriptor activityDescriptor)
        {
            this.CreationDate = when;
            this.ActivityDescriptor = activityDescriptor;
        }
        public ActivityDescriptor ActivityDescriptor { get; set; }
        public DateTime CreationDate { get; set; }  // the date that the user skipped the suggestion
        public DateTime? SuggestionCreationDate { get; set; }    // the date that the suggestion was given
        public DateTime? ApplicableDate { get; set; } // the date that the user is talking about when (s)he says (s)he doesn't want to do this activity on that date
        // returns the exact rating that was given to this Skip
        public AbsoluteRating RawRating { get; set; }
        // returns a Rating with as much information filled in as possible, based on the data in this Skip
        public Rating GetCompleteRating()
        {
            if (this.RawRating == null)
                return null;
            Rating completeRating = this.RawRating.MakeCopy();
            completeRating.FillInFromSkip(this);
            return completeRating;
        }
    }
}
