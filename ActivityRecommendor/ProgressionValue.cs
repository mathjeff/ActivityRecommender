using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the ProgressionValue class represents the value of a Progression at a particular point in time
// It is primarily a real-number, but also contains information about how it was generated
namespace ActivityRecommendation
{
    public class ProgressionValue
    {
        public ProgressionValue(Distribution outputValue, int pointIndex)
        {
            this.value = outputValue;
            this.index = pointIndex;
        }
        // returns the value of the Progression
        public Distribution Value
        {
            get
            {
                return this.value;
            }
        }
        public int Index
        {
            get
            {
                return this.index;
            }
        }
        // returns true iff the datapoints used to create 'this' are the same datapoints used to create 'other'
        public bool IsBasedOnSameDatapoints(ProgressionValue other)
        {
            if (this.index == other.index)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Distribution value;
        private int index;
    }
}
