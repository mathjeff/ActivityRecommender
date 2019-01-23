using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the RecentUserData class stores a small information about what the user has done recently.
namespace ActivityRecommendation
{
    public class RecentUserData
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
        public IEnumerable<ActivitySuggestion> Suggestions
        {
            get
            {
                return this.suggestions;
            }
            set
            {
                this.suggestions = value;
                this.Synchronized = false;
            }
        }
        public bool Synchronized { get; set; }  // tells whether the information on disk matches the information in memory

        private DateTime? latestActionDate;
        private IEnumerable<ActivitySuggestion> suggestions = new LinkedList<ActivitySuggestion>();

    }
}
