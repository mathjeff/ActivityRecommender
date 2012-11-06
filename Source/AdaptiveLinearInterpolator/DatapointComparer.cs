using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// compares datapoints
namespace AdaptiveLinearInterpolation
{
    class DatapointComparer<ScoreType> : IComparer<Datapoint<ScoreType>>
    {
        public DatapointComparer(int inputDimToCompare)
        {
            this.dimension = inputDimToCompare;
        }
        #region Functions for IComparer<IDatapoint<ScoreType>>
        public int Compare(Datapoint<ScoreType> a, Datapoint<ScoreType> b)
        {
            return a.InputCoordinates[this.dimension].CompareTo(b.InputCoordinates[this.dimension]);
        }
        #endregion
        private int dimension;
    }
}
