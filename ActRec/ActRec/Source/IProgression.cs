using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveInterpolation;

// the Progression class represents the value of a (real-number) variable over time
namespace ActivityRecommendation
{
    public interface IProgression
    {
        ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier);   // the value of the Progression at that date, for training
        IEnumerable<ProgressionValue> GetValuesAfter(int indexInclusive);          // the dates where the Progression makes a significant change
        int NumItems { get; }
        Activity Owner { get; }
        string Description { get; }
        FloatRange EstimateOutputRange();
        IEnumerable<double> GetNaturalSubdivisions(double minSubdivision, double maxSubdivision);
    }
}