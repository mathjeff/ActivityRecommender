using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the UserPreferences stores preferences for users
namespace ActivityRecommendation
{
    class UserPreferences
    {
        // singleton
        static UserPreferences defaultPreferences = null;
        public static UserPreferences DefaultPreferences
        {
            get
            {
                if (defaultPreferences == null)
                    defaultPreferences = new UserPreferences();
                return new UserPreferences();
            }
        }

        // The amount of time it takes for the user's value to double
        public TimeSpan HalfLife
        {
            get
            {
                return new TimeSpan(730, 0, 0, 0);  // 2 years
            }
        }
    }
}
