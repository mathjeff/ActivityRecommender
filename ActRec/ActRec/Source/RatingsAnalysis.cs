using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation
{
    // an ActivityRecommendationsAnalysis is a cache of information that contributes to making an ActivitySuggestion
    class RatingsAnalysis
    {
        public RatingsAnalysis(DateTime applicableDate)
        {
            this.ApplicableDate = applicableDate;
        }
        // the DateTime that this analysis applies to
        public DateTime ApplicableDate { get; set; }

        // the predicted rating for each activity
        public Dictionary<Activity, Prediction> ratings = new Dictionary<Activity, Prediction>();
        // estimated probability that the activity will be done if we suggest it
        public Dictionary<Activity, Prediction> participationProbabilities = new Dictionary<Activity, Prediction>();
        // the estimated future efficiency if the user does this
        public Dictionary<Activity, Prediction> futureEfficiencies = new Dictionary<Activity, Prediction>();
    }

    class UtilitiesAnalysis
    {
        public UtilitiesAnalysis(DateTime applicableDate, int numAcceptancesPerParticipation)
        {
            this.ApplicableDate = applicableDate;
            this.NumAcceptancesPerParticipation = numAcceptancesPerParticipation;
        }

        // the DateTime that this analysis applies to
        public DateTime ApplicableDate { get; set; }
        // How many suggestions must be accepted before the user does one
        public int NumAcceptancesPerParticipation { get; set; }

        // A prediction for the user's short-term happiness if this activity is suggested
        // Basically this is the rating, except slightly lower based on the participation probability and typical thinking time
        public Dictionary<Activity, double> utilities = new Dictionary<Activity, double>();
        // the importance of suggesting the activity
        public Dictionary<Activity, Prediction> suggestionValues = new Dictionary<Activity, Prediction>();
    }


}
