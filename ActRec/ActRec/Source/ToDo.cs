using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public ToDo(string activityName, RatingSummarizer overallRatings_summarizer) : base(activityName, overallRatings_summarizer)
        {
        }
        #endregion

        #region Public Member Functions

        public override string ToString()
        {
            return "ToDo " + base.Name;
        }
        
        // makes an ActivityDescriptor that describes this ToDo
        public override ActivityDescriptor MakeDescriptor()
        {
            ActivityDescriptor descriptor = new ActivityDescriptor();
            descriptor.ActivityName = this.Name;
            return descriptor;
        }

        public bool IsCompleted()
        {
            return this.complete;
        }

        public override void AddParticipation(Participation newParticipation)
        {
            if (newParticipation.CompletedTodo)
                this.complete = true;
            base.AddParticipation(newParticipation);
        }


        #endregion


        #region Protected methods

        protected override bool isChoosable()
        {
            if (this.complete)
                return false;
            return true;
        }

        #endregion


        #region Private Member Variables
        private bool complete = false;
        #endregion

    }
}