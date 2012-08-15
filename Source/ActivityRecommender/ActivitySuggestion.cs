using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    class ActivitySuggestion
    {
        public ActivitySuggestion(ActivityDescriptor descriptor)
        {
            this.ActivityDescriptor = descriptor;
        }
        public ActivityDescriptor ActivityDescriptor { get; set; }
        public DateTime StartDate { get; set; }                 // the date that we want the user to start the activity
        public DateTime EndDate { get; set; }                   // the date that we want the user to stop the activity
        public TimeSpan Duration
        {
            get
            {
                return this.EndDate.Subtract(this.StartDate);
            }
        }
        public double ParticipationProbability { get; set; }
        public Distribution PredictedScore { get; set; }
    }
}
