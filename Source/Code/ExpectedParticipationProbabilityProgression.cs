using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveLinearInterpolation;

namespace ActivityRecommendation
{
    class ExpectedParticipationProbabilityProgression : IProgression
    {
        public ExpectedParticipationProbabilityProgression(Activity owner)
        {
            this.Owner = owner;
        }
        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            ProgressionValue value = new ProgressionValue(when, Owner.PredictedParticipationProbability.Distribution);
            return value;
        }
        public IEnumerable<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            return null;
        }
        public int NumItems
        {
            get
            {
                return 1;
            }
        }
        public string Description
        {
            get
            {
                return "The current probability that you will do " + this.Owner.Description;
            }
        }
        public Activity Owner { get; set; }
        public FloatRange EstimateOutputRange()
        {
            return new FloatRange(0, true, 1, true);
        }
        public IEnumerable<double> GetNaturalSubdivisions(double minSubdivision, double maxSubdivision)
        {
            throw new NotImplementedException();
        }

    }
}
