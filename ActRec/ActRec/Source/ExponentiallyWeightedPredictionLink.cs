using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// A SimplePredictionLink predicts the value of one IProgression from the value of another IProgression
// It simply assumes that the value of the output equals the value of the input
namespace ActivityRecommendation
{
    class ExponentiallyWeightedPredictionLink : IPredictionLink
    {
        public ExponentiallyWeightedPredictionLink(IProgression input, IProgression output, string justification)
        {
            this.inputProgression = input;
            this.outputProgression = output;
            this.Justification = justification;
        }

        #region Functions for IPredictionLink

        public Prediction Guess(DateTime when)
        {
            // get the current inputs
            ProgressionValue currentInput = this.inputProgression.GetValueAt(when, false);
            Distribution newDistribution = currentInput.Value;
            // create some result objects
            Prediction prediction = new Prediction(null, newDistribution, when, this.Justification);
            Distribution currentValue = currentInput.Value;
            Distribution output = currentValue;
            double startingWeight = output.Weight;
            // correct the weight
            double newWeight = Math.Min(newDistribution.Weight, 10) / Math.Pow(2, this.outputProgression.NumItems / 2);
            prediction.Distribution = output.CopyAndReweightTo(newWeight);
            prediction.ApplicableDate = when;

            return prediction;
        }

        #endregion

        private string Justification { get; set; }
        private IProgression inputProgression;
        private IProgression outputProgression;
    }
}
