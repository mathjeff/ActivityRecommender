using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;
using AdaptiveInterpolation;

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
            return Distribution.Zero;
        }
        public AdaptiveInterpolation.Distribution ConvertToDistribution(Distribution distribution)
        {
            AdaptiveInterpolation.Distribution converted = new AdaptiveInterpolation.Distribution(distribution.SumValue, distribution.SumSquaredValue, distribution.Weight);
            return converted;
        }

    }
}
