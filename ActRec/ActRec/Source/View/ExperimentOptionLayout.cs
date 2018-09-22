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

        public ExperimentOptionLayout(ExperimentInitializationLayout owner)
        {
            this.owner = owner;
            Button suggestButton = new Button();
            this.suggestButtonLayout = new ButtonLayout(suggestButton, "?");
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

                    this.SubLayout = suggestionLayout;
                }
                else
                {
                    this.SubLayout = this.suggestButtonLayout;
                }
            }
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
    }

    class ExperimentSuggestionLayout : ContainerLayout
    {
        public event SuggestionDismissedHandler SuggestionDismissed;
        public delegate void SuggestionDismissedHandler();

        public ExperimentSuggestionLayout(SuggestedMetric suggestion)
        {
            this.CancelButton = new Button();
            this.CancelButton.Clicked += CancelButton_Clicked;

            GridLayout grid = new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(new TextblockLayout(suggestion.ActivityDescriptor.ActivityName))
                .AddLayout(new TextblockLayout("P:" + suggestion.ActivitySuggestion.ParticipationProbability))
                .AddLayout(new ButtonLayout(this.CancelButton, "X"))
                .Build();
            this.SubLayout = grid;
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            if (this.SuggestionDismissed != null)
            {
                this.SuggestionDismissed.Invoke();
            }
        }

        Button CancelButton;
    }
}
