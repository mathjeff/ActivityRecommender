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

        public ExperimentOptionLayout(ExperimentInitializationLayout owner, ActivityDatabase activityDatabase, bool allowRequestingActivitiesDirectly, Engine engine, LayoutStack layoutStack)
        {
            this.owner = owner;

            RequestSuggestion_Layout requestSuggestion_layout = new RequestSuggestion_Layout(activityDatabase, allowRequestingActivitiesDirectly, false, true, 1, engine, layoutStack);
            requestSuggestion_layout.RequestSuggestion += RequestSuggestion_Impl;
            this.requestSuggestion_layout = requestSuggestion_layout;
            this.Suggestion = null;
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
                    this.SubLayout = this.requestSuggestion_layout;
                }
            }
        }

        public Participation LatestParticipation
        {
            set
            {
                this.requestSuggestion_layout.LatestParticipation = value;
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

        private void RequestSuggestion_Impl(ActivityRequest activityRequest)
        {
            SuggestedMetric_Metadata result = this.owner.ChooseExperimentOption(activityRequest);
            if (result.Content != null)
            {
                this.Suggestion = result.Content;
                this.owner.UpdateStatus();
            }
        }

        private RequestSuggestion_Layout requestSuggestion_layout;
        private ExperimentInitializationLayout owner;
        private SuggestedMetric suggestion;
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
