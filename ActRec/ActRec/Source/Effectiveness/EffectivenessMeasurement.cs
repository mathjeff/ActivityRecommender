using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// An EffectivenessMeasurement is part of the system of experimentation for computing effectivness.
// An EffectivenessMeasurement records the effectiveness scores for a specific Participation
namespace ActivityRecommendation.Effectiveness
{
    // a CompletionEffectivenessMeasurement records whether the Participation completed its ToDo
    public class CompletionEffectivenessMeasurement
    {
        public bool Successful { get; set; }
        public RelativeEffectivenessMeasurement Computation { get; set; }
    }

    // A RelativeEffectivenessMeasurement records the computed effectiveness of the Participation by comparing it to another one
    public class RelativeEffectivenessMeasurement
    {
        // How well we compute the Participation to have done based on its duration, success/failure status, and its counterpart
        public double Score { get; set; }
        // The predicted value of Score / Participation.Duration.TotalSeconds
        public double PredictedScorePerSecond { get; set; }
        // The Participation being measured
        public Participation Participation { get; set; }
        // Counterpart is the other EffectivenessMetric (from the PlannedExperiment) that this was compared to.
        // The reason for a Counterpart is to ensure that results are not biased by the difference between expected difficulty and actual difficulty.
        // If PredictedScorePerSecond were used directly, then having a surprisingly difficult task would decrease the expected magnitude of the score.
        // Because of the existence of Counterpart, having a surprisingly difficult task will increase the variance of the score but not adjust the expected magnitude of the score.
        public RelativeEffectivenessMeasurement Counterpart { get; set; }
    }

}
