using AdaptiveInterpolation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class LazyProgressionValue : LazyCoordinate
    {
        public LazyProgressionValue(DateTime when, IProgression progression)
        {
            this.when = when;
            this.progression = progression;
        }

        public double GetCoordinate()
        {
            return this.progression.GetValueAt(when, false).Value.Mean;
        }
        DateTime when;
        IProgression progression;
    }
}
