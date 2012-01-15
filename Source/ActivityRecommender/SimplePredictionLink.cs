using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// A SimplePredictionLink predicts the value of one IProgression from the value of another IProgression
// It simply assumes that the value of the output equals the value of the input
namespace ActivityRecommendation
{
    class SimplePredictionLink : IPredictionLink
    {
        public SimplePredictionLink(IProgression input, IProgression output, string justification)
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
            Distribution currentValue = currentInput.Value;
            // create some result objects
            Prediction prediction = new Prediction();
            prediction.Justification = this.Justification;
            Distribution output = currentValue;
            double startingWeight = output.Weight;
            // correct the weight
            double newWeight = Math.Sqrt(this.outputProgression.NumItems + 1);
            prediction.Distribution = output.CopyAndReweightTo(newWeight);

            return prediction;
        }

        #endregion

        public string Justification { get; set; }
        private IProgression inputProgression;
        private IProgression outputProgression;
    }
}
