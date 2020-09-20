using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    public class Problem : Activity
    {
        #region Constructors

        // public
        public Problem(string activityName, ScoreSummarizer overallRatings_summarizer, ScoreSummarizer overallEfficiency_summarizer) : base(activityName, overallRatings_summarizer, overallEfficiency_summarizer)
        {
            this.AddIntrinsicMetric(new ProblemMetric(this));
        }
        #endregion

        #region Public Member Functions

        public override string ToString()
        {
            return "Problem: " + base.Name;
        }

        #endregion


    }

}