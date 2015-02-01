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
            this.Date = when;
            this.ActivityDescriptor = activityDescriptor;
        }
        public ActivityDescriptor ActivityDescriptor { get; set; }
        public DateTime Date { get; set; }  // the date that the user skipped the suggestion
        public DateTime? SuggestionDate { get; set; }    // the date that the suggestion was given
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
