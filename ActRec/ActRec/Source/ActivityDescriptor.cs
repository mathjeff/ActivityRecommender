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
        public bool? Suggestible = null; // Whether this activity is allowed to be suggested
        public bool PreferMorePopular = false;   // tells whether this descriptor wants to match the Activity with the best rating
        public bool RequiresPerfectMatch = true;
        public bool PreferAvoidCompletedToDos = false; // if true, we prefer to avoid completed ToDos
        public bool CanMatch(ActivityDescriptor other)
        {
            if (other == null)
                return false;
            if (this.ActivityName != null && other.ActivityName != null)
            {
                if (!this.ActivityName.Equals(other.ActivityName))
                    return false;
            }
            if (this.Suggestible != null && other.Suggestible != null)
            {
                if (this.Suggestible.Value != other.Suggestible.Value)
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
