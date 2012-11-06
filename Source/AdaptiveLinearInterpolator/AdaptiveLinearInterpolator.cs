using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the current version of the AdaptiveLinearInterpolator does a 0th order approximation of the datapoints near the coordinates in question
// It intelligently determines a reasonable neighborhood of points nearby, and so if any dimensions don't help in certain areas, it won't worry about them
// Improving it to a 1st order approximation would be a big improvement because it allows the error to drop much faster
namespace AdaptiveLinearInterpolation
{
    public class AdaptiveLinearInterpolator<ScoreType>
    {
        public AdaptiveLinearInterpolator(HyperBox<ScoreType> inputBoundary, INumerifier<ScoreType> scoreCombiner)
        {
            this.root = new SmartInterpolationBox<ScoreType>(inputBoundary, scoreCombiner);
        }
        public void AddDatapoint(IDatapoint<ScoreType> newDatapoint)
        {
            if (newDatapoint.NumInputDimensions != this.root.NumDimensions)
                throw new ArgumentException("the number of dimensions is incorrect");
            this.root.AddDatapoint(newDatapoint);
        }
        public void RemoveDatapoint(IDatapoint<ScoreType> datapoint)
        {
            this.root.RemoveDatapoint(datapoint);
        }
        public Distribution Interpolate(double[] coordinates)
        {
            if (coordinates.Length != this.root.NumDimensions)
                throw new ArgumentException("the number of dimensions is incorrect");
            // figure out how much room there was to start with
            //double maxInputArea = this.root.GetInputArea();
            double maxInputSpread = this.root.GetInputVariation();
            double maxOutputSpread = this.root.GetScoreSpread();
            SmartInterpolationBox<ScoreType> currentBox = this.root;
            SmartInterpolationBox<ScoreType> nextBox;
            Distribution result = new Distribution();
            //double inputFraction;
            //double outputFraction;
            //double datapointFraction;
            while (true)
            {
                // decay towards the next component
                result = result.CopyAndReweightBy(0.5);
                result = result.Plus(currentBox.Interpolate(coordinates));

                // consider moving to the child
                nextBox = currentBox.ChooseChild(coordinates);
                // figure out whether it's time to stop splitting
                if (nextBox == null)
                {
                    break;
                }
                // the more datapoints we have, the more often that we split
                double nextOutputSpread = nextBox.GetScoreSpread();
                double nextInputSpread = nextBox.GetInputVariation();
                //inputFraction = nextInputSpread / maxInputSpread;
                //outputFraction = nextOutputSpread / maxOutputSpread;
                //datapointFraction = (double)nextBox.NumDatapoints / (double)this.root.NumDatapoints;

                //if (maxOutputSpread * maxOutputSpread * nextBox.NumDatapoints * nextBox.NumDatapoints * nextInputSpread <= this.root.NumDatapoints * maxInputSpread * nextOutputSpread * nextOutputSpread)
                //if ((inputFraction + datapointFraction) * nextBox.NumDatapoints <= outputFraction)
                //if (maxOutputSpread * nextBox.NumDatapoints * nextInputSpread <= maxInputSpread * nextOutputSpread)
                if (maxOutputSpread * nextBox.NumDatapoints * nextBox.NumDatapoints <= this.root.NumDatapoints * nextOutputSpread)
                {
                    // if we finally decided that we could split but didn't want to, then we probably have enough data to simply use the local data
                    // we don't need to incorporate the points that are further away
                    result = currentBox.Interpolate(coordinates);
                    break;
                }
                currentBox = nextBox;
            }
            // now interpolate using the box of appropriate granularity
            //Distribution result = currentBox.Interpolate(coordinates);
            return result;
        }
        public int NumDatapoints
        {
            get
            {
                return this.root.NumDatapoints;
            }
        }
        private SmartInterpolationBox<ScoreType> root;
        //private INumerifier<ScoreType> scoreHandler;
        //FloatRange outputSpan;
    }
}
