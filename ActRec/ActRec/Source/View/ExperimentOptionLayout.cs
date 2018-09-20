﻿using ActivityRecommendation.Effectiveness;
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
        public ExperimentOptionLayout(ExperimentInitializationLayout owner)
        {
            this.owner = owner;
            Button suggestButton = new Button();
            this.suggestButtonLayout = new ButtonLayout(suggestButton, "?");
            this.Suggestion = null;
            suggestButton.Clicked += SuggestButton_Clicked;
        }

        public ExperimentSuggestion Suggestion
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
                    suggestionLayout.SuggestionCancelled += SuggestionLayout_SuggestionDismissed;

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
            this.Suggestion = null;
        }

        private void SuggestButton_Clicked(object sender, EventArgs e)
        {
            this.Suggestion = this.owner.ChooseExperimentOption();
        }

        private ButtonLayout suggestButtonLayout;
        private ExperimentInitializationLayout owner;
        private ExperimentSuggestion suggestion;
    }

    class ExperimentSuggestionLayout : ContainerLayout
    {
        public event SuggestionDismissedHandler SuggestionCancelled;
        public delegate void SuggestionDismissedHandler();

        public ExperimentSuggestionLayout(ExperimentSuggestion suggestion)
        {
            this.CancelButton = new Button();
            this.CancelButton.Clicked += CancelButton_Clicked;

            GridLayout grid = new Vertical_GridLayout_Builder()
                .AddLayout(new TextblockLayout(suggestion.ActivityDescriptor.ActivityName))
                .AddLayout(new ButtonLayout(this.CancelButton, "X"))
                .Build();
            this.SubLayout = grid;
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            if (this.SuggestionCancelled != null)
            {
                this.SuggestionCancelled.Invoke();
            }
        }

        Button CancelButton;
    }
}