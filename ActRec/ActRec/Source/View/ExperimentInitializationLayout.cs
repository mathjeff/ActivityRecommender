﻿using ActivityRecommendation;
using ActivityRecommendation.Effectiveness;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class ExperimentInitializationLayout : TitledControl
    {
        public event RequestedExperimentHandler RequestedExperiment;
        public delegate void RequestedExperimentHandler(List<ExperimentSuggestion> choices);

        public ExperimentInitializationLayout(LayoutStack layoutStack, ActivityRecommender activityRecommender)
        {
            this.SetTitle("Experiment");
            this.activityRecommender = activityRecommender;

            Button okbutton = new Button();
            ButtonLayout okButtonLayout = new ButtonLayout(okbutton, "Accept");
            okbutton.Clicked += Okbutton_Clicked;

            LayoutChoice_Set helpButton = this.make_helpButton(layoutStack);

            string error = activityRecommender.Test_ChooseExperimentOption();
            if (error != "")
            {
                this.SetContent(new TextblockLayout(error));
                return;
            }

            GridLayout topGrid = new Horizontal_GridLayout_Builder().AddLayout(helpButton).AddLayout(okButtonLayout).Uniform().Build();

            Horizontal_GridLayout_Builder childrenBuilder = new Horizontal_GridLayout_Builder().Uniform();
            for (int i = 0; i < this.numChoices; i++)
            {
                ExperimentOptionLayout child = new ExperimentOptionLayout(this);
                this.children.Add(child);
                childrenBuilder.AddLayout(child);
            }
            GridLayout bottomGrid = childrenBuilder.Build();

            GridLayout mainGrid = new Vertical_GridLayout_Builder().AddLayout(topGrid).AddLayout(bottomGrid).Uniform().Build();

            this.SetContent(mainGrid);
        }

        private void Okbutton_Clicked(object sender, System.EventArgs e)
        {
            List<ExperimentSuggestion> suggestions = this.Suggestions;
            // confirm that all slots have suggestions visible
            if (suggestions.Count == this.numChoices)
            {
                this.RequestedExperiment.Invoke(suggestions);
            }
        }

        public ExperimentSuggestion ChooseExperimentOption()
        {
            ExperimentSuggestionOrError result = this.activityRecommender.ChooseExperimentOption(this.Suggestions);
            if (result.Error != "")
            {
                this.SetContent(new TextblockLayout("Internal error; ChooseExperimentOption returned error '" + result.Error + "' after Test_ChooseExperimentOption succeeded"));
                return null;
            }
            return result.ExperimentSuggestion;
        }
        private List<ExperimentSuggestion> Suggestions
        {
            get
            {
                List<ExperimentSuggestion> result = new List<ExperimentSuggestion>();
                foreach (ExperimentOptionLayout child in this.children)
                {
                    if (child.Suggestion != null)
                        result.Add(child.Suggestion);
                }
                return result;
            }
        }

        private LayoutChoice_Set make_helpButton(LayoutStack layoutStack)
        {
            LayoutChoice_Set helpDetails = new HelpWindowBuilder()
                .AddMessage("Use this screen to start an experiment.")
                .AddMessage("These experiments somewhat randomize the order in which you participate in certain activities, which allows you to compare your efficiency" +
                " across those participations.")
                .AddMessage("For example, if on some nights you go to bed early and on other nights you go to bed late, experimentation can enable ActivityRecommender" +
                " to measure how sleep affects your ability to quickly you get your work done.")
                .AddMessage("This is possible even if you don't have any two tasks of the same difficulty, because by randomizing the ordering of the tasks, after taking" +
                " enough measurements, the random variations in difficulty should eventually approximately cancel each other out.")
                .AddMessage("What you have to do, first, is to look at the list of possible suggestions, and determine whether there are any that you are unwilling to attempt" +
                " doing right now.")
                .AddMessage("(Note that it's ok if they're too difficult to finish in one sitting; you just need to be willing to make an attempt.)")
                .AddMessage("If there are any you are unwilling to attempt right now, then press the coresponding X button.")
                .AddMessage("Then, replace the dismissed suggestions with new suggestions, and repeat until you're satisfied.")
                .AddMessage("Note that like the usual suggestions screen, ActivityRecommender might make the same suggestions a few times if it's confident in its" +
                " suggestion, so you may have to dismiss the same suggestion several times if you're sure that you don't want it.")
                .AddMessage("If you can't find " + this.numChoices + " suggestions that you're satisfied with, then at this point it's still ok to go back.")
                .AddMessage("Once you're satisfied with the given candidates, accept the experiment.")
                .AddMessage("One of the visible tasks will then be chosen randomly, and you will be instructed to work on that task until you either complete it or you" +
                " give up.")
                .AddMessage("Note that it's very important to focus on this task so ActivityRecommender can have accurate data about how much time you actually spent on it.")
                .AddMessage("As a result, ActivityRecommender will not allow you to navigate to other screens before you declare having stopped working on this task.")
                .AddMessage("Ready? Go!")
                .Build();
            return new HelpButtonLayout("Important Instructions", helpDetails, layoutStack);
        }

        private int numChoices = 3;
        List<ExperimentOptionLayout> children = new List<ExperimentOptionLayout>();
        ActivityRecommender activityRecommender;

    }
}