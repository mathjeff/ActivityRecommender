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
            if (this.NumDatapoints == 0)
            {
                this.minXObserved = this.maxXObserved = key;
                this.minYObserved = this.maxYObserved = output;
            }
            else
            {
                if (key < this.minXObserved)
                    this.minXObserved = key;
                if (key > this.maxXObserved)
                    this.maxXObserved = key;
                if (output < this.minYObserved)
                    this.minYObserved = output;
                if (output > this.maxYObserved)
                    this.maxYObserved = output;
            }
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
            return this.AdaptivePrediction(input);
            //return this.PredictRootNNearestNeighbors(input);
        }
        public Distribution PredictRootNNearestNeighbors(double input)
        {
            // find the middle point
            int highMiddleIndex = this.searchHelper.CountBeforeKey(input, true);
            int lowMiddleIndex = this.searchHelper.CountBeforeKey(input, false) - 1;
            // determine the size of the neighborhood that we care about
            int deltaIndex = (int)Math.Sqrt((double)(this.searchHelper.NumItems)) / 2;
            // determine the boundaries of the neighborhood
            int lowerIndex = lowMiddleIndex - deltaIndex;
            int upperIndex = highMiddleIndex + deltaIndex;
            ListItemStats<double, Distribution> lowerStats;
            double lowerKey;
            ListItemStats<double, Distribution> upperStats;
            double upperKey;
            // now the behavior changes based on whether the large values are near small ones or not
            if (this.InputWrapsAround)
            {
                // check for indices out of range
                if (lowerIndex < 0 || upperIndex >= this.NumDatapoints)
                {
                    // Handle wraparound. If we get here, then we're including the first part and last part of the input interval (meaning there is wraparound)
                    if (lowerIndex < 0)
                        lowerIndex += this.NumDatapoints;
                    if (upperIndex >= this.NumDatapoints)
                        upperIndex -= this.NumDatapoints;
                    // don't double-count
                    if (lowerIndex == upperIndex)
                        upperIndex--;

                    lowerStats = this.searchHelper.GetValueAtIndex(lowerIndex);
                    lowerKey = lowerStats.Key;
                    Distribution lowerSum = this.searchHelper.CombineAfterKey(lowerKey, true);

                    Distribution result = lowerSum;
                    if (upperIndex >= 0)
                    {
                        upperStats = this.searchHelper.GetValueAtIndex(upperIndex);
                        upperKey = upperStats.Key;
                        Distribution upperSum = this.searchHelper.CombineBeforeKey(upperKey, true);
                        result = result.Plus(upperSum);
                    }
                    return result;
                }
            }
            else
            {
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
                        lowerIndex--;
                }
            }
            lowerStats = this.searchHelper.GetValueAtIndex(lowerIndex);
            lowerKey = lowerStats.Key;
            upperStats = this.searchHelper.GetValueAtIndex(upperIndex);
            upperKey = upperStats.Key;
            // add up all the points in the neighborhood
            Distribution conglomerate = this.searchHelper.CombineBetweenKeys(lowerKey, true, upperKey, true);
            // We return that the output is probably similar to one of the points nearby
            // This should probably be eventually replaced with a linear regression within the neighborhood
            return conglomerate;
        }
        public Distribution AdaptivePrediction(double input)
        {
            //ListItemStats<double, Distribution> firstItem = this.searchHelper.GetValueAtIndex(0);
            //ListItemStats<double, Distribution> lastItem = this.searchHelper.GetValueAtIndex(this.NumDatapoints - 1);
            //double minXObserved = firstItem.Key;
            //double maxXObserved = lastItem.Key;
            double minWindowX = this.minXObserved;
            double maxWindowX = this.maxXObserved;
            double newX1 = minWindowX;
            double newX2 = maxWindowX;
            Distribution contents = this.searchHelper.CombineBetweenKeys(minWindowX, true, maxWindowX, true);
            //while ((this.maxXObserved - this.minXObserved) / (maxWindowX - minWindowX) < (this.maxYObserved - this.minYObserved) / (contents.StdDev / contents.Weight))
            while ((this.maxXObserved - this.minXObserved) * contents.StdDev < (this.maxYObserved - this.minYObserved) * contents.Weight * (maxWindowX - minWindowX)
                && contents.Weight > 2)
            {
                // find the rightmost and leftmost datapoints in the domain we're considering
                ListItemStats<double, Distribution> leftStats = this.searchHelper.FindNextItem(newX1, false);
                if (leftStats != null)
                    minWindowX = leftStats.Key;
                else
                    minWindowX = newX1;
                ListItemStats<double, Distribution> rightStats = this.searchHelper.FindPreviousItem(newX2, false);
                if (rightStats != null)
                    maxWindowX = rightStats.Key;
                else
                    maxWindowX = newX2;
                // make the domain a little smaller
                double width = maxWindowX - minWindowX;
                newX1 = input - width / 4;
                newX2 = input + width / 4;
                if (minWindowX == newX1 && maxWindowX == newX2)
                {
                    // if there would be an infinite loop, then quit
                    break;
                }
                //newX1 = (minWindowX * 3 + maxWindowX) / 4;
                //newX2 = (minWindowX + maxWindowX * 3) / 4;
                // find the standard deviation of all points within this domain
                contents = this.searchHelper.CombineBetweenKeys(newX1, true, newX2, true);
            }
            Distribution result = this.searchHelper.CombineBetweenKeys(minWindowX, true, maxWindowX, true);
            return result;
        }

        public bool InputWrapsAround { get; set; } // tells whether really large values should be considered to be close to really small values
        #region Functions for IComparer<double>
        
        public int Compare(double d1, double d2)
        {
            return d1.CompareTo(d2);
        }

        #endregion

        private StatList<double, Distribution> searchHelper;
        private double minXObserved;
        private double maxXObserved;
        private double minYObserved;
        private double maxYObserved;
    }
}
