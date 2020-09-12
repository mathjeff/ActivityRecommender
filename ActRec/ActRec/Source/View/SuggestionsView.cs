using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using VisiPlacement;
using Xamarin.Forms;

// A SuggestionsView provides a user interface for requesting and receiving suggestions for which Activity to do
namespace ActivityRecommendation.View
{
    class SuggestionsView : TitledControl
    {
        public event RequestedExperiment ExperimentRequested;
        public delegate void RequestedExperiment();

        public event JustifySuggestionHandler JustifySuggestion;
        public delegate void JustifySuggestionHandler(ActivitySuggestion suggestion);

        public event RequestSuggestion_Handler RequestSuggestion;
        public delegate void RequestSuggestion_Handler(ActivityRequest request);

        public event VisitParticipationScreenHandler VisitParticipationScreen;
        public delegate void VisitParticipationScreenHandler();

        public SuggestionsView(ActivityRecommender recommenderToInform, LayoutStack layoutStack, ActivityDatabase activityDatabase, Engine engine)
        {
            this.activityDatabase = activityDatabase;
            this.recommender = recommenderToInform;

            this.layoutStack = layoutStack;

            this.SetTitle("Get Suggestions");

            this.requestSuggestion_layout = new RequestSuggestion_Layout(activityDatabase, true, true, false, engine, layoutStack);
            this.requestSuggestion_layout.RequestSuggestion += RequestSuggestion_layout_RequestSuggestion;

            LayoutChoice_Set helpWindow = (new HelpWindowBuilder()).AddMessage("Use this screen to ask for a activity recommendations.")
                .AddMessage("By default, the recommendation will attempt to maximize your long-term happiness.")
                .AddMessage("The recommendation may be slightly randomized if ActivityRecommender doesn't have enough time to consider every activity within a couple seconds.")
                .AddMessage("If you are sure that you want an activity from a certain category, then you can enter its name into the category box, and ActivityRecommender will make sure " +
                "that your suggestion will be an instance of that activity. For example, if you have previously entered an activity named Checkers and listed it as a child activity " +
                "of Game, then when you ask for a Game, one possible suggestion will be Checkers.")
                .AddMessage("If you're looking for an activity to have a high amount of enjoyability right now, then think of what you would do if you couldn't ask ActivityRecommender for " +
                "help, and type the name of that activity into the second box. If you do, then ActivityRecommender will make sure to provide a suggestion that it thinks you will like as " +
                "much as it thinks you will like as much as the one you entered.")
                .AddMessage("Then, push one of the Suggest buttons")
                .AddMessage("You can request either the activity that is expected to maximize the long-term value to you, or " +
                "the activity that ActivityRecommender thinks that you're most likely to do, among the activities satisfying your other criteria.")
                .AddMessage("Each suggestion will list an activity name, followed by the time to start the activity, an estimate of the probability that you will actually do that activity, " +
                "and an estimate of the rating that you'd be expected to give to that activity.")
                .AddMessage("If you don't like a suggestion, press the X button next to it. The duration between when you ask for a suggestion and when you press the X is considered to " +
                "be worth 0 happiness to you (unless you already recorded having done a participation during that time, in which case the duration between your latest completed participation and " +
                "when you press the X is considered to be worth 0 happiness to you), so ActivityRecommender tries to avoid giving you too many suggestions that you don't take. However, if there's " +
                "a certain activity that ActivityRecommender thinks would be awesome for you despite your current disinterest, then ActivityRecommender may repeat its suggestions a few times, in " +
                "an effort to get you to reconsider.")
                .AddMessage("You can also plan out several participations in a row by pressing the Suggest button multiple times in a row.")
                .AddMessage("Whenever you record a participation matching the first suggestion in the list, then that suggestion will be removed.")
                .AddMessage("Enjoy!")
                .AddLayout(new CreditsButtonBuilder(layoutStack)
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2019, 8, 1), "Suggested that when the user asks for a suggestion at least as fun as a certain activity, ActivityRecommender should always suggest a different activity than the one they mentioned")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 7, 18), "Pointed out that buttons on iOS weren't visually responding to touch")
                    .Build()
                 )
                .Build();

            this.helpButton_layout = new HelpButtonLayout(helpWindow, this.layoutStack);
            this.experimentButton = new Button();
            this.startExperiment_layout = new ButtonLayout(this.experimentButton, "New Experiment (Advanced!)");
            this.experimentButton.Clicked += ExperimentButton_Clicked;
            this.topLayout = new Horizontal_GridLayout_Builder().Uniform().AddLayout(this.startExperiment_layout).AddLayout(this.helpButton_layout).Build();

            this.noActivities_explanationLayout = new TextblockLayout(
                "This screen is where you will be able to ask for suggestions of what to do.\n" +
                "Before you can ask for a suggestion, ActivityRecommender needs to know what activities are relevant to you.\n" +
                "You should go back and create at least one activity first (press the button that says \"Activities\" and proceed from there)."
                );

            this.UpdateSuggestions();
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            // show/hide the noActivities_explanationLayout as needed
            bool shouldShowActivityCreationHelp = !this.activityDatabase.ContainsCustomActivity();
            if (shouldShowActivityCreationHelp)
            {
                this.SetContent(this.noActivities_explanationLayout);
            }
            else
            {
                bool doesShowActivityCreationHelp = (this.GetContent() == this.noActivities_explanationLayout);
                if (doesShowActivityCreationHelp)
                {
                    this.UpdateLayout_From_Suggestions();
                }
            }
            // call parent
            return base.GetBestLayout(query);
        }

        private void RequestSuggestion_layout_RequestSuggestion(ActivityRequest activityRequest)
        {
            this.RequestSuggestion.Invoke(activityRequest);
        }

        private void ExperimentButton_Clicked(object sender, EventArgs e)
        {
            this.ExperimentRequested.Invoke();
        }

        public void RemoveSuggestion(ActivitySuggestion suggestion)
        {
            this.suggestions.Remove(suggestion);
            this.UpdateSuggestions();
        }

        public void ClearSuggestions()
        {
            this.suggestions.Clear();
            this.UpdateSuggestions();
        }
        public void AddSuggestion(ActivitySuggestion suggestion)
        {
            this.suggestions.Add(suggestion);
            this.UpdateSuggestions();
        }
        public void AddSuggestions(IEnumerable<ActivitySuggestion> suggestions)
        {
            foreach (ActivitySuggestion suggestion in suggestions)
            {
                this.suggestions.Add(suggestion);
                this.UpdateSuggestions();
            }
        }
        public IEnumerable<ActivitySuggestion> GetSuggestions()
        {
            this.Update_Suggestion_StartTimes();
            return this.suggestions;
        }
        public void SetErrorMessage(string errorMessage)
        {
            this.messageLayout = new TextblockLayout(errorMessage);
            this.UpdateLayout_From_Suggestions();
        }
        public Participation LatestParticipation
        {
            set
            {
                this.requestSuggestion_layout.LatestParticipation = value;
            }
        }
        
        private void UpdateSuggestions()
        {
            this.Update_Suggestion_StartTimes();
            this.messageLayout = null;
            this.UpdateLayout_From_Suggestions();
        }
        private void Update_Suggestion_StartTimes()
        {
            // Update the start time of each activity to be when the previous one ends
            DateTime start = DateTime.Now;
            foreach (ActivitySuggestion suggestion in this.suggestions)
            {
                TimeSpan duration = suggestion.Duration.Value;
                suggestion.StartDate = start;
                suggestion.EndDate = start = start.Add(duration);
            }
        }
        private void UpdateLayout_From_Suggestions()
        {
            List<LayoutChoice_Set> layouts = new List<LayoutChoice_Set>();
            if (this.suggestions.Count == 0)
                layouts.Add(this.topLayout);
            if (this.messageLayout != null)
                layouts.Insert(0, messageLayout);
            bool addDoNowButton = true;
            foreach (ActivitySuggestion suggestion in this.suggestions)
            {
                layouts.Add(this.makeLayout(suggestion, addDoNowButton));
                addDoNowButton = false;
            }
            if (this.suggestions.Count < this.maxNumSuggestions)
                layouts.Add(this.requestSuggestion_layout);

            GridLayout grid = GridLayout.New(BoundProperty_List.Uniform(layouts.Count), new BoundProperty_List(1), LayoutScore.Zero);
            foreach (LayoutChoice_Set layout in layouts)
            {
                grid.AddLayout(layout);
            }

            this.SetContent(grid);
        }

        private LayoutChoice_Set makeLayout(ActivitySuggestion suggestion, bool doNowButton)
        {
            SuggestionView suggestionView = new SuggestionView(suggestion, doNowButton, this.layoutStack);
            suggestionView.Dismissed += SuggestionView_Dismissed;
            suggestionView.JustifySuggestion += SuggestionView_JustifySuggestion;
            suggestionView.VisitParticipationScreen += SuggestionView_VisitParticipationScreen;
            return new LayoutCache(suggestionView);
        }

        private void SuggestionView_VisitParticipationScreen()
        {
            this.VisitParticipationScreen.Invoke();
        }

        private void SuggestionView_JustifySuggestion(ActivitySuggestion suggestion)
        {
            this.JustifySuggestion.Invoke(suggestion);
        }

        private void SuggestionView_Dismissed(ActivitySuggestion suggestion)
        {
            this.DeclineSuggestion(suggestion);
        }

        private void DeclineSuggestion(ActivitySuggestion suggestion)
        {
            this.suggestions.Remove(suggestion);
            ActivitySkip skip = this.recommender.DeclineSuggestion(suggestion);
            double numSecondsThinking = skip.ThinkingTime.TotalSeconds;
            this.messageLayout = new TextblockLayout("Recorded " + (int)numSecondsThinking + " seconds (wasted) considering " + suggestion.ActivityDescriptor.ActivityName);
            this.Update_Suggestion_StartTimes();
            this.UpdateLayout_From_Suggestions();
        }


        RequestSuggestion_Layout requestSuggestion_layout;
        LayoutChoice_Set helpButton_layout;
        Button experimentButton;
        LayoutChoice_Set startExperiment_layout;
        LayoutChoice_Set topLayout;
        LayoutStack layoutStack;
        List<ActivitySuggestion> suggestions = new List<ActivitySuggestion>();
        int maxNumSuggestions = 4;
        ActivityRecommender recommender;
        LayoutChoice_Set messageLayout;
        LayoutChoice_Set noActivities_explanationLayout;
        ActivityDatabase activityDatabase;
    }
}
