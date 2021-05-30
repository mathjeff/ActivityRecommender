using System;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

// a SuggestionView displays one suggested Activity and some details of the suggestion
namespace ActivityRecommendation
{
    class SuggestionView : ContainerLayout
    {
        public event SuggestionDismissed Dismissed;
        public delegate void SuggestionDismissed(ActivitiesSuggestion suggestion);

        public event RequestedExperiment ExperimentRequested;
        public delegate void RequestedExperiment(ActivitySuggestion suggestion);

        public event JustifySuggestionHandler JustifySuggestion;
        public delegate void JustifySuggestionHandler(ActivitySuggestion suggestion);

        public event VisitParticipationScreenHandler AcceptedSuggestion;
        public delegate void VisitParticipationScreenHandler(ActivitySuggestion suggestion);

        public SuggestionView(ActivitiesSuggestion suggestion, bool isFirstSuggestion, Dictionary<ActivitySuggestion, bool> repeatingDeclinedSuggestion, LayoutStack layoutStack)
        {
            this.suggestion = suggestion;
            this.layoutStack = layoutStack;


            bool allWorseThanAverage = true;
            foreach (ActivitySuggestion child in suggestion.Children)
            {
                if (!child.WorseThanRootActivity)
                    allWorseThanAverage = false;
            }

            GridLayout_Builder fullBuilder = new Vertical_GridLayout_Builder();
            string startTimeText = suggestion.Children[0].StartDate.ToString("HH:mm");

            if (allWorseThanAverage && suggestion.Skippable)
                fullBuilder.AddLayout(new TextblockLayout("" + suggestion.Children.Count + " best ideas at " + startTimeText + ". How about a new activity?", 24).AlignHorizontally(TextAlignment.Center));
            else
                fullBuilder.AddLayout(new TextblockLayout("" + suggestion.Children.Count + " ideas at " + startTimeText + ":", 24).AlignHorizontally(TextAlignment.Center));

            List<GridLayout> optionsGrids = new List<GridLayout>();
            this.explainButtons = new Dictionary<Button, ActivitySuggestion>();
            this.doButtons = new Dictionary<Button, ActivitySuggestion>();
            for (int mainFontSize = 20; mainFontSize >= 12; mainFontSize -= 8)
            {
                GridLayout optionsGrid = GridLayout.New(new BoundProperty_List(3), BoundProperty_List.Uniform(suggestion.Children.Count), LayoutScore.Zero);

                for (int i = 0; i < suggestion.Children.Count; i++)
                {
                    ActivitySuggestion child = suggestion.Children[i];

                    // set up the options for the text
                    string mainText = this.summarize(child, repeatingDeclinedSuggestion[child]);
                    TextblockLayout mainBlock = new TextblockLayout(mainText, mainFontSize);
                    TextAlignment horizontalAlignment;
                    TextAlignment verticalAlignment;
                    if (i == 0)
                    {
                        horizontalAlignment = TextAlignment.Start;
                        if (suggestion.Children.Count == 1)
                            verticalAlignment = TextAlignment.Start;
                        else
                            verticalAlignment = TextAlignment.End;
                    }
                    else
                    {
                        if (i == suggestion.Children.Count - 1)
                        {
                            horizontalAlignment = TextAlignment.End;
                            verticalAlignment = TextAlignment.Start;
                        }
                        else
                        {
                            horizontalAlignment = TextAlignment.Center;
                            verticalAlignment = TextAlignment.Center;
                        }
                    }
                    mainBlock.AlignHorizontally(horizontalAlignment);
                    mainBlock.AlignVertically(verticalAlignment);
                    optionsGrid.PutLayout(mainBlock, i, 0);

                    // set up the buttons
                    GridLayout_Builder buttonsBuilder = new Horizontal_GridLayout_Builder().Uniform();
                    double buttonFontSize = mainFontSize * 0.9;
                    // make a doNow button if needed
                    if (isFirstSuggestion)
                    {
                        Button doNowButton = new Button();
                        doNowButton.Clicked += DoNowButton_Clicked;
                        this.doButtons[doNowButton] = child;
                        ButtonLayout doButtonLayout = new ButtonLayout(doNowButton, "OK", buttonFontSize);
                        buttonsBuilder.AddLayout(doButtonLayout);
                    }
                    if (child.PredictedScoreDividedByAverage != null)
                    {
                        Button explainButton = new Button();
                        explainButton.Clicked += explainButton_Clicked;
                        this.explainButtons[explainButton] = child;
                        ButtonLayout explainLayout = new ButtonLayout(explainButton, "?", buttonFontSize);
                        buttonsBuilder.AddLayout(explainLayout);
                    }
                    optionsGrid.PutLayout(buttonsBuilder.BuildAnyLayout(), i, 1);
                    if (child.ExpectedReaction != null)
                    {
                        TextblockLayout reactionLayout = new TextblockLayout(child.ExpectedReaction, buttonFontSize * 0.9);
                        reactionLayout.AlignHorizontally(horizontalAlignment);
                        reactionLayout.AlignVertically(verticalAlignment);
                        optionsGrid.PutLayout(reactionLayout, i, 2);
                    }

                    optionsGrids.Add(optionsGrid);
                }
            }


            // Add buttons on the right
            this.cancelButton = new Button();
            this.cancelButton.Clicked += cancelButton_Click;
            this.explainWhyYouCantSkipButton = new Button();
            this.explainWhyYouCantSkipButton.Clicked += ExplainWhyYouCantSkipButton_Clicked;
            ButtonLayout cancelLayout;
            if (suggestion.Skippable)
                cancelLayout = new ButtonLayout(this.cancelButton, "X");
            else
                cancelLayout = new ButtonLayout(this.explainWhyYouCantSkipButton, "!");


            fullBuilder.AddLayout(new LayoutUnion(optionsGrids))
                .AddLayout(cancelLayout);

            this.SubLayout = fullBuilder.BuildAnyLayout();
        }

