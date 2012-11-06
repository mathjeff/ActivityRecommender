using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// A Consideration represents the user thinking about doing something
namespace ActivityRecommendation
{
    public class Consideration
    {
        public Consideration(ActivityDescriptor activityDescriptor)
        {
            this.ActivityDescriptor = activityDescriptor;
        }
        public Consideration()
        {
        }
        public ActivityDescriptor ActivityDescriptor { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        ActivitySuggestion Prompt { get; set; }     // the suggestion that caused this Consideration
        public void FillInFromParticipation(Participation source)
        {
            if (this.ActivityDescriptor == null)
                this.ActivityDescriptor = source.ActivityDescriptor;
        }
    }
}
