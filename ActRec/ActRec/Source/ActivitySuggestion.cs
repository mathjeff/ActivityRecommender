using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    public class ActivitySuggestion
    {
        public ActivitySuggestion(ActivityDescriptor activityDescriptor)
        {
            this.ActivityDescriptor = activityDescriptor;
        }
        public ActivityDescriptor ActivityDescriptor;
        public DateTime StartDate { get; set; }                 // the date that we want the user to start the activity
        public DateTime? EndDate { get; set; }                   // the date that we want the user to stop the activity
        public DateTime CreatedDate { get; set; }               // the date at which the suggestion was created
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
        public bool WorseThanRootActivity { get; set; }
        // what we expect the engine's reaction to be if the user does this
        public string ExpectedReaction { get; set; }
        public ActivitySkip Skip { get; set; } // Describes when the user skipped this suggestion, if at all
        public int NumActivitiesConsidered { get; set; }
        public bool Skippable = true;
    }

    public class ActivitiesSuggestion
    {
        public ActivitiesSuggestion()
        {
            this.Children = new List<ActivitySuggestion>();
        }
        public ActivitiesSuggestion(List<ActivitySuggestion> children)
        {
            this.Children = children;
        }
        public ActivitiesSuggestion(ActivitySuggestion child)
        {
            this.Children = new List<ActivitySuggestion>() { child };
        }
        public bool CanMatch(ActivityDescriptor activityDescriptor)
        {
            foreach (ActivitySuggestion ours in this.Children)
            {
                if (ours.ActivityDescriptor.CanMatch(activityDescriptor))
                {
                    return true;
                }
            }
            return false;
        }
        public bool CanMatch(ActivitiesSuggestion other)
        {
            foreach (ActivitySuggestion child in this.Children)
            {
                if (other.CanMatch(child.ActivityDescriptor))
                    return true;
            }
            return false;
        }
        public DateTime StartDate
        {
            get
            {
                return this.Children.First().StartDate;
            }
        }
        public DateTime CreatedDate
        {
            get
            {
                return this.Children.First().CreatedDate;
            }
        }
        public List<ActivityDescriptor> ActivityDescriptors
        {
            get
            {
                List<ActivityDescriptor> result = new List<ActivityDescriptor>();
                foreach (ActivitySuggestion child in this.Children)
                {
                    result.Add(child.ActivityDescriptor);
                }
                return result;
            }
        }
        public bool Skippable
        {
            get
            {
                foreach (ActivitySuggestion suggestion in this.Children)
                {
                    if (!suggestion.Skippable)
                        return false;
                }
                return true;
            }
        }

        public List<ActivitySuggestion> Children;
    }


    class ActivitySuggestionOrError
    {
        public ActivitySuggestionOrError(ActivitySuggestion suggestion) { this.Suggestion = suggestion; }
        public ActivitySuggestionOrError(string error) { this.Error = error; }

        public ActivitySuggestion Suggestion { get; set; }
        public string Error { get; set; }
    }

}
