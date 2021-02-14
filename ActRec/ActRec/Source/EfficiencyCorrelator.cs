using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    public class EfficiencyCorrelator
    {
        public delegate void CorrelatorUpdatedHandler();
        public event CorrelatorUpdatedHandler CorrelatorUpdated;

        public EfficiencyCorrelator()
        {
        }
        public void Add(DateTime start, DateTime end, double efficiency)
        {
            if (this.firstDate == null)
                this.firstDate = start;
            double weight = end.Subtract(start).TotalDays;
            if (efficiency != 0)
            {
                double logEfficiency = Math.Log(efficiency, 2);
                this.correlator.Add(start.Subtract(this.firstDate.Value).TotalDays / 365, logEfficiency, weight);
                if (this.CorrelatorUpdated != null)
                    this.CorrelatorUpdated.Invoke();
            }
        }
        public Correlator Correlator
        {
            get
            {
                return this.correlator;
            }
        }

        Correlator correlator = new Correlator();
        DateTime? firstDate;
    }
}
