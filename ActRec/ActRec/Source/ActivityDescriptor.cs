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
        }

        public ActivityDescriptor(string ActivityName)
        {
            this.ActivityName = ActivityName;
        }

        #endregion

        #region Public Member Functions
        
        public string ActivityName { get; set; }
        public bool? Choosable = null;
        public bool PreferMorePopular = false;   // tells whether this descriptor wants to match the Activity with the best rating
        public bool RequiresPerfectMatch = true;
        public bool CanMatch(ActivityDescriptor other)
        {
            if (other == null)
                return false;
            if (this.ActivityName != null && other.ActivityName != null)
            {
                if (!this.ActivityName.Equals(other.ActivityName))
                    return false;
            }
            if (this.Choosable != null && other.Choosable != null)
            {
                if (this.Choosable.Value != other.Choosable.Value)
                    return false;
            }
            return true;
        }
        public bool Matches(Activity activity)
        {
            return this.CanMatch(activity.MakeDescriptor());
        }
        #endregion

    }
}
