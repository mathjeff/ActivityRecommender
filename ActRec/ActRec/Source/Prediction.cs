﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    public class Prediction
    {
        public Prediction()
        {
        }
        public string Justification { get; set; }           // the primary reason that the PredictedScore is as high as it is
        public Distribution Distribution { get; set; }    // the expected rating that the user would assign to the activity
        //public Distribution ValueMinusAverage { get; set; } // the expected change in rating caused by doing this activity
        //public Distribution ParticipationProbability { get; set; }  // the probability that the user will take the suggestion
        //public Distribution SuggestionValue { get; set; }   // how important it is to suggest this activity
        public Category Activity { get; set; }              // the Activity that is being suggested
        public DateTime ApplicableDate { get; set; }                  // The date that this Prediction describes
        public DateTime CreationDate { get; set; }

        public Prediction Plus(Prediction other)
        {
            Prediction newPrediction = new Prediction();
            //newPrediction.Score = this.Score.Plus(other.Score);
            //newPrediction.ParticipationProbability = this.ParticipationProbability.Plus(other.ParticipationProbability);
            //newPrediction.SuggestionValue = this.ParticipationProbability.Plus(other.ParticipationProbability);
            newPrediction.Distribution = this.Distribution.Plus(other.Distribution);
            if (this.ApplicableDate.CompareTo(other.ApplicableDate) > 0)
                newPrediction.ApplicableDate = this.ApplicableDate;
            else
                newPrediction.ApplicableDate = other.ApplicableDate;

            return newPrediction;
        }
    }
}
