using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation.Effectiveness
{
    class CompletionMetric : Metric
    {
        public CompletionMetric(Activity activity)
        {
            this.activity = activity;
        }
        private Activity activity;

        #region Required to implement Metric

        public Activity GetDoable()
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
