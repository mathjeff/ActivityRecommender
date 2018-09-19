using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// a PlannedExperiment represents an intent to do two Participations, each measured by a corresponding Metric, and compare the results
// a PlannedExperiment is a prerequisite to obtaining an EffectivenessMeasurement
namespace ActivityRecommendation.Effectiveness
{
    public class PlannedExperiment
    {
        public ExperimentSuggestion Earlier { get; set; }
        public ExperimentSuggestion Later { get; set; }
        public Participation FirstParticipation { get; set; }

        // returns the ActivitySuggestion that is recommended to be done next as part of this experiment
        public ActivitySuggestion NextIncompleteSuggestion
        {
            get
            {
                if (this.FirstParticipation == null)
                    return this.Earlier.ActivitySuggestion;
                return this.Later.ActivitySuggestion;
            }
        }
    }

    public class ExperimentSuggestion
    {
        public ActivitySuggestion ActivitySuggestion { get; set; }

        public Metric Metric { get; set; }

        // an estimate of Metric.getScore(participation) / participation.Duration.TotalSeconds
        public double EstimatedSuccessesPerSecond { get; set; }

        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                return this.ActivitySuggestion.ActivityDescriptor;
            }
        }

    }

    public class ExperimentSuggestionOrError
    {
        public ExperimentSuggestionOrError(ExperimentSuggestion suggestion) { this.ExperimentSuggestion = suggestion; }
        public ExperimentSuggestionOrError(string error) { this.Error = error; }

        public ExperimentSuggestion ExperimentSuggestion { get; set; }
        public string Error = "";
    }
}
