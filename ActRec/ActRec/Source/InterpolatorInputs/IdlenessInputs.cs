using AdaptiveInterpolation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class IdlenessInputs : LazyInputs
    {
        public IdlenessInputs(DateTime when, Activity considered)
        {
            this.when = when;
            this.considered = considered;
        }
        public int GetNumCoordinates()
        {
            return 1;
        }
        public double GetInput(int index)
        {

            ProgressionValue value = this.considered.IdlenessProgression.GetValueAt(this.when, true);
            if (value != null)
                return value.Value.Mean;
            return 0;
        }
        public string GetDescription(int index)
        {
            return "Idleness of " + this.considered + " at " + when;
        }

        DateTime when;
        Activity considered;
    }
}
