using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// the SimpleInterpolationBox will split in exactly the dimension it is told to split in
namespace AdaptiveLinearInterpolation
{
    class SimpleInterpolationBox<ScoreType> : ICombiner<IDatapoint<ScoreType>>
    {
        public SimpleInterpolationBox(HyperBox<ScoreType> boundary, LinkedList<int> dimensionSplitOrder, int dimToSort, INumerifier<ScoreType> scoreHandler)
        {
            this.scoreHandler = scoreHandler;
            this.currentBoundary = boundary;
            this.observedBoundary = null;
            this.dimensionsToSplit = dimensionSplitOrder;
            //this.datapointsByInput = new StatList<Datapoint, Datapoint>(new DatapointComparer(dimToSort), this);
            this.datapoints = new LinkedList<IDatapoint<ScoreType>>();
            this.dimensionToSort = dimToSort;
            this.numDatapoints = 0;
            this.splitDimension = dimensionSplitOrder.First.Value;
            this.depth = 0;

        }
        public void AddDatapoint(IDatapoint<ScoreType> newDatapoint)
        {
            // if this datapoint falls outside our promised boundary, then expand our boundary to include it
            if (this.observedBoundary == null)
            {
                this.observedBoundary = new HyperBox<ScoreType>(newDatapoint);
            }
            else
            {
                this.observedBoundary.ExpandToInclude(newDatapoint);
            }
            this.currentBoundary.ExpandToInclude(newDatapoint);
            // keep track of a datapoint on each extreme
            if (this.maxPoint == null || this.maxPoint.InputCoordinates[this.dimensionToSort] <= newDatapoint.InputCoordinates[this.dimensionToSort])
                this.maxPoint = newDatapoint;
            // keep track of a datapoint on each extreme
            if (this.minPoint == null || this.minPoint.InputCoordinates[this.dimensionToSort] >= newDatapoint.InputCoordinates[this.dimensionToSort])
                this.minPoint = newDatapoint;
            //this.datapointsByInput.Add(newDatapoint, newDatapoint);
            this.numDatapoints++;
            if (this.lowerChild == null && this.upperChild == null)
            {
                this.datapoints.AddLast(newDatapoint);
                this.ConsiderSplitting();
            }
            else
            {
                if (this.lowerChild.currentBoundary.Contains(newDatapoint))
                    this.lowerChild.AddDatapoint(newDatapoint);
                if (this.upperChild.currentBoundary.Contains(newDatapoint))
                    this.upperChild.AddDatapoint(newDatapoint);
            }
            this.UpdateFromChildren();
        }
        public void ConsiderSplitting()
        {
            // make sure there is a splittable dimension where the spread is nonzero
            bool splittable = false;
            foreach (int coordinate in this.dimensionsToSplit)
            {
                if (this.observedBoundary.Coordinates[coordinate].Width > 0)
                {
                    splittable = true;
                    break;
                }
            }
            if (!splittable)
                return;
            // now split
            int dimension = this.splitDimension;

            // compute the coordinates of each child
            double splitValue = (this.currentBoundary.Coordinates[dimension].LowCoordinate + this.currentBoundary.Coordinates[dimension].HighCoordinate) / 2;
            HyperBox<ScoreType> lowerBoundary = new HyperBox<ScoreType>(this.currentBoundary);
            lowerBoundary.Coordinates[dimension].HighCoordinate = splitValue;
            lowerBoundary.Coordinates[dimension].HighInclusive = true;
            HyperBox<ScoreType> upperBoundary = new HyperBox<ScoreType>(this.currentBoundary);
            upperBoundary.Coordinates[dimension].LowCoordinate = splitValue;
            upperBoundary.Coordinates[dimension].LowInclusive = false;
            // determine the split order for the children
            LinkedList<int> childSplitOrder = new LinkedList<int>(this.dimensionsToSplit);
            childSplitOrder.RemoveFirst();
            childSplitOrder.AddLast(dimension);

            // fill data into the children
            this.lowerChild = new SimpleInterpolationBox<ScoreType>(lowerBoundary, childSplitOrder, this.dimensionToSort, this.scoreHandler);
            this.upperChild = new SimpleInterpolationBox<ScoreType>(upperBoundary, childSplitOrder, this.dimensionToSort, this.scoreHandler);
            foreach (IDatapoint<ScoreType> datapoint in this.datapoints)
            {
                if (lowerBoundary.Contains(datapoint))
                    this.lowerChild.AddDatapoint(datapoint);
                if (upperBoundary.Contains(datapoint))
                    this.upperChild.AddDatapoint(datapoint);
            }
        }
        public int SplitDimension
        {
            get
            {
                return this.splitDimension;
            }
        }
        public double GetError()
        {
            double result = this.totalError / this.numDatapoints;
            return result;
        }
        public void UpdateFromChildren()
        {
            if (this.lowerChild == null || this.upperChild == null)
            {
                this.depth = 0;
                this.totalError = 0;
                return;
            }
            double lowerError = this.lowerChild.totalError;
            double upperError = this.upperChild.totalError;
            double difference;
            IDatapoint<ScoreType> nextPoint = this.upperChild.minPoint;
            IDatapoint<ScoreType> previousPoint = this.lowerChild.maxPoint;
            if (nextPoint != null && previousPoint != null)
            {
                double nextScore = this.scoreHandler.ConvertToDistribution(nextPoint.Score).Mean;
                double previousScore = this.scoreHandler.ConvertToDistribution(previousPoint.Score).Mean;
                difference = Math.Abs(nextScore - previousScore);
                difference *= difference;
            }
            else
            {
                difference = 0;
            }
            this.totalError = lowerError + difference + upperError;

            this.depth = Math.Min(this.lowerChild.depth, this.upperChild.depth) + 1;
        }
        public int NumDimensions
        {
            get
            {
                return this.currentBoundary.NumDimensions;
            }
        }
        public int Depth
        {
            get
            {
                return this.depth;
            }
        }
        #region Functions for ICombiner<IDatapoint>
        public IDatapoint<ScoreType> Combine(IDatapoint<ScoreType> a, IDatapoint<ScoreType> b)
        {
            return null;
        }
        public IDatapoint<ScoreType> Default()
        {
            return null;
        }
        #endregion

        //StatList<Datapoint, Datapoint> datapointsByInput;
        LinkedList<IDatapoint<ScoreType>> datapoints;
        int numDatapoints;
        IDatapoint<ScoreType> minPoint;
        IDatapoint<ScoreType> maxPoint;
        private LinkedList<int> dimensionsToSplit;
        private int splitDimension;
        private HyperBox<ScoreType> currentBoundary;
        private HyperBox<ScoreType> observedBoundary;
        private SimpleInterpolationBox<ScoreType> lowerChild;
        private SimpleInterpolationBox<ScoreType> upperChild;
        private double totalError;
        private int dimensionToSort;
        private int depth;
        private INumerifier<ScoreType> scoreHandler;
    }
}
