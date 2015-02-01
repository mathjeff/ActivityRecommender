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
            Distribution newDistribution = currentInput.Value.Plus(Distribution.MakeDistribution(0.5, 0.5, -2)); // remove (most of) the prediction uncertainty and just leave the observations
            // create some result objects
            Prediction prediction = new Prediction();
            prediction.Justification = this.Justification;
            Distribution currentValue = currentInput.Value;
            Distribution output = currentValue;
            double startingWeight = output.Weight;
            // correct the weight
            //double newWeight = Math.Sqrt(this.outputProgression.NumItems);
            double newWeight = newDistribution.Weight / Math.Pow(2, this.outputProgression.NumItems / 2);
            //double newWeight = currentValue.Weight;
            //if (currentValue.StdDev != 0)
            //    newWeight /= currentValue.StdDev;
            //double newWeight = this.inputProgression.NumItems / ((this.outputProgression.NumItems + 1) * (this.outputProgression.NumItems + 1));
            //double newWeight = startingWeight;
            prediction.Distribution = output.CopyAndReweightTo(newWeight);
            /*
            if (this.outputProgression.NumItems > 0)
                prediction.Distribution = output.CopyAndReweightBy(1 / Math.Sqrt(this.outputProgression.NumItems));
            else
                prediction.Distribution = output;
            */
            prediction.ApplicableDate = when;

            return prediction;
        }

        #endregion

        public string Justification { get; set; }
        private IProgression inputProgression;
        private IProgression outputProgression;
    }
}
