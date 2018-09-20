using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    public class ActivitySuggestion
    {
        public ActivitySuggestion(ActivityDescriptor descriptor)
        {
            this.ActivityDescriptor = descriptor;
        }
        public ActivityDescriptor ActivityDescriptor { get; set; }
        public DateTime StartDate { get; set; }                 // the date that we want the user to start the activity
        public DateTime? EndDate { get; set; }                   // the date that we want the user to stop the activity
        public DateTime? CreatedDate { get; set; }               // the date at which the suggestion was created
        public DateTime GuessCreationDate()
        {
            if (this.CreatedDate != null)
                return this.CreatedDate.Value;
            return this.StartDate;
        }
        public TimeSpan? Duration
        {
            get
            {
                if (this.EndDate != null)
                {
                    return this.EndDate.Value.Subtract(this.StartDate);
                }
                return null;
            }
        }
        public double? ParticipationProbability { get; set; }
        public double? PredictedScoreDividedByAverage { get; set; }
        //public Distribution PredictedScore { get; set; }
        public string Comment { get; set; }
        public ActivitySkip Skip { get; set; } // Describes when the user skipped this suggestion, if at all
        // number of suggestions for which we don't have another suggestion to follow up with
        public virtual int CountNumLeaves()
        {
            return 1; // self
        }
        public virtual int CountNumLevelsFromLeaf()
        {
            return 0; // self
        }
        public bool Skippable = true;
    }


    class ActivitySuggestionOrError
    {
        public ActivitySuggestionOrError(ActivitySuggestion suggestion) { this.Suggestion = suggestion; }
        public ActivitySuggestionOrError(String error) { this.Error = error; }

        public ActivitySuggestion Suggestion { get; set; }
        public String Error { get; set; }
    }

}
