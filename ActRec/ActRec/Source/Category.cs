﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveLinearInterpolation;

// A category is an Activity that represents potentially a class of activities, for example, Fun, Exercise or Playing Frisbee
namespace ActivityRecommendation
{
    public class Category : Activity
    {
        #region Constructors

        // public
        public Category(string activityName, RatingSummarizer overallRatings_summarizer) : base(activityName, overallRatings_summarizer)
        {
        }
        #endregion

        #region Public Member Functions

        public override string ToString()
        {
            return "Activity " + base.Name;
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
        private List<Activity> children = new List<Activity>();

        #endregion

    }
}