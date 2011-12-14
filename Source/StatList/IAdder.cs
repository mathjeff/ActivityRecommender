using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatLists
{
    public interface IAdder<ValueType>
    {
        ValueType Sum(ValueType a, ValueType b);
        //ValueType Difference(ValueType larger, ValueType smaller);
        ValueType Zero();
    }
}
