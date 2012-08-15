using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// compares datapoints
namespace AdaptiveLinearInterpolation
{
    class DatapointComparer : IComparer<Datapoint>
    {
        public DatapointComparer(int inputDimToCompare)
        {
            this.dimension = inputDimToCompare;
        }
        #region Functions for IComparer<IDatapoint>
        public int Compare(Datapoint a, Datapoint b)
        {
            return a.InputCoordinates[this.dimension].CompareTo(b.InputCoordinates[this.dimension]);
        }
        #endregion
        private int dimension;
    }
}
