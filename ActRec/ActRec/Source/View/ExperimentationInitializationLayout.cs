using ActivityRecommendation;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class ExperimentationInitializationLayout : TitledControl
    {
        public ExperimentationInitializationLayout(LayoutStack layoutStack)
        {
            this.SetTitle("Experiment");

            Button okbutton = new Button();
            ButtonLayout okButtonLayout = new ButtonLayout(okbutton, "Accept");

            LayoutChoice_Set helpButton = this.make_helpButton(layoutStack);

            this.candidatesLayout = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(3), LayoutScore.Zero);

            GridLayout topGrid = new Horizontal_GridLayout_Builder().AddLayout(helpButton).AddLayout(okButtonLayout).Uniform().Build();

            LayoutChoice_Set warningLayout = new TextblockLayout("This page is still a work in progress and doesn't work yet.");

            GridLayout mainGrid = new Vertical_GridLayout_Builder().AddLayout(warningLayout).AddLayout(topGrid).AddLayout(this.candidatesLayout).Uniform().Build();

            this.SetContent(mainGrid);
        }

        public void AddSuggestion(ActivitySuggestion suggestion)
        {
            this.suggestions.Add(suggestion);
            this.candidatesLayout.AddLayout(new SuggestionView(suggestion));
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
        private GridLayout candidatesLayout;
        private List<ActivitySuggestion> suggestions = new List<ActivitySuggestion>();

    }
}
