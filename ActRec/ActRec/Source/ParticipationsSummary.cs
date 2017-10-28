using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation
{
    public class ParticipationsSummary
    {
        public ParticipationsSummary() { }

        public Distribution LogActiveTime { get; set; }
        public Correlator Trend { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public TimeSpan CumulativeIntensity { get; set; }
    }
}
