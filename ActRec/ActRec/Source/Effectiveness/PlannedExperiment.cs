using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

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

        public DifficultyEstimate DifficultyEstimate = new DifficultyEstimate();
    }

    public class DifficultyEstimate
    {
        // an estimate of Metric.getScore(participation) / participation.Duration.TotalSeconds, using information about the activity itself but without asking the user
        public double EstimatedSuccessesPerSecond_WithoutUser { get; set; }

        // an estimate of Metric.getScore(participation) / participation.Duration.TotalSeconds, using information about the activity and also consulting the user
        public double EstimatedSuccessesPerSecond { get; set; }

        // the number of activities that the user said were harder than this one, from among the list of other activities being considered at the same time
        public int NumHarders { get; set; }
        // the number of activities that the user said were easier than this one, from among the list of other activities being considered at the same time
        public int NumEasiers { get; set; }
    }

    // Suggests doing a certain activity and measuring it in a certain way (unless it contains an error)
    public class SuggestedMetric
    {
        public SuggestedMetric(PlannedMetric metric, ActivitySuggestion activitySuggestion, bool chosenByUser)
{
            this.PlannedMetric = metric;
            this.ActivitySuggestion = activitySuggestion;
            this.ChosenByUser = chosenByUser;
        }
        public PlannedMetric PlannedMetric { get; set; }
        public ActivitySuggestion ActivitySuggestion { get; set; }
        public bool ChosenByUser { get; set; }

        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                return this.ActivitySuggestion.ActivityDescriptor;
            }
        }
    }

    public class SuggestedMetric_Renderer : LayoutProvider<SuggestedMetric>
    {
        public static SuggestedMetric_Renderer Instance = new SuggestedMetric_Renderer();
        public LayoutChoice_Set GetLayout(SuggestedMetric metric)
        {
            BoundProperty_List columnWidths = new BoundProperty_List(2);
            columnWidths.BindIndices(0, 1);
            columnWidths.SetPropertyScale(0, 5);
            columnWidths.SetPropertyScale(1, 2);
            GridLayout gridLayout = GridLayout.New(new BoundProperty_List(1), columnWidths, LayoutScore.Zero);
            gridLayout.AddLayout(new TextblockLayout(metric.ActivityDescriptor.ActivityName));
            gridLayout.AddLayout(new TextblockLayout(metric.PlannedMetric.MetricName));
            return gridLayout;

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
