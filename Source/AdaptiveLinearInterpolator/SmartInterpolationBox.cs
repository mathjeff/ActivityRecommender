using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// the SmartInterpolationBox will figure out which dimensions are worth splitting and when
namespace AdaptiveLinearInterpolation
{
    class SmartInterpolationBox : IComparer<Datapoint>, ICombiner<Datapoint>
    {
        public SmartInterpolationBox(HyperBox boundary)
        {
            int i;
            this.currentBoundary = boundary;
            this.observedBoundary = null;
            this.splitDimension = -1;
            this.datapoints = new LinkedList<Datapoint>();
            this.pendingDatapoints = new LinkedList<Datapoint>();
            this.outputs = new Distribution();
#if MIN_SPLIT_COUNTS
            this.minSplitCounts = new int[boundary.NumDimensions];
#endif
            this.inputs = new Distribution[boundary.NumDimensions];
            for (i = 0; i < this.inputs.Length; i++)
            {
                this.inputs[i] = new Distribution();
            }
        }
        // adds a datapoint
        public void AddDatapoint(Datapoint newDatapoint)
        {
            this.pendingDatapoints.AddLast(newDatapoint);
            //this.AddPointNowWithoutSplitting(newDatapoint);
            //this.ConsiderSplitting();
        }
        // adds a datapoint and does not consider splitting
        private void AddPointNowWithoutSplitting(Datapoint newDatapoint)
        {
            // keep track of the outputs we've observed
            this.outputs = this.outputs.Plus(newDatapoint.Score);
            int i;
            for (i = 0; i < this.inputs.Length; i++)
            {
                this.inputs[i] = this.inputs[i].Plus(newDatapoint.InputCoordinates[i]);
            }

            // if this datapoint falls outside our promised boundary, then expand our boundary to include it
            if (this.observedBoundary == null)
            {
                this.observedBoundary = new HyperBox(newDatapoint);
            }
            else
            {
                this.observedBoundary.ExpandToInclude(newDatapoint);
            }
            if (!this.currentBoundary.Contains(newDatapoint))
            {
                this.currentBoundary.ExpandToInclude(newDatapoint);
            }
            // add it to our set
            //this.datapointsByOutput.Add(newDatapoint, Distribution.MakeDistribution(newDatapoint.Output, 0, 1));
            this.datapoints.AddLast(newDatapoint);
            //if (this.datapointsByInput != null)
            //    this.datapointsByInput.Add(newDatapoint, newDatapoint);
            /*
            foreach (SimpleInterpolationBox box in this.simpleChildren)
            {
                box.AddDatapoint(newDatapoint);
            }
            */
            if (this.lowerChild != null)
            {
                if (newDatapoint.InputCoordinates[this.splitDimension] > this.lowerChild.currentBoundary.Coordinates[this.splitDimension].HighCoordinate)
                    this.upperChild.AddDatapoint(newDatapoint);
                else
                    this.lowerChild.AddDatapoint(newDatapoint);
#if MIN_SPLIT_COUNTS
                this.UpdateSplitCounts();
#endif
            }
        }
        // Exactly the same as calling AddDatapoint a bunch of times, but potentially faster
        public void AddDatapoints(IEnumerable<Datapoint> newDatapoints)
        {
            foreach (Datapoint newDatapoint in newDatapoints)
            {
                this.AddDatapoint(newDatapoint);
            }
        }
        public Distribution Interpolate(double[] location)
        {
            this.ApplyPendingPoints();
            // finally, return the 
            return this.outputs;
        }
        // moves any points from the list of pendingPoints into the main list, and updates any stats
        private void ApplyPendingPoints()
        {
            // Figure out how many points there should be at the final split.
            // We simulate adding them one at a time, so that the results are the same as if they were added one at a time
            int count = this.datapoints.Count;
            int splitCount = this.numPointsAtLastConsideredSplit;
            foreach (Datapoint datapoint in this.pendingDatapoints)
            {
                count++;
                if (this.HasTimeToSplit(count, splitCount))
                {
                    splitCount = count;
                }
            }
            // Now actually add those points and do the split
            foreach (Datapoint datapoint in this.pendingDatapoints)
            {
                this.AddPointNowWithoutSplitting(datapoint);
                if (this.datapoints.Count == splitCount)
                    this.ConsiderSplitting();
            }
            this.pendingDatapoints.Clear();
        }
        // estimates the product of amount of variation in each dimension
        public double GetInputVariation()
        {
            this.ApplyPendingPoints();
            double variation = 1;
            double currentVariation;
            int i;
            //foreach (Distribution distribution in this.inputs)
            for (i = 0; i < this.NumDimensions; i++)
            {
                //currentVariation = Math.Min(this.inputs[i].StdDev, this.currentBoundary.Coordinates[i].Width / 4);
                //currentVariation = (this.inputs[i].StdDev + this.currentBoundary.Coordinates[i].Width / this.NumDatapoints);
                currentVariation = this.inputs[i].StdDev;
                variation *= currentVariation;
            }
            return variation;
        }
        public double GetInputArea()
        {
            this.ApplyPendingPoints();
            return this.currentBoundary.Area;
        }
        public double GetOutputSpread()
        {
            this.ApplyPendingPoints();
            //Distribution outputs = this.datapointsByOutput.CombineAll();
            //double spread = outputs.StdDev;
            double spread = this.outputs.StdDev;
            return spread;
        }
        public int NumDatapoints
        {
            get
            {
                return this.datapoints.Count + this.pendingDatapoints.Count;
            }
        }
        public int NumDimensions
        {
            get
            {
                return this.currentBoundary.NumDimensions;
            }
        }
        public SmartInterpolationBox ChooseChild(double[] coordinates)
        {
            this.ApplyPendingPoints();
            if (this.lowerChild != null && this.lowerChild.currentBoundary.Contains(coordinates))
                return this.lowerChild;
            if (this.upperChild != null && this.upperChild.currentBoundary.Contains(coordinates))
                return this.upperChild;
            return null;
        }

