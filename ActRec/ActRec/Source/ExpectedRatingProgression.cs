using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveInterpolation;

namespace ActivityRecommendation
{
    public class ExpectedRatingProgression : IProgression
    {
        public ExpectedRatingProgression(Activity owner, Engine engine)
        {
            this.Owner = owner;
            this.Engine = engine;
        }
        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            Prediction prediction = this.Engine.EstimateRating(this.Owner, when);
            ProgressionValue value = new ProgressionValue(when, prediction.Distribution);
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
                return "The current rating of " + this.Owner.Description;
            }
        }
        public Activity Owner { get; set; }
        public Engine Engine { get; set; }
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
