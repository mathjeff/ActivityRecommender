using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation
{
    class ActivityAverageScoreComparer : IComparer<Activity>
    {
        public ActivityAverageScoreComparer()
        {
        }
        public int Compare(Activity a, Activity b)
        {
            if (a.Ratings.Weight <= 0)
            {
                if (b.Ratings.Weight <= 0)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (b.Ratings.Weight <= 0)
                    return 1;
            }

            return a.Ratings.Mean.CompareTo(b.Ratings.Mean);
        }
    }
}
