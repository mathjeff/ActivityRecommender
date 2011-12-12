using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// The DistributionAdder class adds and subtracts distributions, to allow for use in Generics
namespace ActivityRecommendation
{
    class DistributionAdder : IAdder<Distribution>
    {
        public Distribution Sum(Distribution d1, Distribution d2)
        {
            return d1.Plus(d2);
        }
        public Distribution Difference(Distribution d1, Distribution d2)
        {
            return d1.Minus(d2);
        }
        public Distribution Zero()
        {
            return new Distribution(0, 0, 0);
        }

    }
}
