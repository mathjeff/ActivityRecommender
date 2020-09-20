using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActivityRecommendation.Effectiveness;
using AdaptiveLinearInterpolation;

// A ToDo is a specific thing (like a task but less specific) that can be attempted several times, and can be completed once, at which point it's no longer relevant to attempt.
// If it's an important thing, then it can be considered a goal, like fixing a specific software bug.
// However, even something that isn't considered work, like watching a specific movie, can be considered a ToDo.
namespace ActivityRecommendation
{
    public class ToDo : Activity
    {
        #region Constructors

        // public
        public ToDo(string activityName, ScoreSummarizer overallRatings_summarizer, ScoreSummarizer overallEfficiency_summarizer) : base(activityName, overallRatings_summarizer, overallEfficiency_summarizer)
        {
            this.AddIntrinsicMetric(new TodoMetric(this));
        }
        #endregion

        #region Public Member Functions

        public override string ToString()
        {
            return "ToDo " + base.Name;
        }
        
        public bool IsCompleted()
        {
            return this.complete;
        }
        public bool WasCompletedSuccessfully()
        {
            return this.completedSuccessfully;
        }

        public override void AddParticipation(Participation newParticipation)
        {
            if (newParticipation.DismissedActivity)
                this.complete = true;
            if (newParticipation.CompletedMetric)
                this.completedSuccessfully = true;
            base.AddParticipation(newParticipation);
        }


        #endregion


        #region Protected methods

        protected override bool isSuggestible()
        {
            if (this.complete)
                return false;
            return true;
        }

        #endregion


        #region Private Member Variables
        private bool complete = false;
        private bool completedSuccessfully = false;
        #endregion

    }
}