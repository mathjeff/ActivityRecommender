using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the RecentUserData class stores a small information about what the user has done recently.
namespace ActivityRecommendation
{
    class RecentUserData
    {
        public RecentUserData()
        {
        }
        public DateTime? LatestActionDate 
        {
            get
            {
                return this.latestActionDate;
            }
            set
            {
                this.latestActionDate = value;
                this.Synchronized = false;
            }
        }
        public ActivitySuggestion LatestSuggestion 
        {
            get
            {
                return this.latestSuggestion;
            }
            set
            {
                this.latestSuggestion = value;
                this.Synchronized = false;
            }
        }
        public bool Synchronized { get; set; }  // tells whether the information on disk matches the information in memory

        private DateTime? latestActionDate;
        private ActivitySuggestion latestSuggestion;

    }
}
