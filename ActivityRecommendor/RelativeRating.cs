using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the RelativeRating class represents the statement that the rating of one Activity at one time is better than the rating of another Activity at another time
namespace ActivityRecommendation
{
    class RelativeRating
    {
        #region Public Member Functions

        public void SetBetterActivityDescriptor(ActivityDescriptor descriptor)
        {
            this.betterDescriptor = descriptor;
        }
        public ActivityDescriptor GetBetterActivityDescriptor()
        {
            return this.betterDescriptor;
        }
        public void SetBetterDate(DateTime when)
        {
            this.betterDate = when;
        }
        public DateTime GetBetterDate()
        {
            return this.betterDate;
        }

        public void SetWorseActivityDescriptor(ActivityDescriptor descriptor)
        {
            this.worseDescriptor = descriptor;
        }
        public ActivityDescriptor GetWorseActivityDescriptor()
        {
            return this.worseDescriptor;
        }
        public void SetWorseDate(DateTime when)
        {
            this.worseDate = when;
        }
        public DateTime GetWorseDate()
        {
            return this.worseDate;
        }

        #endregion

        #region Private Member Variables

        private ActivityDescriptor betterDescriptor;
        private DateTime betterDate;
        private ActivityDescriptor worseDescriptor;
        private DateTime worseDate;

        #endregion

    }
}