        #region Functions for IComparer<Datapoint>
        public int Compare(Datapoint a, Datapoint b)
        {
            return a.Score.CompareTo(b.Score);
        }
        #endregion

        #region Functions for IAdder<Datapoint>
        public Datapoint Default()
        {
            return new Datapoint(null, 0);
        }
        public Datapoint Combine(Datapoint a, Datapoint b)
        {
            Datapoint result = new Datapoint(null, a.Score + b.Score);
            return result;
        }
        #endregion

        // tells whether it is worth considering a split, given the current number of datapoints and also the number that we had at the last split
        public bool HasTimeToSplit(int numDatapoints, int previousNumPoints)
        {
            if (numDatapoints >= previousNumPoints * 2)
                return true;
            return false;
        }
        public void ConsiderSplitting()
        {
            // we only allow have time to split every once in a while
            if (!this.HasTimeToSplit(this.datapoints.Count, this.numPointsAtLastConsideredSplit))
                return;

            this.numPointsAtLastConsideredSplit = this.datapoints.Count;
            if (this.datapoints.Count <= 1)
                return;
            int i, j;
            // if all subchild trees agree that it's good to split the current dimension, then we don't need to bother
            /*if (this.splitDimension >= 0)
            {
                if (this.minSplitCounts[this.splitDimension] >= 2)
                {
                    return;
                }
            }*/
            // setup a bunch of SimpleInterpolationBox to calculate the error we'd get if we couldn't split any particular dimension
            // setup the simple children
            SimpleInterpolationBox[] simpleChildren = new SimpleInterpolationBox[this.NumDimensions];
            for (i = 0; i < simpleChildren.Length; i++)
            {
                // tell it to split each dimension in order, except for one
                LinkedList<int> dimensionsToSplit = new LinkedList<int>();
                for (j = 0; j < this.NumDimensions; j++)
                {
                    if (j != i)
                    {
                        dimensionsToSplit.AddLast(j);
                    }
                }
                SimpleInterpolationBox box = new SimpleInterpolationBox(this.currentBoundary, dimensionsToSplit, i);
                foreach (Datapoint datapoint in this.datapoints)
                {
                    box.AddDatapoint(datapoint);
                }
                simpleChildren[i] = box;
            }

            double bestError = -1;
            int dimension = -1;
            /*
            // if there are children already, then we only split a different dimension if we could do substantially better
            if (this.splitDimension >= 0)
            {
                // apply a bonus to the current error, so we don't flip-flop too often
                bestError = simpleChildren[this.splitDimension].GetError() * 2;
                dimension = this.splitDimension;
            }*/
            // identify the dimension such that if we cannot split that dimension, then we get the most error
            for (i = 0; i < this.NumDimensions; i++)
            {
                if (this.observedBoundary.Coordinates[i].Width > 0)
                {
#if true
                    double currentError = simpleChildren[i].GetError();
#else
                    double currentError = this.observedBoundary.Coordinates[i].Width;
#endif
                    if (currentError > bestError)
                    {
                        bestError = currentError;
                        dimension = i;
                    }
                }
            }
            // if we can't split, then give up
            if (dimension < 0)
                return;
            // if the worst child didn't split in every dimension, then we don't have enough data to worry about longterm convergence
            if (simpleChildren[dimension].Depth <= this.NumDimensions - 1)
            {
                // so, we simply split the first split dimension of the best child
                for (i = 0; i < this.NumDimensions; i++)
                {
                    int d = simpleChildren[i].SplitDimension;
                    if (this.observedBoundary.Coordinates[d].Width > 0)
                    {
                        if (simpleChildren[i].GetError() < bestError)
                        {
                            dimension = d;
                            bestError = simpleChildren[i].GetError();
                        }
                    }
                }
            }
            /*
            if (dimension != this.splitDimension)
            {
                this.Split(dimension);
            }
            */
            this.Split(dimension);
        }
        public void Split(int dimension)
        {
            this.splitDimension = dimension;

            // make sure that datapointsByInput exists, and that it sorts in the correct dimension
            //this.datapointsByInput = new StatList<Datapoint, Datapoint>(new DatapointComparer(dimension), this);

            // compute the coordinates of each child
#if true
            double splitValue = (this.observedBoundary.Coordinates[dimension].LowCoordinate + this.observedBoundary.Coordinates[dimension].HighCoordinate) / 2;
            if (splitValue == this.currentBoundary.Coordinates[dimension].HighCoordinate || splitValue == this.currentBoundary.Coordinates[dimension].LowCoordinate)
                splitValue = (this.currentBoundary.Coordinates[dimension].LowCoordinate + this.currentBoundary.Coordinates[dimension].HighCoordinate) / 2;
#else
            double splitValue = (this.currentBoundary.Coordinates[dimension].LowCoordinate + this.currentBoundary.Coordinates[dimension].HighCoordinate) / 2;
#endif
            HyperBox lowerBoundary = new HyperBox(this.currentBoundary);
            lowerBoundary.Coordinates[dimension].HighCoordinate = splitValue;
            lowerBoundary.Coordinates[dimension].HighInclusive = true;
            HyperBox upperBoundary = new HyperBox(this.currentBoundary);
            upperBoundary.Coordinates[dimension].LowCoordinate = splitValue;
            upperBoundary.Coordinates[dimension].LowInclusive = false;

            //if (this.NumDimensions == 3 && dimension == 0 && splitValue == 900 && this.currentBoundary.Coordinates[0].LowCoordinate == 0)
            //    upperBoundary = upperBoundary;

            // decide which data goes in which child
            LinkedList<Datapoint> lowerPoints = new LinkedList<Datapoint>();
            LinkedList<Datapoint> upperPoints = new LinkedList<Datapoint>();            
            foreach (Datapoint datapoint in this.datapoints)
            {
                if (lowerBoundary.Contains(datapoint))
                    lowerPoints.AddLast(datapoint);
                if (upperBoundary.Contains(datapoint))
                    upperPoints.AddLast(datapoint);
            }
            this.lowerChild = new SmartInterpolationBox(lowerBoundary);
            this.lowerChild.AddDatapoints(lowerPoints);
            this.upperChild = new SmartInterpolationBox(upperBoundary);
            this.upperChild.AddDatapoints(upperPoints);
#if false
            this.lowerChild.ApplyPendingPoints();
            this.upperChild.ApplyPendingPoints();
#endif
#if MIN_SPLIT_COUNTS
            this.UpdateSplitCounts();
#endif
        }
#if MIN_SPLIT_COUNTS
        public void UpdateSplitCounts()
        {
            int i;
            for (i = 0; i < this.NumDimensions; i++)
            {
                this.minSplitCounts[i] = Math.Min(this.lowerChild.minSplitCounts[i], this.upperChild.minSplitCounts[i]);
            }
            if (this.splitDimension >= 0)
                this.minSplitCounts[this.splitDimension]++;
        }
#endif
        /*
        public double GetError()
        {
            double result = this.totalError / this.datapointsByOutput.NumItems;
            return result;
        }
        public void UpdateError()
        {
            if (this.lowerChild == null || this.upperChild == null)
            {
                this.totalError = 0;
                return;
            }

            double lowerError = this.lowerChild.totalError;
            double upperError = this.upperChild.totalError;
            double nextOutput = this.upperChild.datapointsByInput.GetFirstValue().Value.Output;
            double previousOutput = this.lowerChild.datapointsByInput.GetLastValue().Value.Output;
            double difference = Math.Abs(nextOutput - previousOutput);
            this.totalError = lowerError + difference + upperError;
        }
        */


        private HyperBox currentBoundary;
        private HyperBox observedBoundary;
        private int splitDimension;
        private SmartInterpolationBox lowerChild;
        private SmartInterpolationBox upperChild;
        //private SimpleInterpolationBox[] simpleChildren;
        private Distribution outputs;
        private LinkedList<Datapoint> datapoints;
        private LinkedList<Datapoint> pendingDatapoints;    // any points that are waiting in a queue to be added. Their values aren't included in anything yet
        //private StatList<Datapoint, Distribution> datapointsByOutput;
        //private StatList<Datapoint, Datapoint> datapointsByInput;
        //private double totalError;
        private int numPointsAtLastConsideredSplit;
#if MIN_SPLIT_COUNTS
        private int[] minSplitCounts;  // the minimum number of times that dimension[index] was split, over any path through the children
#endif
        private Distribution[] inputs;
    }
}
