using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation.Effectiveness
{
    class CompletionMetric : Metric
    {
        public CompletionMetric(Doable activity)
        {
            this.activity = activity;
        }
        private Doable activity;

        #region Required to implement Metric

        public Doable GetDoable()
        {
            return this.activity;
        }

        public String Describe()
        {
            return "Complete " + this.activity.Name;
        }

        public double GetScore(Participation participation)
        {
            double numSuccesses = 0;
            if (this.WasSuccessful(participation))
                numSuccesses++;
            return numSuccesses;
        }
        #endregion

        public bool WasSuccessful(Participation participation)
        {
            throw new NotImplementedException();
        }

    }
}
