using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// An EffectivenessMeasurement is part of the system of experimentation for computing effectivness.
// An EffectivenessMeasurement records the effectiveness scores for a specific Participation
namespace ActivityRecommendation.Effectiveness
{
    public class EffectivenessMeasurement
    {
        // How well the Participation did
        public double Score { get; set; }
        // The predicted value of Score / Participation.Duration.TotalSeconds
        public double PredictedScorePerSecond { get; set; }
        // The Participation being measured
        public Participation Participation { get; set; }
        // The way that we measured the Participation
        public Metric Metric { get; set; }
        // Counterpart is the other EffectivenessMetric that this was compared to
        // The reason for a Counterpart is to ensure that results are not biased by the difference between expected difficulty and actual difficulty
        // If PredictedScorePerSecond were used directly, then having a surprisingly difficult task would increase the variance and decrease the expected magnitude of the score
        // Because of the existence of Counterpart, having a surprisingly difficult task will increase the variance of the score but not adjust the expected magnitude of the score
        public EffectivenessMeasurement Counterpart { get; set; }
    }
}
