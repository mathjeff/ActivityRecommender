using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the Progression class represents the value of a (real-number) variable over time
namespace ActivityRecommendation
{
    public interface IProgression
    {
        ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier);
        Activity Owner { get; }
    }
}