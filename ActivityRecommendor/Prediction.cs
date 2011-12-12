using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    public class Prediction
    {
        public Prediction()
        {
        }
        public Prediction(Distribution scores, string reason)
        {
            this.Distribution = scores;
            this.Justification = reason;
        }
        public string Justification { get; set; }
        public Distribution Distribution { get; set; }
        public Prediction Plus(Prediction other)
        {
            Prediction newPrediction = new Prediction();
            newPrediction.Distribution = this.Distribution.Plus(other.Distribution);
            return newPrediction;
        }
    }
}
