using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// an INumerifier needs to provide functions to convert a certain data type to a double, and to be able to add them
namespace AdaptiveLinearInterpolation
{
    public interface INumerifier<ValueType>
    {
        //double ConvertToDouble(ValueType value);
        //double GetStdDev(ValueType value);
        Distribution ConvertToDistribution(ValueType value);

        ValueType Combine(ValueType value1, ValueType value2);
        ValueType Remove(ValueType sum, ValueType itemToSubtract);
        ValueType Default();
    }
}
