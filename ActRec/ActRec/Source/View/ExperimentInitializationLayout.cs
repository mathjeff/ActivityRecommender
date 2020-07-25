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

        public ExperimentInitializationLayout(LayoutStack layoutStack, ActivityRecommender activityRecommender, ActivityDatabase activityDatabase, Engine engine, int numActivitiesThatMayBeRequestedDirectly)
        {
            this.SetTitle("Experiment");
            this.activityRecommender = activityRecommender;

            Button okbutton = new Button();
            this.okButtonLayout = new ButtonLayout(okbutton, "Next");
            okbutton.Clicked += Okbutton_Clicked;

            LayoutChoice_Set helpButton = this.make_helpButton(layoutStack);

            SuggestedMetric_Metadata experimentsStatus = activityRecommender.Test_ChooseExperimentOption();
            if (experimentsStatus.Error != "")
            {
                this.SetContent(new TextblockLayout(experimentsStatus.Error));
                return;
            }

            this.statusHolder = new ContainerLayout();
            GridLayout topGrid = new Horizontal_GridLayout_Builder()
                .AddLayout(helpButton)
                .AddLayout(new HelpButtonLayout("List Open ToDos", new ListOpenTodosView(activityDatabase), layoutStack))
                .Uniform()
                .Build();

            Horizontal_GridLayout_Builder childrenBuilder = new Horizontal_GridLayout_Builder().Uniform();
            for (int i = 0; i < this.numChoices; i++)
            {
                bool allowRequestingActivityDirectly = (i < numActivitiesThatMayBeRequestedDirectly);
                ExperimentOptionLayout child = new ExperimentOptionLayout(this, activityDatabase, allowRequestingActivityDirectly, engine, layoutStack);
                this.children.Add(child);
                childrenBuilder.AddLayout(child);
                child.SuggestionDismissed += Child_SuggestionDismissed;
                child.JustifySuggestion += Child_JustifySuggestion;
            }
            GridLayout bottomGrid = childrenBuilder.Build();

            BoundProperty_List rowHeights = new BoundProperty_List(3);
            rowHeights.BindIndices(0, 1);
            rowHeights.BindIndices(0, 2);
            rowHeights.SetPropertyScale(0, 2);
            rowHeights.SetPropertyScale(1, 1);
            rowHeights.SetPropertyScale(2, 6);

            GridLayout mainGrid = GridLayout.New(rowHeights, new BoundProperty_List(1), LayoutScore.Zero);
            mainGrid.AddLayout(topGrid);
            mainGrid.AddLayout(this.statusHolder);
            mainGrid.AddLayout(bottomGrid);

            string statusMessage = "(" + experimentsStatus.NumExperimentParticipationsRemaining + " experiment";
            if (experimentsStatus.NumExperimentParticipationsRemaining != 1)
                statusMessage += "s";
            statusMessage += " remaining before another ToDo must be entered!)";
            this.UpdateStatus(statusMessage);

            this.SetContent(mainGrid);
        }

        private void Child_JustifySuggestion(ActivitySuggestion suggestion)
        {
            this.activityRecommender.JustifySuggestion(suggestion);
        }

        private void Child_SuggestionDismissed(ActivitySuggestion suggestion)
        {
            ActivitySkip skip = this.activityRecommender.DeclineSuggestion(suggestion);
            double numSecondsThinking = skip.ThinkingTime.TotalSeconds;
            this.UpdateStatus("Recorded " + (int)numSecondsThinking + " seconds wasted");
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

        public SuggestedMetric_Metadata ChooseExperimentOption(ActivityRequest activityRequest)
        {
            SuggestedMetric_Metadata result = this.activityRecommender.ChooseExperimentOption(activityRequest, this.Suggestions);
            if (result.Error != "")
                this.UpdateStatus(result.Error);
            return result;
        }
        public void UpdateStatus(string errorMessage = "")
        {
            if (errorMessage == "")
            {
                int numExtraSuggestionsNeeded = this.numChoices - this.Suggestions.Count;
                if (numExtraSuggestionsNeeded > 0)
                {
                    string text = "Choose " + numExtraSuggestionsNeeded + " suggestion";
                    if (numExtraSuggestionsNeeded > 1)
                        text += "s";
                    this.statusHolder.SubLayout = new TextblockLayout(text);
                }
                else
                {
                    this.statusHolder.SubLayout = this.okButtonLayout;
                }
            }
            else
            {
                this.statusHolder.SubLayout = new TextblockLayout(errorMessage);
            }
        }
        public Participation LatestParticipation
        {
            set
            {
                foreach (ExperimentOptionLayout child in this.children)
                {
                    child.LatestParticipation = value;
                }
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
                .AddMessage("These experiments somewhat randomize the order in which you participate in certain activities, which allows you to compare your efficiency " +
                "across those participations.")
                .AddMessage("For example, if on some nights you go to bed early and on other nights you go to bed late, experimentation can enable ActivityRecommender " +
                "to measure how sleep affects your ability to quickly you get your work done.")
                .AddMessage("This is possible even if you don't have any two tasks of the same difficulty, because by randomizing the ordering of the tasks, after taking " +
                "enough measurements, the random variations in difficulty should eventually approximately cancel each other out.")
                .AddMessage("What you should do first is to push each of the Suggest buttons, to create some experiment options.")
                .AddMessage("(You can customize the suggestions via the other fields, if you like.)")
                .AddMessage("Next, determine whether any of the provided suggestions are any that you are unwilling to attempt " +
                "doing right now.")
                .AddMessage("(Note that it's ok if they're too difficult to finish in one sitting; you just need to be willing to make an attempt.)")
                .AddMessage("If there are any you are unwilling to attempt right now, then press the coresponding X button.")
                .AddMessage("Then, replace the dismissed suggestions with new suggestions, and repeat until you're satisfied.")
                .AddMessage("Note that like the usual suggestions screen, ActivityRecommender might make the same suggestions a few times if it's confident in its " +
                "suggestion, so you may have to dismiss the same suggestion several times if you're sure that you don't want it.")
                .AddMessage("If you can't find " + this.numChoices + " suggestions that you're satisfied with, then at this point it's still ok to go back.")
                .AddMessage("Once you're satisfied with the given candidates, accept the experiment.")
                .AddMessage("One of the visible tasks will then be chosen randomly, and you will be instructed to work on that task until you either complete it or you " +
                "give up.")
                .AddMessage("Note that it's very important to focus on this task so ActivityRecommender can have accurate data about how much time you actually spent on it.")
                .AddMessage("As a result, ActivityRecommender will not allow you to dismiss the resultant suggestion; you will have to attempt to work on it.")
                .AddMessage("On subsequent experiments, you may also notice that when you are deciding which activities to choose from, some of the boxes may " +
                "allow you to type in exactly which activity you want to do, and some only say 'Suggest'. You're only allowed to directly enter an average of " +
                "one activity suggestion per experiment (but you may cancel suggestions as many times as you want (but this may take longer)); the more " +
                "activity names that you directly choose this time, the fewer that you will be allowed to directly choose next time. This is done to prevent you " +
                "from accidentally ordering activities in a way such that ActivityRecommender can't analyze efficiency trends over long periods of time.")
                // Specifically, we want to prevent the user from finishing all of their post-tasks before creating enough new pre-tasks
                .AddMessage("Ready? Go!")
                .AddLayout(new CreditsButtonBuilder(layoutStack)
                    .AddContribution(ActRecContributor.CORY_JALBERT, new System.DateTime(2017, 12, 14), "Suggested adding the ability to quantify more things about yourself than just happiness")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new System.DateTime(2019, 11, 2), "Pointed out that the experiment screen crashed for new users")
                    // TODO: move this contribution inside the experiment difficulty selection screen?
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new System.DateTime(2019, 11, 28), "Discussed the experiment difficulty selection screen and the addition of the 2*A difficulty entry")
                    .Build()
                 )
                .Build();
            return new HelpButtonLayout("Important Instructions", helpDetails, layoutStack);
        }

        private int numChoices = 3;
        private List<ExperimentOptionLayout> children = new List<ExperimentOptionLayout>();
        private ActivityRecommender activityRecommender;
        private ContainerLayout statusHolder;
        private LayoutChoice_Set okButtonLayout;


    }
}
