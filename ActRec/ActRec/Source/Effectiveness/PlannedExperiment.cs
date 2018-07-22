using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// a PlannedExperiment represents an intent to do two Participations, each measured by a corresponding Metric, and compare the results
// a PlannedExperiment is a prerequisite to obtaining an EffectivenessMeasurement
namespace ActivityRecommendation.Effectiveness
{
    class PlannedExperiment
    {
        PlannedMetric Earlier { get; set; }
        PlannedMetric Later { get; set; }
        Participation FirstParticipation { get; set; }
    }

    // a PlannedMeasuredParticipation is a plan to do a specific Participation and to measure it in a certain way
    class PlannedMetric
    {
        public Metric Metric { get; set; }

        // an estimate of estimate of Metric.getScore(participation) / participation.Duration.TotalSeconds
        public double EstimatedSuccessesPerSecond { get; set; }
    }
}
