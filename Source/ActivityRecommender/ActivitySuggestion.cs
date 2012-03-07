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
    }
}
