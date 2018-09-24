using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// An EfficiencyMeasurement is part of the system of experimentation for computing effectivness.
// An EfficiencyMeasurement records the effectiveness scores for a specific Participation
namespace ActivityRecommendation.Effectiveness
{
    // a CompletionEffectivenessMeasurement records whether the Participation completed its ToDo
    public class CompletionEfficiencyMeasurement
    {
        public CompletionEfficiencyMeasurement(bool successful) { this.Successful = successful; }
        public CompletionEfficiencyMeasurement(RelativeEfficiencyMeasurement computation, bool successful) { this.Computation = computation; this.Successful = successful; }

        public bool Successful { get; set; }
        public RelativeEfficiencyMeasurement Computation { get; set; }
    }

    // A RelativeEffectivenessMeasurement records the computed effectiveness of the Participation by comparing it to another one
    public class RelativeEfficiencyMeasurement
    {
        public RelativeEfficiencyMeasurement(Participation participation, Distribution efficiency)
        {
            this.ActivityDescriptor = participation.ActivityDescriptor;
            this.Date = participation.StartDate;
            this.RecomputedEfficiency = efficiency;
        }
        public RelativeEfficiencyMeasurement() { }

        public void FillInFromParticipation(Participation participation)
        {
            this.ActivityDescriptor = participation.ActivityDescriptor;
            this.Date = participation.StartDate;
        }

        // How well we compute the Participation to have done based on its duration, success/failure status, and its counterpart
        public Distribution RecomputedEfficiency { get; set; }

        // The participation being measured
        public ActivityDescriptor ActivityDescriptor { get; set; }
        public DateTime Date { get; set; }

        // the other RelativeEfficiencyMeasurement that was used to help compute this one, if the other one was earlier
        public RelativeEfficiencyMeasurement Earlier { get; set; }
        // the later RelativeEfficiencyMeasurement that was used to help compute this one, if the other one was later
        public RelativeEfficiencyMeasurement Later { get; set; }
    }

}
