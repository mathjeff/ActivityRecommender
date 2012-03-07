using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The ICombiner interface is intended to support any associated function
// It can be used to calculate the maximum, sum, or concatenation of items in a list
namespace StatLists
{
    public interface ICombiner<ValueType>
    {
        ValueType Combine(ValueType a, ValueType b);
        //ValueType Difference(ValueType larger, ValueType smaller);
        ValueType Default();
    }
}
