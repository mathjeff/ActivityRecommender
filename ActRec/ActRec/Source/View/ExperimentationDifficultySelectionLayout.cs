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
    // An ExperimentationDifficultySelectionLayout asks the user for some information about the relative difficulties of several activities
    // This is supposed to happen after the user has identified several plausible activities to include in an experiment but before one specific activity has been chosen to do now
    class ExperimentationDifficultySelectionLayout : ContainerLayout
    {
        public event RequestedExperimentHandler Done;
        public delegate void RequestedExperimentHandler(List<SuggestedMetric> choices);

        public ExperimentationDifficultySelectionLayout(List<SuggestedMetric> choices)
        {
            string instructions = "Rearrange these tasks so they appear in order by increasing difficulty (easiest task at the top).";

            Button okButton = new Button();
            okButton.Clicked += OkButton_Clicked;
            ButtonLayout okButtonLayout = new ButtonLayout(okButton, "Accept");

            this.choicesLayout = new ReorderableList<SuggestedMetric>(choices, SuggestedMetric_Renderer.Instance);


            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();
            gridBuilder.AddLayout(new TextblockLayout(instructions));
            gridBuilder.AddLayout(okButtonLayout);
            gridBuilder.AddLayout(this.choicesLayout);
            this.SubLayout = gridBuilder.Build();
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            this.submit(this.choicesLayout.Items);
        }

        private void submit(List<SuggestedMetric> reorderedItems)
        {
            for (int i = 0; i < reorderedItems.Count; i++)
            {
                reorderedItems[i].PlannedMetric.DifficultyEstimate.NumEasiers = i;
                reorderedItems[i].PlannedMetric.DifficultyEstimate.NumHarders = reorderedItems.Count - 1 - i;
            }
            this.Done.Invoke(reorderedItems);
        }

        private ReorderableList<SuggestedMetric> choicesLayout;

    }
}