using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    public class Prediction
    {
        public Prediction(Activity activity, Distribution distribution, DateTime when, string justification)
            : this(activity, distribution, when, new LabeledDistributionJustification(distribution, justification))
        {
        }
        public Prediction(Activity activity, Distribution distribution, DateTime when, Justification justification)
        {
            this.Activity = activity;
            this.Distribution = distribution;
            this.ApplicableDate = when;
            this.CreationDate = when;
            this.Justification = justification;
        }
        public Justification Justification { get; set; } // the primary reason that the PredictedScore is as high as it is
        public Distribution Distribution { get; set; }    // the expected rating that the user would assign to the activity
        public Activity Activity { get; set; }              // the Activity that is being suggested
        public DateTime ApplicableDate { get; set; }                  // The date that this Prediction describes
        public DateTime CreationDate { get; set; }

        public Prediction Plus(Prediction other)
        {
            Prediction newPrediction = new Prediction(this.Activity, this.Distribution, this.CreationDate, this.Justification);
            newPrediction.Distribution = this.Distribution.Plus(other.Distribution);
            if (this.ApplicableDate.CompareTo(other.ApplicableDate) > 0)
                newPrediction.ApplicableDate = this.ApplicableDate;
            else
                newPrediction.ApplicableDate = other.ApplicableDate;
            newPrediction.Justification = new Composite_SuggestionJustification(newPrediction.Distribution, this.Justification, other.Justification);

            return newPrediction;
        }
    }
}
