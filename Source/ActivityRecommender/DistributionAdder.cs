using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;
using AdaptiveLinearInterpolation;

// The DistributionAdder class adds and subtracts distributions, to allow for use in Generics
namespace ActivityRecommendation
{
    class DistributionAdder : ICombiner<Distribution>, INumerifier<Distribution>
    {
        public Distribution Combine(Distribution d1, Distribution d2)
        {
            return d1.Plus(d2);
        }
        public Distribution Remove(Distribution sum, Distribution valueToRemove)
        {
            return sum.Minus(valueToRemove);
        }
        public Distribution Default()
        {
            return new Distribution(0, 0, 0);
        }
        public AdaptiveLinearInterpolation.Distribution ConvertToDistribution(Distribution distribution)
        {
            AdaptiveLinearInterpolation.Distribution converted = new AdaptiveLinearInterpolation.Distribution(distribution.SumValue, distribution.SumSquaredValue, distribution.Weight);
            return converted;
        }

    }
}
