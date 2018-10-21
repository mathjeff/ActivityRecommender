using ActivityRecommendation;
using ActivityRecommendation.Effectiveness;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class ExperimentInitializationLayout : TitledControl
    {
        public event RequestedExperimentHandler RequestedExperiment;
        public delegate void RequestedExperimentHandler(List<SuggestedMetric> choices);

        public ExperimentInitializationLayout(LayoutStack layoutStack, ActivityRecommender activityRecommender, ActivityDatabase activityDatabase)
        {
            this.SetTitle("Experiment");
            this.activityRecommender = activityRecommender;

            Button okbutton = new Button();
            this.okButtonLayout = new ButtonLayout(okbutton, "Accept");
            okbutton.Clicked += Okbutton_Clicked;

            LayoutChoice_Set helpButton = this.make_helpButton(layoutStack);

            string error = activityRecommender.Test_ChooseExperimentOption();
            if (error != "")
            {
                this.SetContent(new TextblockLayout(error));
                return;
            }

            this.okButtonHolder = new ContainerLayout();
            GridLayout topGrid = new Horizontal_GridLayout_Builder().AddLayout(helpButton).AddLayout(this.okButtonHolder).Uniform().Build();

            Horizontal_GridLayout_Builder childrenBuilder = new Horizontal_GridLayout_Builder().Uniform();
            for (int i = 0; i < this.numChoices; i++)
            {
                bool allowRequestingActivitiesDirectly = (i == 0);
                ExperimentOptionLayout child = new ExperimentOptionLayout(this, activityDatabase, allowRequestingActivitiesDirectly);
                this.children.Add(child);
                childrenBuilder.AddLayout(child);
                child.SuggestionDismissed += Child_SuggestionDismissed;
                child.JustifySuggestion += Child_JustifySuggestion;
            }
            GridLayout bottomGrid = childrenBuilder.Build();

            GridLayout mainGrid = new Vertical_GridLayout_Builder().AddLayout(topGrid).AddLayout(bottomGrid).Uniform().Build();

            this.UpdateOkButton();

            this.SetContent(mainGrid);
        }

        private void Child_JustifySuggestion(ActivitySuggestion suggestion)
        {
            this.activityRecommender.JustifySuggestion(suggestion);
        }

        private void Child_SuggestionDismissed(ActivitySuggestion suggestion)
        {
            this.activityRecommender.DeclineSuggestion(suggestion);
            this.UpdateOkButton();
        }

        private void Okbutton_Clicked(object sender, System.EventArgs e)
        {
            List<SuggestedMetric> suggestions = this.Suggestions;
            // confirm that all slots have suggestions visible
            if (suggestions.Count == this.numChoices)
            {
                this.RequestedExperiment.Invoke(suggestions);
            }
        }

        public SuggestedMetricOrError ChooseExperimentOption(ActivityRequest activityRequest)
        {
            SuggestedMetricOrError result = this.activityRecommender.ChooseExperimentOption(activityRequest, this.Suggestions);
            if (result.Error != "")
                this.UpdateOkButton(result.Error);
            return result;
        }
        public void UpdateOkButton(string errorMessage = "")
        {
            if (errorMessage == "")
            {
                int numExtraSuggestionsNeeded = this.numChoices - this.Suggestions.Count;
                if (numExtraSuggestionsNeeded > 0)
                {
                    string text = "Choose " + numExtraSuggestionsNeeded + " suggestion";
                    if (numExtraSuggestionsNeeded > 1)
                        text += "s";
                    this.okButtonHolder.SubLayout = new TextblockLayout(text);
                }
                else
                {
                    this.okButtonHolder.SubLayout = this.okButtonLayout;
                }
            }
            else
            {
                this.okButtonHolder.SubLayout = new TextblockLayout(errorMessage);
            }
        }
        private List<SuggestedMetric> Suggestions
        {
            get
            {
                List<SuggestedMetric> result = new List<SuggestedMetric>();
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
                .AddMessage("As a result, ActivityRecommender will not allow you to dismiss the resultant suggestion; you will have to attempt to work on it.")
                .AddMessage("Ready? Go!")
                .Build();
            return new HelpButtonLayout("Important Instructions", helpDetails, layoutStack);
        }

        private int numChoices = 3;
        private List<ExperimentOptionLayout> children = new List<ExperimentOptionLayout>();
        private ActivityRecommender activityRecommender;
        private ContainerLayout okButtonHolder;
        private LayoutChoice_Set okButtonLayout;


    }
}
