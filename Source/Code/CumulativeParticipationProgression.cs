using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveLinearInterpolation;

namespace ActivityRecommendation
{
    class CumulativeParticipationProgression : IProgression
    {
        public CumulativeParticipationProgression(ParticipationProgression datasource)
        {
            this.participationProgression = datasource;
        }
        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            Participation participation = this.participationProgression.SummarizeParticipationsBetween(new DateTime(), when);
            ProgressionValue value = new ProgressionValue(when, Distribution.MakeDistribution(participation.TotalIntensity.Mean * participation.TotalIntensity.Weight, 0, 1));
            return value;
        }
        public IEnumerable<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            throw new NotImplementedException();
        }
        public int NumItems 
        { 
            get
            {
                return this.participationProgression.NumItems;
            }
        }
        public Activity Owner 
        { 
            get
            {
                return this.participationProgression.Owner;
            }
        }
        public string Description 
        {
            get
            {
                return "The cumulative participation time in " + this.Owner.Name;
            }
        }
        public FloatRange EstimateOutputRange()
        {
            throw new NotImplementedException();
        }
        public IEnumerable<double> GetNaturalSubdivisions(double minSubdivision, double maxSubdivision)
        {
            throw new NotImplementedException();
        }

        private ParticipationProgression participationProgression;
    }
}
