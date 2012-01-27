using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            ProgressionValue value = new ProgressionValue(when, Owner.PredictedParticipationProbability.Distribution, Owner.NumConsiderations);
            return value;
        }
        public List<ProgressionValue> GetValuesAfter(int indexInclusive)
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

    }
}
