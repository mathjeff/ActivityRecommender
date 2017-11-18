using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The ActivityDescriptor class is a description of an Activity
namespace ActivityRecommendation
{
    public class ActivityDescriptor
    {
        #region Constructor

        public ActivityDescriptor()
        {
            this.Choosable = null;
            this.RequiresPerfectMatch = true;
        }

        #endregion

        #region Public Member Functions
        
        public string ActivityName
        {
            get
            {
                return this.activityName;
            }
            set
            {
                // now update the desired data
                this.activityName = value;
            }
        }
        public bool? Choosable { get; set; }
        public bool PreferHigherProbability { get; set; }   // tells whether this descriptor wants to match the Activity with the best rating
        public bool RequiresPerfectMatch { get; set; }
        public bool CanMatch(ActivityDescriptor other)
        {
            if (other == null)
                return false;
            if (this.ActivityName != null && other.activityName != null)
            {
                if (!this.activityName.Equals(other.activityName))
                    return false;
            }
            if (this.Choosable != null && other.Choosable != null)
            {
                if (this.Choosable.Value != other.Choosable.Value)
                    return false;
            }
            return true;
        }
        #endregion

        #region Private Member Variables

        private string activityName;

        #endregion

    }
}
