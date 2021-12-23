using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the UserPreferences stores preferences for users
namespace ActivityRecommendation
{
    class CommonPreferences
    {
        // singleton
        static CommonPreferences instance = null;
        public static CommonPreferences Instance
        {
            get
            {
                if (instance == null)
                    instance = new CommonPreferences();
                return instance;
            }
        }

        // The amount of time it takes for the user's value to double
        public TimeSpan HalfLife
        {
            get
            {
                return TimeSpan.FromDays(730); // 2 years
            }
        }

        // When the user thinks about future efficiency, the user is more interested in sooner efficiency than in later efficiency. This is the halflife of the user's interest.
        public TimeSpan EfficiencyHalflife
        {
            get
            {
                return TimeSpan.FromDays(7);
            }
        }
    }
}
