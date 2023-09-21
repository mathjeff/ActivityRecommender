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

        public event RequestedExperiment RequestExperiment;
        public delegate void RequestedExperiment(ActivitySuggestion suggestion);

        public event JustifySuggestionHandler JustifySuggestion;
        public delegate void JustifySuggestionHandler(ActivitySuggestion suggestion);

        public event VisitParticipationScreenHandler AcceptedSuggestion;
        public delegate void VisitParticipationScreenHandler(ActivitySuggestion suggestion);

        public event VisitProtoactivitiesScreenHandler VisitProtoactivitiesScreen;
        public delegate void VisitProtoactivitiesScreenHandler();

        public event NewActivitityHandler Request_MakeNewActivity;
        public delegate void NewActivitityHandler();


        public SuggestionView(ActivitiesSuggestion suggestion, bool isFirstSuggestion, Dictionary<ActivitySuggestion, bool> repeatingDeclinedSuggestion, UserSettings userSettings, LayoutStack layoutStack)
        {
            this.suggestion = suggestion;
            this.layoutStack = layoutStack;
            this.userSettings = userSettings;

            LayoutChoice_Set cancelLayout = this.getCancelLayout(suggestion.Skippable);

            bool allWorseThanAverage = true;
            foreach (ActivitySuggestion child in suggestion.Children)
            {
                if (!child.WorseThanRootActivity)
                    allWorseThanAverage = false;
            }

            //GridLayout_Builder fullBuilder = new Horizontal_GridLayout_Builder();
            string startTimeText = suggestion.Children[0].StartDate.ToString("HH:mm");

            bool badSuggestion = (allWorseThanAverage && suggestion.Skippable);

            //fullBuilder.AddLayout(new TextblockLayout("At " + startTimeText + ":", 24).AlignHorizontally(TextAlignment.Center));

            List<LayoutChoice_Set> specificFont_contentChoices = new List<LayoutChoice_Set>(); // list of layouts we might use, each with a different font size
            this.explainButtons = new Dictionary<Button, ActivitySuggestion>();
            this.doButtons = new Dictionary<Button, ActivitySuggestion>();
            for (int mainFontSize = 20; mainFontSize >= 12; mainFontSize -= 8)
            {
                // grid containing the specific activities the user could do
                GridLayout activityOptionsGrid = GridLayout.New(new BoundProperty_List(3), BoundProperty_List.Uniform(suggestion.Children.Count), LayoutScore.Zero);

                for (int i = 0; i < suggestion.Children.Count; i++)
                {
                    ActivitySuggestion child = suggestion.Children[i];

                    // set up the options for the text
                    string mainText = this.summarize(child, repeatingDeclinedSuggestion[child]);
                    TextblockLayout mainBlock = new TextblockLayout(mainText, mainFontSize);
                    mainBlock.AlignHorizontally(TextAlignment.Center);
                    mainBlock.AlignVertically(TextAlignment.Center);
                    activityOptionsGrid.PutLayout(mainBlock, i, 0);

                    // set up the buttons
                    GridLayout_Builder bottomButtons_builder = new Horizontal_GridLayout_Builder().Uniform();
                    double buttonFontSize = mainFontSize * 0.9;
                    // make a doNow button if needed
                    if (isFirstSuggestion)
                    {
                        if (!child.WorseThanRootActivity)
                        {
                            Button doNowButton = new Button();
                            doNowButton.Clicked += DoNowButton_Clicked;
                            this.doButtons[doNowButton] = child;
                            ButtonLayout doButtonLayout = new ButtonLayout(doNowButton, "OK", buttonFontSize);
                            bottomButtons_builder.AddLayout(doButtonLayout);
                        }
                    }
                    if (child.WorseThanRootActivity)
                    {
                        bottomButtons_builder.AddLayouts(make_otherActivities_layout(buttonFontSize));
                    }
                    if (child.PredictedScoreDividedByAverage != null)
                    {
                        Button explainButton = new Button();
                        explainButton.Clicked += explainButton_Clicked;
                        this.explainButtons[explainButton] = child;
                        ButtonLayout explainLayout = new ButtonLayout(explainButton, "?", buttonFontSize);
                        bottomButtons_builder.AddLayout(explainLayout);
                    }
                    bottomButtons_builder.AddLayout(cancelLayout);
                    if (child.ExpectedReaction != null && !child.WorseThanRootActivity)
                    {
                        TextblockLayout reactionLayout = new TextblockLayout(child.ExpectedReaction.Get(this.userSettings.FeedbackType), buttonFontSize * 0.9);
                        reactionLayout.AlignHorizontally(TextAlignment.Center);
                        reactionLayout.AlignVertically(TextAlignment.Center);
                        activityOptionsGrid.PutLayout(reactionLayout, i, 1);
                    }
                    activityOptionsGrid.PutLayout(bottomButtons_builder.BuildAnyLayout(), i, 2);
                }

                LayoutChoice_Set optionsAtThisFontSize = activityOptionsGrid;
                specificFont_contentChoices.Add(optionsAtThisFontSize);
            }

            LayoutChoice_Set contentGrid = LayoutUnion.New(specificFont_contentChoices);

            this.SubLayout = contentGrid;
        }

        private LayoutChoice_Set getCancelLayout(bool skippable)
        {
            // Add cancel buttons to the bottom
            this.cancelButton = new Button();
            this.cancelButton.Clicked += cancelButton_Click;
            this.explainWhyYouCantSkipButton = new Button();
            this.explainWhyYouCantSkipButton.Clicked += ExplainWhyYouCantSkipButton_Clicked;
            ButtonLayout cancelLayout;
            if (skippable)
                cancelLayout = new ButtonLayout(this.cancelButton, "X");
            else
                cancelLayout = new ButtonLayout(this.explainWhyYouCantSkipButton, "!");
            return cancelLayout;
        }
        private List<LayoutChoice_Set> make_otherActivities_layout(double fontSize)
        {
            Button createNewActivity_button = new Button();
            createNewActivity_button.Clicked += CreateNewActivity_button_Clicked;
            Button brainstormNewActivities_button = new Button();
            brainstormNewActivities_button.Clicked += BrainstormNewActivities_button_Clicked;

            List<LayoutChoice_Set> layouts = new List<LayoutChoice_Set>();

            // space in "brain storm" to allow breaking specifcially at that location in the middle of the word
            layouts.Add(new ButtonLayout(brainstormNewActivities_button, "Brain storm", fontSize));
            layouts.Add(new ButtonLayout(createNewActivity_button, "New activity", fontSize));
            return layouts;
        }

        private void BrainstormNewActivities_button_Clicked(object sender, EventArgs e)
        {
            this.VisitProtoactivitiesScreen.Invoke();
        }

        private void CreateNewActivity_button_Clicked(object sender, EventArgs e)
        {
            this.Request_MakeNewActivity.Invoke();
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
                {
                    text = "No, really: I recommend " + suggestion.ActivityDescriptor.ActivityName;
                }
                else
                {
                    if (suggestion.WorseThanRootActivity)
                    {

                        text = "Remember " + suggestion.ActivityDescriptor.ActivityName + "? Don't do that!";
                    }
                    else
                    {
                        text = suggestion.ActivityDescriptor.ActivityName;
                    }
                }

                if (!suggestion.WorseThanRootActivity)
                {
                    // Start and end times
                    string timeFormat;
                    if (suggestion.Duration.HasValue && suggestion.Duration.Value.CompareTo(TimeSpan.FromMinutes(1)) >= 0)
                        timeFormat = shortTimeFormat;
                    else
                        timeFormat = longTimeFormat;
                    string whenText;
                    if (suggestion.EndDate.HasValue)
                        whenText = " (" + suggestion.StartDate.ToString(timeFormat) + "-" + suggestion.EndDate.Value.ToString(timeFormat) + ").";
                    else
                        whenText = ".";
                    text += whenText;
                }
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
        UserSettings userSettings;
    }
}
