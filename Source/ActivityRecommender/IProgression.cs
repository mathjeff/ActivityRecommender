using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the Progression class represents the value of a (real-number) variable over time
namespace ActivityRecommendation
{
    public interface IProgression
    {
        ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier);   // the value of the Progression at that date, for training
        //ProgressionValue GetCurrentValue(DateTime when);                    // the value of the Progression at that date, to make the best prediction
        List<ProgressionValue> GetValuesAfter(int indexInclusive);          // the dates where the Progression makes a significant change
        int NumItems { get; }
        Activity Owner { get; }
        string Description { get; }
    }
}