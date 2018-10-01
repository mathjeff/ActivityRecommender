using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    // an ExperimentOptionLayout is one of the entries in an ExperimentationInitializationLayout
    class ExperimentOptionLayout : ContainerLayout
    {
        public event SuggestionDismissedHandler SuggestionDismissed;
        public delegate void SuggestionDismissedHandler(ActivitySuggestion suggestion);

        public event JustifySuggestionHandler JustifySuggestion;
        public delegate void JustifySuggestionHandler(ActivitySuggestion suggestion);

        public ExperimentOptionLayout(ExperimentInitializationLayout owner)
        {
            this.owner = owner;
            
            this.suggestButtonLayout = new ButtonLayout(suggestButton, "Suggest");
            this.Suggestion = null;
            suggestButton.Clicked += SuggestButton_Clicked;
        }

        public SuggestedMetric Suggestion
        {
            get
            {
                return this.suggestion;
            }
            set
            {
                this.suggestion = value;
                if (suggestion != null)
                {
                    ExperimentSuggestionLayout suggestionLayout = new ExperimentSuggestionLayout(suggestion);
                    suggestionLayout.SuggestionDismissed += SuggestionLayout_SuggestionDismissed;
                    suggestionLayout.JustifySuggestion += SuggestionLayout_JustifySuggestion;

                    this.SubLayout = suggestionLayout;
                }
                else
                {
                    this.SubLayout = this.suggestButtonLayout;
                }
            }
        }

        private void SuggestionLayout_JustifySuggestion()
        {
            this.JustifySuggestion.Invoke(this.suggestion.ActivitySuggestion);
        }

        private void SuggestionLayout_SuggestionDismissed()
        {
            this.SuggestionDismissed.Invoke(this.Suggestion.ActivitySuggestion);
            this.Suggestion = null;
        }

        private void SuggestButton_Clicked(object sender, EventArgs e)
        {
            SuggestedMetricOrError result = this.owner.ChooseExperimentOption();
            this.Suggestion = result.Content;
        }

        private ButtonLayout suggestButtonLayout;
        private ExperimentInitializationLayout owner;
        private SuggestedMetric suggestion;
        private Button suggestButton = new Button();
    }

    class ExperimentSuggestionLayout : ContainerLayout
    {
        public event SuggestionDismissedHandler SuggestionDismissed;
        public delegate void SuggestionDismissedHandler();

        public event JustifySuggestionHandler JustifySuggestion;
        public delegate void JustifySuggestionHandler();

        public ExperimentSuggestionLayout(SuggestedMetric suggestion)
        {
            this.CancelButton.Clicked += CancelButton_Clicked;
            this.JustifyButton.Clicked += JustifyButton_Clicked;
            
            GridLayout grid = new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(new TextblockLayout(suggestion.ActivityDescriptor.ActivityName))
                .AddLayout(new TextblockLayout("(" + suggestion.PlannedMetric.MetricName + ")"))
                .AddLayout(new TextblockLayout("P:" + suggestion.ActivitySuggestion.ParticipationProbability))
                .AddLayout(new Horizontal_GridLayout_Builder().Uniform()
                    .AddLayout(new ButtonLayout(this.CancelButton, "X"))
                    .AddLayout(new ButtonLayout(this.JustifyButton, "?"))
                    .Build())
                .Build();
            this.SubLayout = grid;
        }

        private void JustifyButton_Clicked(object sender, EventArgs e)
        {
            this.JustifySuggestion.Invoke();            
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            if (this.SuggestionDismissed != null)
            {
                this.SuggestionDismissed.Invoke();
            }
        }

        private Button CancelButton = new Button();
        private Button JustifyButton = new Button();
    }
}
