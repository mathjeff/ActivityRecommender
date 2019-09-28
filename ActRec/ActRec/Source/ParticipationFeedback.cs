using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class ParticipationFeedback
    {
        public ParticipationFeedback(Activity activity, string summary, string details)
        {
            this.Activity = activity;
            this.Summary = summary;
            this.Details = details;
        }
        public Activity Activity { get; set; }

        public string Summary { get; set; }
        public string Details { get; set; }
    }
}
