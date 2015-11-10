using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

namespace ActivityRecommendation
{
    interface IActivitySuggestionJustification
    {
        LayoutChoice_Set Visualize();
        String Summarize();
        ActivitySuggestion Suggestion { get; set; }
    }

    public abstract class ActivitySuggestionJustification : IActivitySuggestionJustification
    {
        public ActivitySuggestionJustification()
        {
        }
        public ActivitySuggestion Suggestion { get; set; }
        public abstract LayoutChoice_Set Visualize();
        public abstract String Summarize();
    }
}
