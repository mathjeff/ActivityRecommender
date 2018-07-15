using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveLinearInterpolation;

// An Activity is a Doable that represents potentially a class of activities, for example, Fun, Exercise or Playing Frisbee
namespace ActivityRecommendation
{
    public class Activity : Doable
    {
        #region Constructors

        static int nextID = 0;

        // public
        public Activity(string activityName, RatingSummarizer overallRatings_summarizer) : base(activityName, overallRatings_summarizer)
        {
        }
        #endregion

        #region Public Member Functions

        public override string ToString()
        {
            return "Activity " + base.Name;
        }

        public List<Doable> Children
        {
            get
            {
                return this.children;
            }
        }
        public override List<Doable> GetChildren()
        {
            return this.children;
        }
        public void AddChild(Doable child)
        {
            this.children.Add(child);
        }

        // returns a list containing this activity and all of its descendents
        public List<Doable> GetChildrenRecursive()
        {
            List<Doable> subCategories = new List<Doable>();
            subCategories.Add(this);
            int i = 0;
            for (i = 0; i < subCategories.Count; i++)
            {
                Doable item = subCategories[i];
                Activity activity = item as Activity;
                if (activity != null)
                {
                    foreach (Doable child in activity.Children)
                    {
                        if (!subCategories.Contains(child))
                        {
                            subCategories.Add(child);
                        }
                    }
                }
            }
            return subCategories;
        }
        // makes an ActivityDescriptor that describes this Activity
        public override ActivityDescriptor MakeDescriptor()
        {
            ActivityDescriptor descriptor = new ActivityDescriptor();
            descriptor.ActivityName = this.Name;
            descriptor.Choosable = this.Choosable; // we can require that the descriptor only match activities with the same Choosable as this, but it doesn't give much benefit
            return descriptor;
        }

        public void setChooseable(bool chooseable)
        {
            this.chooseable = chooseable;
        }

        protected override bool isChoosable()
        {
            if (this.children.Count > 0)
                return false;
            return this.chooseable;
        }
        

        #endregion

        #region Private Member Variables
        private bool chooseable = true;
        private List<Doable> children = new List<Doable>();

        #endregion

    }
}