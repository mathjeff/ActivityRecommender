using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveInterpolation;

// A category is an Activity that represents potentially a class of activities, for example, Fun, Exercise or Playing Frisbee
namespace ActivityRecommendation
{
    public class Category : Activity
    {
        #region Constructors

        // public
        public Category(string activityName, ScoreSummarizer overallRatings_summarizer, ScoreSummarizer efficiencySummarizer) : base(activityName, overallRatings_summarizer, efficiencySummarizer)
        {
        }
        #endregion

        #region Public Member Functions

        public override string ToString()
        {
            return "Category: " + base.Name;
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

        // makes an ActivityDescriptor that describes this Activity
        public override ActivityDescriptor MakeDescriptor()
        {
            ActivityDescriptor descriptor = new ActivityDescriptor();
            descriptor.ActivityName = this.Name;
            descriptor.Suggestible = this.Suggestible;
            return descriptor;
        }

        public void setSuggestible(bool suggestible)
        {
            this.suggestible = suggestible;
        }

        protected override bool isSuggestible()
        {
            return this.suggestible;
        }
        

        #endregion

        #region Private Member Variables
        private bool suggestible = true;
        private List<Activity> children = new List<Activity>();

        #endregion

    }
}