        private string summarize(ActivitySuggestion suggestion, bool repeatingDeclinedSuggestion)
        {
            // Summarize participation probability and predicted score
            string text;
            string shortTimeFormat = "HH:mm";
            string longTimeFormat = "HH:mm:ss";

            if (!suggestion.Skippable)
            {
                // If this is an unskippable experiment, then remind the user that they promised to do it
                text = "You promised to do " + suggestion.ActivityDescriptor.ActivityName + " at " + suggestion.StartDate.ToString(shortTimeFormat) + ".";
                text += " Get started!";
            }
            else
            {
                // For a normal suggestion, we tell them how likely we think it is that they will do it, and
                // how we think they will feel about it

                // Optional emphasis if we're repating ourselves and we think it's a good idea
                if (repeatingDeclinedSuggestion && !suggestion.WorseThanRootActivity)
                    text = "No, really: I recommend ";
                else
                    text = "";
                // activity name
                text += suggestion.ActivityDescriptor.ActivityName;

                // Start and end times
                string timeFormat;
                if (suggestion.Duration.HasValue && suggestion.Duration.Value.CompareTo(TimeSpan.FromMinutes(1)) >= 0)
                    timeFormat = shortTimeFormat;
                else
                    timeFormat = longTimeFormat;
                string whenText;
                if (suggestion.EndDate.HasValue)
                    whenText = " (until " + suggestion.EndDate.Value.ToString(timeFormat) + ")";
                else
                    whenText = "";
                text += " " + whenText + ".";
            }
            return text;
        }

        private void DoNowButton_Clicked(object sender, EventArgs e)
        {
            if (this.AcceptedSuggestion != null)
            {
                Button button = sender as Button;
                ActivitySuggestion suggestion = this.doButtons[button];
                this.AcceptedSuggestion.Invoke(suggestion);
            }
        }

        private void ExplainWhyYouCantSkipButton_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(new TextblockLayout("This suggestion is part of an experiment, so you're not allowed to skip it. " +
                "After spending some time on it, if you haven't completed it, then go to the participations page to record having worked on it. " +
                "That's where you can specify that you didn't complete it."), "Help");
        }

        void explainButton_Clicked(object sender, EventArgs e)
        {
            if (this.JustifySuggestion != null)
            {
                Button explainButton = sender as Button;
                ActivitySuggestion suggestion = this.explainButtons[explainButton];
                this.JustifySuggestion.Invoke(suggestion);
            }
        }

        void cancelButton_Click(object sender, EventArgs e)
        {
            if (this.Dismissed != null)
                this.Dismissed.Invoke(this.suggestion);
        }
        
        Button cancelButton;
        Button explainWhyYouCantSkipButton;
        Dictionary<Button, ActivitySuggestion> explainButtons;
        Dictionary<Button, ActivitySuggestion> doButtons;
        ActivitiesSuggestion suggestion;
        LayoutStack layoutStack;
    }
}
