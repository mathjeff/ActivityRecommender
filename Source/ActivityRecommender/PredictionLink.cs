using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The PredictionLink class is used to predict the value of a RatingProgression from the value of an IProgression
namespace ActivityRecommendation
{
    public class PredictionLink
    {
        // Constructor
        public PredictionLink(IProgression predictor, RatingProgression predictee)
        {
            this.predictorProgression = predictor;
            this.predicteeProgression = predictee;
            this.predictionPlot = new ScatterPlot();
            this.numDatapoints = 0;
        }
        public void InitializeIncreasing()
        {
            this.predictionPlot.AddDatapoint(new Datapoint(0, 0, 1));
            this.predictionPlot.AddDatapoint(new Datapoint(0, 0, 1));
            this.predictionPlot.AddDatapoint(new Datapoint(0, 0, 1));
            this.predictionPlot.AddDatapoint(new Datapoint(0.25, 0.25, 1));
            this.predictionPlot.AddDatapoint(new Datapoint(0.5, 0.5, 1));
            this.predictionPlot.AddDatapoint(new Datapoint(0.75, 0.75, 1));
            this.predictionPlot.AddDatapoint(new Datapoint(1, 1, 1));
            this.predictionPlot.AddDatapoint(new Datapoint(1, 1, 1));
            this.predictionPlot.AddDatapoint(new Datapoint(1, 1, 1));
        }

        // updates the internal data based on the data from the predictor and predictee progressions
        public void Update()
        {
            // get a list of the ratings in the order that they were provided to the computer program, so we can find the new ones
            List<AbsoluteRating> ratings = this.predicteeProgression.GetRatingsInDiscoveryOrder();
            int i;
            int currentNumDatapoints = this.numDatapoints;
            // iterate over all of the new ratings
            for (i = currentNumDatapoints; i < ratings.Count; i++)
            {
                AbsoluteRating currentRating = ratings[i];
                ProgressionValue currentInput = this.predictorProgression.GetValueAt(currentRating.Date, true);
                if (currentInput.Index >= 0)
                {
                    // add the appropriate Datapoint to the ScatterPlot
                    this.predictionPlot.AddDatapoint(new Datapoint(currentInput.Value.Mean, currentRating.Score, currentRating.Weight));
                }
            }
            this.numDatapoints = ratings.Count;
        }
        // returns a distribution indicating the most likely values of the predictor, based on the current value of the predictee
        public Distribution Guess(DateTime when)
        {
            // now make the prediction
            ProgressionValue currentValue = this.predictorProgression.GetValueAt(when, true);
            return this.Guess(currentValue.Value);
        }
        // returns a distribution indicating the most likely values of the predictor, based on the current value of the predictee
        public Distribution Guess(Distribution input)
        {
            // make sure the ScatterPlot is up-to-date
            this.Update();
            // get the current value to predict from
            double mean = input.Mean;
            // eventually, this will may improved by making use of the StdDev of the input
            Distribution rawEstimate = this.predictionPlot.Predict(input.Mean);
            rawEstimate = rawEstimate.CopyAndReweightTo(this.numDatapoints);
            // add two more points with outputs of 0 and 1, to increase the uncertainty a little
            // The StdDev is increased more when there are fewer datapoints
            Distribution extraError = new Distribution(1, 1, 2);
            Distribution result = rawEstimate.Plus(extraError);
            return result;
        }
        public RatingProgression Predictee
        {
            get
            {
                return this.predicteeProgression;
            }
        }
        public IProgression Predictor
        {
            get
            {
                return this.predictorProgression;
            }
        }
        private IProgression predictorProgression;
        private RatingProgression predicteeProgression;
        private ScatterPlot predictionPlot;
        private int numDatapoints;
    }
}