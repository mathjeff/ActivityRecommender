using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// a PlannedExperiment represents an intent to do two Participations, each measured by a corresponding Metric, and compare the results
// a PlannedExperiment is a prerequisite to obtaining an EffectivenessMeasurement
namespace ActivityRecommendation.Effectiveness
{
    // Contains a plan to do two activitin a particular order and how to measure them
    public class PlannedExperiment
    {
        public PlannedMetric Earlier { get; set; }
        public PlannedMetric Later { get; set; }
        public Participation FirstParticipation { get; set; }

        public bool InProgress
        {
            get
            {
                return this.FirstParticipation != null;
            }
        }
    }

    // A plan to do a certain Activity, along with how to measure its success
    public class PlannedMetric
    {
        public ActivityDescriptor ActivityDescriptor { get; set; }

        public string MetricName { get; set; }

        // an estimate of Metric.getScore(participation) / participation.Duration.TotalSeconds
        public double EstimatedSuccessesPerSecond { get; set; }
    }

    // Suggests doing a certain activity and measuring it in a certain way (unless it contains an error)
    public class SuggestedMetric
    {
        public SuggestedMetric(PlannedMetric metric, ActivitySuggestion activitySuggestion) { this.PlannedMetric = metric; this.ActivitySuggestion = activitySuggestion; }
        public PlannedMetric PlannedMetric { get; set; }
        public ActivitySuggestion ActivitySuggestion { get; set; }

        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                return this.ActivitySuggestion.ActivityDescriptor;
            }
        }
    }

    // holds a SuggestedMetric and some information about the process of creating it
    public class SuggestedMetric_Metadata
    {
        public SuggestedMetric_Metadata(SuggestedMetric Content, int numExperimentParticipationsRemaining)
        {
            this.Content = Content;
            this.NumExperimentParticipationsRemaining = numExperimentParticipationsRemaining;
        }
        public SuggestedMetric_Metadata(string error) { this.Error = error; }

        // Holds an error if creating a SuggestedMetric failed. If this is nonempty, the other fields will be empty.
        public string Error = "";
        // The actual suggestion
        public SuggestedMetric Content;
        // The number of times that an experiment participation can happen before no longer having enough tasks (which is required for making comparisons that don't have too much error)
        public int NumExperimentParticipationsRemaining;

        public ActivitySuggestion ActivitySuggestion
        {
            get
            {
                return this.Content.ActivitySuggestion;
            }
        }

        public PlannedMetric PlannedMetric
        {
            get
            {
                return this.Content.PlannedMetric;
            }
        }
    }

    // Embodies a suggestion to run an experiment
    public class ExperimentSuggestion
    {
        public ExperimentSuggestion(PlannedExperiment experiment, ActivitySuggestion activitySuggestion) { this.Experiment = experiment; this.ActivitySuggestion = activitySuggestion; }

        public ActivitySuggestion ActivitySuggestion { get; set; }
        public PlannedExperiment Experiment { get; set; }
    }
}
