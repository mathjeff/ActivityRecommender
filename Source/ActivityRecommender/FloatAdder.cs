using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveLinearInterpolation;

// The FloatAdder adds numbers, for the purpose of using in Generics
namespace ActivityRecommendation
{
    public class FloatAdder : INumerifier<double>
    {
        public FloatAdder()
        {
        }

        #region Required for INumerifier

        public AdaptiveLinearInterpolation.Distribution ConvertToDistribution(double number)
        {
            return AdaptiveLinearInterpolation.Distribution.MakeDistribution(number, 0, 1);
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
        
        #endregion
    }
}
