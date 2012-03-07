using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// compares datapoints
namespace AdaptiveLinearInterpolation
{
    class DatapointComparer : IComparer<Datapoint>
    {
        public DatapointComparer(int dimToSort)
        {
            this.dimension = dimToSort;
        }
        #region Functions for IComparer<IDatapoint>
        public int Compare(Datapoint a, Datapoint b)
        {
            return a.Coordinates[this.dimension].CompareTo(b.Coordinates[this.dimension]);
        }
        #endregion
        private int dimension;
    }
}
