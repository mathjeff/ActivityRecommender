using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

namespace ActivityRecommendation
{
    public class ActivitySuggestion_Explanation
    {
        public ActivitySuggestion Suggestion { get; set; }
        public double Score { get; set; } // normally an ActivitySuggestion only records its score divided by average
        public double SuggestionValue { get; set; } // not normally needed after MakeRecommendation completes
        public List<Justification> Reasons { get; set; }
    }
    public abstract class Justification
    {
        public string Label { get;set; }
        public Distribution Value { get; set; }
    }
    public class InterpolatorSuggestion_Justification : Justification
    {
        public InterpolatorSuggestion_Justification(Activity Activity, Distribution currentPrediction, Distribution averagePrediction, double[] coords)
        {
            this.Activity = Activity;
            this.Value = currentPrediction;
            this.PredictionWithoutCurrentCoordinates = averagePrediction;
            this.Coordinates = coords;
            this.Label = "Current interpolator result";
        }

        // The activity being used to predict from
        public Activity Activity { get; set; }
        // A prediction of the user's happiness based solely on the activity and not other factors that are time-dependent
        public Distribution PredictionWithoutCurrentCoordinates { get; set; }
        // The individual coordinates like how long ago the user did this activity and what time of day it is now
        public double[] Coordinates { get; set; }
    }
    public class LabeledDistributionJustification: Justification
    {
        public LabeledDistributionJustification(Distribution value, string Text)
        {
            this.Label = Text;
            this.Value = value;
        }
    }
    public class Composite_SuggestionJustification: Justification
    {
        public Composite_SuggestionJustification(Distribution value)
        {
            this.initialize(value, new List<Justification>());
        }
        public Composite_SuggestionJustification(Distribution value, Justification a, Justification b)
        {
            this.initialize(value, new List<Justification>() { a, b });
        }
        public Composite_SuggestionJustification(Distribution value, List<Justification> children)
        {
            this.initialize(value, children);
        }
        private void initialize(Distribution value, List<Justification> children)
        {
            this.Value = value;
            this.Children = children;
            this.Label = "Combination of " + this.Children.Count + " estimates:";
        }
        public List<Justification> Children;
    }
}
