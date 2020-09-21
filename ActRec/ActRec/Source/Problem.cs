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

        public List<Activity> Children
        {
            get
            {
                return this.children;
            }
        }
        public override List<Activity> GetChildren()
        {
            return this.children;
        }
        public void AddChild(Activity child)
        {
            this.children.Add(child);
            child.InvalidateAncestorList();
        }

        public override List<Metric> InheritedMetrics
        {
            get
            {
                // An Activity that started with built-in metrics doesn't need to inherit more from elsewhere
                return new List<Metric>(0);
            }
        }

        protected override bool isSuggestible()
        {
            return false;
        }


        #endregion

        private List<Activity> children = new List<Activity>();

    }

}