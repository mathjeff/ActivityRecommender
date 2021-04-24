using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveInterpolation;
using StatLists;

// The FloatAdder adds numbers, for the purpose of using in Generics
namespace ActivityRecommendation
{
    public class FloatAdder : INumerifier<double>, ICombiner<double>
    {
        public FloatAdder()
        {
        }

        public AdaptiveInterpolation.Distribution ConvertToDistribution(double number)
        {
            return AdaptiveInterpolation.Distribution.MakeDistribution(number, 0, 1);
        }

        public double Combine(double a, double b)
        {
            return a + b;
        }

        public double Remove(double sum, double valueToSubtract)
        {
            return sum - valueToSubtract;
        }

        public double Default()
        {
            return 0;
        }

    }
}
