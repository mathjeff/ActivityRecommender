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
        public int NumRecent_UserChosen_ExperimentSuggestions
        {
            get
            {
                return this.numRecent_userchosen_ExperimentSuggestions;
            }
            set
            {
                this.numRecent_userchosen_ExperimentSuggestions = value;
                this.Synchronized = false;
            }
        }
        // if the user started an experiment, then we require that they measure their participation via this metric
        public string DemandedMetricName {
            get
            {
                return this.demandedMetricName;
            }
            set
            {
                this.demandedMetricName = value;
                this.Synchronized = false;
            }
        }
        public bool Synchronized { get; set; }  // tells whether the information on disk matches the information in memory

        private DateTime? latestActionDate;
        private IEnumerable<ActivitySuggestion> suggestions = new List<ActivitySuggestion>();
        private int numRecent_userchosen_ExperimentSuggestions;
        private string demandedMetricName;
    }
}
