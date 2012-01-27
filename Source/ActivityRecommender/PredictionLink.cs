using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The PredictionLink class is used to predict the value of a RatingProgression from the value of an IProgression
namespace ActivityRecommendation
{
    public class PredictionLink : IPredictionLink
    {
        // Constructor
        public PredictionLink(IProgression trainingInput, IProgression output)
        {
            this.trainingInputProgression = this.testingInputProgression = trainingInput;
            this.outputProgression = output;
            this.Initialize();            
        }
        public PredictionLink(IProgression trainingInput, IProgression testingInput, IProgression output)
        {
            this.trainingInputProgression = trainingInput;
            this.testingInputProgression = testingInput;
            this.outputProgression = output;
            this.Initialize();
        }
        private void Initialize()
        {
            this.predictionPlot = new ScatterPlot();
            this.Justification = this.trainingInputProgression.Description;
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
            
            List<ProgressionValue> values = this.outputProgression.GetValuesAfter(this.nextDatapointIndex);
            // iterate over all of the new ratings
            foreach (ProgressionValue outputValue in values)
            {
                DateTime when = outputValue.Date;
                ProgressionValue currentInput = this.trainingInputProgression.GetValueAt(when, true);
                if (currentInput.Index >= 0)
                {
                    //ProgressionValue currentOutput = this.predicteeProgression.GetValueAt(when, false);
                    Distribution outputDistribution = outputValue.Value;
                    // add the appropriate Datapoint to the ScatterPlot
                    this.predictionPlot.AddDatapoint(new Datapoint(currentInput.Value.Mean, outputDistribution.Mean, outputDistribution.Weight));
                    this.numDatapoints++;
                }
            }
            this.nextDatapointIndex += values.Count;
        }
        
        #region Functions for IPredictionLink
        
        // returns a distribution indicating the most likely values of the predictor, based on the current value of the predictee
        public Prediction Guess(DateTime when)
        {
            // now make the prediction
            ProgressionValue currentValue = this.testingInputProgression.GetValueAt(when, false);
            Distribution currentInput = currentValue.Value;
            Prediction result = this.Guess(currentInput);
            // set the date correctly
            result.Date = when;
            return result;
        }

        #endregion

        Prediction Guess(Distribution input)
        {
            // make sure the ScatterPlot is up-to-date
            this.Update();
            // get the current value to predict from
            double mean = input.Mean;
            // eventually, this will may improved by making use of the StdDev of the input
            Distribution rawEstimate = this.predictionPlot.Predict(input.Mean);
            rawEstimate = rawEstimate.CopyAndReweightTo(this.numDatapoints);
            // some more points with outputs of 0 and 1, to increase the uncertainty a little
            // The StdDev is increased more when there are fewer datapoints
            Distribution extraError = Distribution.MakeDistribution(0.5, 0.5, 2);
            Distribution predictionValue = rawEstimate.Plus(extraError);

            Prediction result = new Prediction();
            result.Distribution = predictionValue;
            result.Justification = this.Justification;

            return result;
        }
        /*public RatingProgression Predictee
        {
            get
            {
                return this.predicteeProgression;
            }
        }*/
        /*public IProgression Predictor
        {
            get
            {
                return this.predictorProgression;
            }
        }*/
        public bool InputWrapsAround // tells whether really large values should be considered to be close to really small values
        {
            set
            {
                this.predictionPlot.InputWrapsAround = value;
            }
        }
        public string Justification { get; set; }   // text that explains why the Prediction is as it is

        private IProgression trainingInputProgression;  // the Progression that supplies the input coordinate for the training data
        private IProgression testingInputProgression;   // the Progression that supplies the current input coordinate to predict from
        private IProgression outputProgression;   // the Progression that supplies the output coordinate for the training data
        private ScatterPlot predictionPlot;
        private int numDatapoints;
        private int nextDatapointIndex;
    }
}