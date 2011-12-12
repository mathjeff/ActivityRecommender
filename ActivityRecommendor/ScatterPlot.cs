using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// The ScatterPlot stores a bunch of x,y points for the purpose of predicting y from x
namespace ActivityRecommendation
{
    class ScatterPlot : IComparer<double>
    {
        public ScatterPlot()
        {
            this.searchHelper = new StatList<double, Distribution>(this, new DistributionAdder());
        }
        public int NumDatapoints
        {
            get
            {
                return this.searchHelper.NumItems;
            }
        }
        public void AddDatapoint(Datapoint newDatapoint)
        {
            double key = newDatapoint.Input;
            double output = newDatapoint.Output;
            double weight = newDatapoint.Weight;
            Distribution value = new Distribution(output * weight, output * output * weight, weight);
            this.searchHelper.Add(key, value);
        }
        public Distribution Predict(double input)
        {
            // check whether there is any data to predict from
            if (this.searchHelper.NumItems <= 0)
            {
                return new Distribution(0, 0, 0);
            }
            // find the middle point
            int highMiddleIndex = this.searchHelper.CountBeforeKey(input, true);
            int lowMiddleIndex = this.searchHelper.CountBeforeKey(input, false) - 1;
            /*int middleIndex = (lowMiddleIndex + highMiddleIndex) / 2;
            if (middleIndex >= this.searchHelper.NumItems)
            {
                middleIndex = this.searchHelper.NumItems - 1;
            }*/
            // determine the size of the neighborhood that we care about
            int deltaIndex = (int)Math.Sqrt((double)(this.searchHelper.NumItems)) / 2;
            // determine the boundaries of the neighborhood
            //int lowerIndex = Math.Max(lowMiddleIndex - deltaIndex, 0);
            //int upperIndex = Math.Min(highMiddleIndex + deltaIndex, this.searchHelper.NumItems - 1);
            int lowerIndex = lowMiddleIndex - deltaIndex;
            int upperIndex = highMiddleIndex + deltaIndex;
            // if we fell off the edge of the list of datapoints, then add one more at the other end of the range
            if (lowerIndex < 0)
            {
                lowerIndex = 0;
                upperIndex++;
            }
            if (upperIndex >= this.searchHelper.NumItems)
            {
                upperIndex = this.searchHelper.NumItems - 1;
                if (lowerIndex > 0)
                {
                    lowerIndex--;
                }
            }
            ListItemStats<double, Distribution> lowerStats = this.searchHelper.GetValueAtIndex(lowerIndex);
            double lowerKey = lowerStats.Key;
            ListItemStats<double, Distribution> upperStats = this.searchHelper.GetValueAtIndex(upperIndex);
            double upperKey = upperStats.Key;
            // move the further edge of the window to no further away than the closer edge of the window
            /*if (Math.Abs(lowerKey - input) < Math.Abs(upperKey - input))
            {
                upperKey = input + (input - lowerKey);
            }
            else
            {
                lowerKey = input - (upperKey - input);
            }*/
            // add up all the points in the neighborhood
            Distribution conglomerate = this.searchHelper.SumBetweenKeys(lowerKey, true, upperKey, true);
            // We return that the output is probably similar to one of the points nearby
            // This should probably be eventually replaced with a linear regression within the neighborhood
            return conglomerate;
        }

        #region Functions for IComparer<double>
        
        public int Compare(double d1, double d2)
        {
            return d1.CompareTo(d2);
        }

        #endregion

        private StatList<double, Distribution> searchHelper;
    }
}
