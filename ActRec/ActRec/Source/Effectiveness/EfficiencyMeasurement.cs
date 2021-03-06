﻿using System;
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
        public CompletionEfficiencyMeasurement(Metric metric, bool successful, double helpFraction)
        {
            this.Successful = successful;
            this.HelpFraction = helpFraction;
            this.Metric = metric;
        }
        public CompletionEfficiencyMeasurement(RelativeEfficiencyMeasurement computation, Metric metric, bool successful, double helpFraction)
        {
            this.Computation = computation;
            this.Successful = successful;
            this.HelpFraction = helpFraction;
            this.Metric = metric;
        }

        // Whether this task was completed
        public bool Successful { get; set; }
        // What fraction of the total effort came from an entity other than the user
        public double HelpFraction { get; set; }
        // Whether the task should be dismissed
        public bool DismissedActivity { get; set; }
        public Metric Metric { get; set; }
        public RelativeEfficiencyMeasurement Computation { get; set; }
    }

    // A RelativeEffectivenessMeasurement records the computed effectiveness of the Participation by comparing it to another one
    public class RelativeEfficiencyMeasurement : EfficiencyMeasurement
    {
        public RelativeEfficiencyMeasurement(Participation participation, Distribution efficiency)
        {
            this.FillInFromParticipation(participation);
            this.RecomputedEfficiency = efficiency;
        }
        public RelativeEfficiencyMeasurement() { }

        public void FillInFromParticipation(Participation participation)
        {
            this.ActivityDescriptor = participation.ActivityDescriptor;
            this.StartDate = participation.StartDate;
            this.EndDate = participation.EndDate;
        }

        // How well we compute the Participation to have done based on its duration, success/failure status, and its counterpart
        public Distribution RecomputedEfficiency { get; set; }

        // The participation being measured
        public ActivityDescriptor ActivityDescriptor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // the other RelativeEfficiencyMeasurement that was used to help compute this one, if the other one was earlier
        public RelativeEfficiencyMeasurement Earlier { get; set; }
        // the later RelativeEfficiencyMeasurement that was used to help compute this one, if the other one was later

        public PlannedExperiment Experiment { get; set; }
    }

    // An EfficiencyMeasurement records the computed efficiency of a Participation
    public interface EfficiencyMeasurement
    {
        ActivityDescriptor ActivityDescriptor { get; }
        DateTime StartDate { get; }
        DateTime EndDate { get; }
        Distribution RecomputedEfficiency { get; }
    }

}
