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

        public event VisitParticipationScreenHandler AcceptedSuggestion;
        public delegate void VisitParticipationScreenHandler(ActivitySuggestion suggestion);

        public event VisitProtoactivitiesScreenHandler VisitProtoactivitiesScreen;
        public delegate void VisitProtoactivitiesScreenHandler();

        public event Visit_AnalyzeActivitiesScreen_Handler VisitAnalyzeActivitiesScreen;
        public delegate void Visit_AnalyzeActivitiesScreen_Handler();

        public SuggestionsView(ActivityRecommender recommenderToInform, LayoutStack layoutStack, ActivityDatabase activityDatabase, Engine engine, UserSettings userSettings) : base("Get Suggestions")
        {
            this.TitleLayout.AlignVertically(TextAlignment.Center);
            this.activityDatabase = activityDatabase;
            this.engine = engine;
            this.recommender = recommenderToInform;
            this.userSettings = userSettings;

            this.layoutStack = layoutStack;

            this.messageLayout = new TextblockLayout("").AlignHorizontally(TextAlignment.Center).AlignVertically(TextAlignment.Center);

            this.requestSuggestion_layout = new RequestSuggestion_Layout(activityDatabase, true, true, false, 3, engine, layoutStack);
            this.requestSuggestion_layout.RequestSuggestion += RequestSuggestion_layout_RequestSuggestion;
            this.askWhatIsNext_layout = new TextblockLayout().AlignHorizontally(TextAlignment.Center).AlignVertically(TextAlignment.Center);

            LayoutChoice_Set helpWindow = (new HelpWindowBuilder()).AddMessage("Use this screen to ask for activity recommendations from among activities you have said that you like.")
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
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 12, 17), "Mentioned that the suggestions were sometimes more repetitive than desired")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2021, 3, 26), "Suggested that the suggestions view could use the same descriptions as the participation entry view")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2021, 4, 24), "Asked for the suggestions to be less repetitive again")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2021, 4, 30), "Asked for the suggestions view to display multiple suggestions at once")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2021, 7, 2), "Suggested shorter suggestion button text")
                    .Build()
                 )
                .Build();

            this.helpButton_layout = new HelpButtonLayout(helpWindow, this.layoutStack);
            this.experimentButton = new Button();
            this.startExperiment_layout = new ButtonLayout(this.experimentButton, "Efficiency Experiment");
            this.experimentButton.Clicked += ExperimentButton_Clicked;
            this.bottomLayout = new Horizontal_GridLayout_Builder().Uniform().AddLayout(this.startExperiment_layout).AddLayout(this.helpButton_layout).Build();

            this.UpdateSuggestions();
        }

        private void RequestSuggestion_layout_RequestSuggestion(ActivityRequest activityRequest)
        {
            this.RequestSuggestion.Invoke(activityRequest);
        }

        private void ExperimentButton_Clicked(object sender, EventArgs e)
        {
            this.ExperimentRequested.Invoke();
        }

        public void RemoveSuggestion(ActivitiesSuggestion suggestion)
        {
            this.suggestions.Remove(suggestion);
            this.UpdateSuggestionsAndMessage();
        }

        public void ClearSuggestions()
        {
            this.suggestions.Clear();
            this.UpdateSuggestionsAndMessage();
        }
        public void AddSuggestion(ActivitiesSuggestion suggestion)
        {
            this.suggestions.Add(suggestion);
            this.UpdateSuggestionsAndMessage();
            this.previousDeclinedSuggestion = null;
        }
        public void AddSuggestions(IEnumerable<ActivitiesSuggestion> suggestions)
        {
            foreach (ActivitiesSuggestion suggestion in suggestions)
            {
                this.suggestions.Add(suggestion);
                this.UpdateSuggestionsAndMessage();
            }
        }
        public IEnumerable<ActivitiesSuggestion> GetSuggestions()
        {
            this.Update_Suggestion_StartTimes();
            return this.suggestions;
        }
        public void SetErrorMessage(string errorMessage)
        {
            this.messageLayout.setText(errorMessage);
            this.UpdateLayout_From_Suggestions();
        }
        public Participation LatestParticipation
        {
            set
            {
                this.requestSuggestion_layout.LatestParticipation = value;
            }
        }

        public List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() {
                new RequestSuggestion_Feature(this.activityDatabase),
                new DeclineSuggestion_Feature(this.activityDatabase),
                new RequestSuggestionFromCategory_Feature(this.activityDatabase),
                new RequestSuggestionToBeat_Feature(this.activityDatabase),
                new StartExperiment_Feature(this.engine)
            };
        }
        
        private void UpdateSuggestionsAndMessage()
        {
            this.messageLayout.setText("");
            this.UpdateSuggestions();
        }
        private void UpdateSuggestions()
        {
            this.Update_Suggestion_StartTimes();
            this.UpdateLayout_From_Suggestions();
        }

        private void Update_Suggestion_StartTimes()
        {
            // Update the start time of each activity to be when the previous one ends
            DateTime start = DateTime.Now;
            foreach (ActivitiesSuggestion suggestion in this.suggestions)
            {
                foreach (ActivitySuggestion child in suggestion.Children)
                {
                    TimeSpan duration = child.Duration.Value;
                    child.StartDate = start;
                    child.EndDate = start.Add(duration);
                }
                start = suggestion.Children[0].EndDate.Value;
            }
        }
        private void UpdateLayout_From_Suggestions()
        {
            List<LayoutChoice_Set> layouts = new List<LayoutChoice_Set>();
            // show feedback if there is any
            if (this.messageLayout.ModelledText != "")
            {
                layouts.Add(messageLayout);
            }
            // show suggestions if there are any
            bool addDoNowButton = true;
            if (this.suggestions.Count > 0)
            {
                foreach (ActivitiesSuggestion suggestion in this.suggestions)
                {
                    Dictionary<ActivitySuggestion, bool> repeatingDeclinedSuggestion = new Dictionary<ActivitySuggestion, bool>();
                    foreach (ActivitySuggestion child in suggestion.Children)
                    {
                        if (this.previousDeclinedSuggestion != null && this.previousDeclinedSuggestion.CanMatch(child.ActivityDescriptor))
                            repeatingDeclinedSuggestion[child] = true;
                        else
                            repeatingDeclinedSuggestion[child] = false;
                    }
                    layouts.Add(this.makeLayout(suggestion, addDoNowButton, repeatingDeclinedSuggestion));
                    addDoNowButton = false;
                }
            }
            // Show an explanation about how multiple suggestions work (they're in chronological order) if there's room
            // Also be sure to save room for the suggestion buttons
            if (layouts.Count <= this.maxNumSuggestions - 2)
            {
                if (this.suggestions.Count > 0)
                {
                    this.askWhatIsNext_layout.setText("What's after that?");
                    layouts.Add(this.askWhatIsNext_layout);
                }
            }

            // show the button for getting more suggestions if there's room
            if (this.suggestions.Count < this.maxNumSuggestions)
            {
                layouts.Add(this.requestSuggestion_layout);
            }
            // show help and experiments if there are no suggestions visible
            if (this.suggestions.Count < 1)
            {
                layouts.Add(this.bottomLayout);
            }

            LayoutChoice_Set even =  new Vertical_GridLayout_Builder().Uniform().AddLayouts(layouts).BuildAnyLayout();
            LayoutChoice_Set uneven = new ScoreShifted_Layout(new Vertical_GridLayout_Builder().AddLayouts(layouts).BuildAnyLayout(), LayoutScore.Get_UnCentered_LayoutScore(1));

            this.SetContent(new LayoutUnion(even, uneven));
        }

        private LayoutChoice_Set makeLayout(ActivitiesSuggestion suggestion, bool doNowButton, Dictionary<ActivitySuggestion, bool> repeatingDeclinedSuggestion)
        {
            SuggestionView suggestionView = new SuggestionView(suggestion, doNowButton, repeatingDeclinedSuggestion, this.userSettings, this.layoutStack);
            suggestionView.Dismissed += SuggestionView_Dismissed;
            suggestionView.JustifySuggestion += SuggestionView_JustifySuggestion;
            suggestionView.AcceptedSuggestion += SuggestionView_VisitParticipationScreen;
            suggestionView.VisitProtoactivitiesScreen += SuggestionView_VisitProtoactivitiesScreen;
            suggestionView.Request_AnalyzeActivities += SuggestionView_Visit_AnalyzeScreen;
            return new LayoutCache(suggestionView);
        }

        private void SuggestionView_Visit_AnalyzeScreen()
        {
            this.VisitAnalyzeActivitiesScreen.Invoke();
        }

        private void SuggestionView_VisitProtoactivitiesScreen()
        {
            this.VisitProtoactivitiesScreen.Invoke();
        }

        private void SuggestionView_VisitParticipationScreen(ActivitySuggestion suggestion)
        {
            this.AcceptedSuggestion.Invoke(suggestion);
        }

        private void SuggestionView_JustifySuggestion(ActivitySuggestion suggestion)
        {
            this.JustifySuggestion.Invoke(suggestion);
        }

        private void SuggestionView_Dismissed(ActivitiesSuggestion suggestion)
        {
            this.DeclineSuggestion(suggestion);
        }

        private void DeclineSuggestion(ActivitiesSuggestion suggestion)
        {
            this.previousDeclinedSuggestion = suggestion;
            this.suggestions.Remove(suggestion);
            ActivitySkip skip = this.recommender.DeclineSuggestion(suggestion);
            double numSecondsThinking = skip.ThinkingTime.TotalSeconds;
            string message = "Recorded " + (int)numSecondsThinking + " seconds (wasted) considering " + suggestion.Children[0].ActivityDescriptor.ActivityName;
            if (suggestion.Children.Count > 1)
                message += ", ...";
            this.SetErrorMessage(message);
            this.Update_Suggestion_StartTimes();
            this.UpdateLayout_From_Suggestions();
        }


        RequestSuggestion_Layout requestSuggestion_layout;
        TextblockLayout askWhatIsNext_layout;
        LayoutChoice_Set helpButton_layout;
        Button experimentButton;
        LayoutChoice_Set startExperiment_layout;
        LayoutChoice_Set bottomLayout;
        LayoutStack layoutStack;
        List<ActivitiesSuggestion> suggestions = new List<ActivitiesSuggestion>();
        int maxNumSuggestions = 4;
        Engine engine;
        ActivityRecommender recommender;
        TextblockLayout messageLayout;
        LayoutChoice_Set noActivities_explanationLayout;
        ActivityDatabase activityDatabase;
        ActivitiesSuggestion previousDeclinedSuggestion;
        UserSettings userSettings;
    }

    class RequestSuggestion_Feature : AppFeature
    {
        public RequestSuggestion_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Ask for a suggestion";
        }
        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.RootActivity.NumSuggestions > 0;
        }
        public bool GetIsUsable()
        {
            return this.activityDatabase.ContainsCustomActivity();
        }
        ActivityDatabase activityDatabase;
    }

    class DeclineSuggestion_Feature : AppFeature
    {
        public DeclineSuggestion_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Decline a suggestion";
        }
        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.RootActivity.NumSkips > 0;
        }
        public bool GetIsUsable()
        {
            return this.activityDatabase.RootActivity.NumSuggestions > 0;
        }
        ActivityDatabase activityDatabase;

    }
    class RequestSuggestionFromCategory_Feature : AppFeature
    {
        public RequestSuggestionFromCategory_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Request suggestion from specific category";
        }

        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.RequestedActivityFromCategory;
        }
        public bool GetIsUsable()
        {
            return this.activityDatabase.ContainsCustomActivity();
        }

        ActivityDatabase activityDatabase;
    }

    class RequestSuggestionToBeat_Feature : AppFeature
    {
        public RequestSuggestionToBeat_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Request a suggestion at least as good as another activity";
        }

        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.RequestedActivityAtLeastAsGoodAsOther;
        }
        public bool GetIsUsable()
        {
            return this.activityDatabase.ContainsCustomActivity();
        }

        ActivityDatabase activityDatabase;
    }

    class StartExperiment_Feature : AppFeature
    {
        public StartExperiment_Feature(Engine engine)
        {
            this.engine = engine;
        }
        public string GetDescription()
        {
            return "Start an experiment";
        }

        public bool GetHasBeenUsed()
        {
            return this.engine.HasInitiatedExperiment;
        }
        public bool GetIsUsable()
        {
            return !this.engine.Test_ChooseExperimentOption().HasError;
        }
        Engine engine;
    }

    class RequestSolution_Feature : AppFeature
    {
        public RequestSolution_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }

        public string GetDescription()
        {
            return "Request a solution to a Problem";
        }
        public bool GetIsUsable()
        {
            return this.activityDatabase.HasProblem;
        }

        public bool GetHasBeenUsed()
        {
            foreach (Problem problem in this.activityDatabase.AllProblems)
            {
                if (problem.EverRequestedFromDirectly)
                    return true;
            }
            return false;

        }
        ActivityDatabase activityDatabase;
    }

}
