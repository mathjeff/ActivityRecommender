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
        
        public string NamePrefix
        {
            get
            {
                if (this.ActivityName != null)
                {
                    return this.ActivityName;
                }
                else
                {
                    return this.namePrefix;
                }
            }
            set
            {
                this.namePrefix = value;
            }
        }
        public string ActivityName
        {
            get
            {
                return this.activityName;
            }
            set
            {
                if (this.activity != null)
                {
                    // if the activity name disagrees with the new activity...
                    if (!string.Equals(this.activity.Name, value))
                    {
                        // ... then discard the older data
                        this.activity = null;
                    }
                }
                // now update the desired data
                this.activityName = value;
            }
        }
        public Activity Activity
        {
            get
            {
                return this.activity;
            }
            set
            {
                if (value != null)
                {
                    // if the activity name disagrees with the new activity...
                    if (!string.Equals(this.activityName, value.Name))
                    {
                        // ... then discard the older data
                        this.activityName = null;
                    }
                }
                // now update the desired data
                this.activity = value;
            }
        }
        public bool? Choosable { get; set; }
        public bool PreferHigherProbability { get; set; }   // tells whether this descriptor wants to match the Activity with the best rating
        public bool RequiresPerfectMatch { get; set; }
        #endregion

        #region Private Member Variables

        private string activityName;
        private Activity activity;
        private string namePrefix;

        #endregion

    }
}
