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
    class SuggestionsView : TitledControl, OnBack_Listener
    {
        public event RequestedExperiment ExperimentRequested;
        public delegate void RequestedExperiment();

        public event JustifySuggestionHandler JustifySuggestion;
        public delegate void JustifySuggestionHandler(ActivitySuggestion suggestion);

        public event RequestSuggestion_Handler RequestSuggestion;
        public delegate void RequestSuggestion_Handler(ActivityRequest request);

        public event VisitParticipationScreenHandler VisitParticipationScreen;
        public delegate void VisitParticipationScreenHandler();

        public event VisitActivitiesScreenHandler VisitActivitiesScreen;
        public delegate void VisitActivitiesScreenHandler();

        public SuggestionsView(ActivityRecommender recommenderToInform, LayoutStack layoutStack, ActivityDatabase activityDatabase, Engine engine) : base("Get Suggestions")
        {
            this.activityDatabase = activityDatabase;
            this.engine = engine;
            this.recommender = recommenderToInform;

            this.layoutStack = layoutStack;

            this.messageLayout = new TextblockLayout("What kind of suggestion would you like?").AlignHorizontally(TextAlignment.Center).AlignVertically(TextAlignment.Center);

            this.requestSuggestion_layout = new RequestSuggestion_Layout(activityDatabase, true, true, false, engine, layoutStack);
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
                    .Build()
                 )
                .Build();

            this.helpButton_layout = new HelpButtonLayout(helpWindow, this.layoutStack);
            this.experimentButton = new Button();
            this.startExperiment_layout = new ButtonLayout(this.experimentButton, "Experiment (Advanced!)");
            this.experimentButton.Clicked += ExperimentButton_Clicked;
            this.topLayout = new Horizontal_GridLayout_Builder().Uniform().AddLayout(this.startExperiment_layout).AddLayout(this.helpButton_layout).Build();

            Vertical_GridLayout_Builder noActivities_help_builder = new Vertical_GridLayout_Builder();
            noActivities_help_builder.AddLayout(new TextblockLayout("This screen is where you will be able to ask for suggestions of what to do."));
            noActivities_help_builder.AddLayout(
                new HelpButtonLayout("Not a calendar: no interruptions, no reminders. Exciting!",
                    new HelpWindowBuilder()
                        .AddMessage("ActivityRecommender doesn't offer reminders at prescheduled times because that's boring. " +
                        "If you're looking for a simple scheduling algorithm, you may be interested in a calendar. ActivityRecommender is more like " +
                        "a friend, full of cool, interesting, somewhat randomized ideas based on information about you.")
                        .Build()
                ,
                    layoutStack
                )
            );
            noActivities_help_builder.AddLayout(
                new HelpButtonLayout("Suggestions get better over time",
                    new HelpWindowBuilder()
                        .AddMessage("The more data you enter into ActivityRecommender, the better its suggestions will be.")
                        .AddMessage("When you haven't entered much data, ActivityRecommender primarily suggests activities that resemble other " +
                        "activities that you like. For example, if you've mentioned that Soccer and Jogging are relevant to you and that " +
                        "both of them are exercise, and if you tend to like Soccer, then ActivityRecommender may also recomend Jogging to you.")
                        .AddMessage("After you've entered a little bit of data, ActivityRecommender tends to suggest activities that you like. " +
                        "If you tend to enjoy playing Soccer, then ActivityRecommender will recommend that!")
                        .AddMessage("After you've entered more data, ActivityRecommender will be able to look for trends in how happy you are after "+
                        "having done various activities, and incorporate that too. For example, if you like to Eat Ice Cream, but eating ice cream" +
                        "late at night tends to make it hard to sleep, then maybe ActivityRecommender won't suggest it late at night even if you " +
                        "like it.")
                        .AddMessage("After you've entered a lot of data, ActivityRecommender will even be able to look for trends in how happy you are " +
                        "after suggesting various activities. If you like walking but you don't have any ideas of where to walk, then maybe " +
                        "ActivityRecommender will notice that suggesting Walking never works. However, if you like buying things at the store, then " +
                        "perhaps ActivityRecommender will suggest that instead, if that causes you to walk to the store to buy something.")
                        .Build()
                ,
                    layoutStack
                )
            );
            noActivities_help_builder.AddLayout(new TextblockLayout("Before you can ask for a suggestion, ActivityRecommender needs you to go back " +
                "and add some activities first. Here is a convenient button for jumping directly to the Activities screen:"));
            Button visitActivities_button = new Button();
            visitActivities_button.Text = "Activities";
            visitActivities_button.Clicked += VisitActivities_button_Clicked;
            noActivities_help_builder.AddLayout(new ButtonLayout(visitActivities_button));

            this.noActivities_explanationLayout = noActivities_help_builder.BuildAnyLayout();

            this.make_makeNewActivities_layout();

            this.UpdateSuggestions();
        }

        private void make_makeNewActivities_layout()
        {
            Button createNewActivity_button = new Button();
            createNewActivity_button.Clicked += CreateNewActivity_button_Clicked;
            Button brainstormNewActivities_button = new Button();
            brainstormNewActivities_button.Clicked += BrainstormNewActivities_button_Clicked;

            Horizontal_GridLayout_Builder builder = new Horizontal_GridLayout_Builder().Uniform();
            builder.AddLayout(this.numCandidates_layout);
            // Because numCandidates_layout shows the number of candidate activities, we can use extra-short descriptions on the create-new buttons
            builder.AddLayout(new ButtonLayout(createNewActivity_button, "New Activity"));
            builder.AddLayout(new ButtonLayout(brainstormNewActivities_button, "Brainstorm"));

            this.newActivities_layout = builder.Build();
        }

        private void CreateNewActivity_button_Clicked(object sender, EventArgs e)
        {
            ActivityCreationLayout creationLayout = new ActivityCreationLayout(this.activityDatabase, this.layoutStack);
            this.layoutStack.AddLayout(creationLayout, "New Activity", this);
        }

        private void BrainstormNewActivities_button_Clicked(object sender, EventArgs e)
        {
            if (this.VisitActivitiesScreen != null)
                this.VisitActivitiesScreen.Invoke();
        }


        private void VisitActivities_button_Clicked(object sender, EventArgs e)
        {
            if (this.VisitActivitiesScreen != null)
                this.VisitActivitiesScreen.Invoke();
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            this.updateNumCandidateActivities();

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

        private void updateNumCandidateActivities()
        {
            if (this.numCandidates < 0)
            {
                this.numCandidates = this.engine.GetActivitiesToConsider(new ActivityRequest()).Count;
                this.numCandidates_layout.setText("" + this.numCandidates + " candidates");
            }
        }
        public void OnBack(LayoutChoice_Set layout)
        {
            this.invalidateNumCandidates();
        }

        private void invalidateNumCandidates()
        {
            this.numCandidates = -1;
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
            this.UpdateSuggestionsAndMessage();
        }

        public void ClearSuggestions()
        {
            this.suggestions.Clear();
            this.UpdateSuggestionsAndMessage();
        }
        public void AddSuggestion(ActivitySuggestion suggestion)
        {
            this.suggestions.Add(suggestion);
            this.UpdateSuggestionsAndMessage();
            this.previousDeclinedSuggestion = null;
        }
        public void AddSuggestions(IEnumerable<ActivitySuggestion> suggestions)
        {
            foreach (ActivitySuggestion suggestion in suggestions)
            {
                this.suggestions.Add(suggestion);
                this.UpdateSuggestionsAndMessage();
            }
        }
        public IEnumerable<ActivitySuggestion> GetSuggestions()
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
            // If the user hasn't asked for any suggestions yet, then show them some buttons for making more activities
            // Alternatively, if our suggestion isn't that good, be sure to show them those buttons for making more activities
            if (this.suggestions.Count < 1 || (this.suggestions.Count == 1 && this.suggestions[0].WorseThanRootActivity))
            {
                layouts.Add(this.newActivities_layout);
            }
            // show feedback if there is any
            if (this.messageLayout.ModelledText != "")
            {
                layouts.Add(messageLayout);
            }
            // show suggestions if there are any
            bool addDoNowButton = true;
            if (this.suggestions.Count > 0)
            {
                foreach (ActivitySuggestion suggestion in this.suggestions)
                {
                    bool repeatingDeclinedSuggestion = false;
                    if (this.previousDeclinedSuggestion != null && suggestion.ActivityDescriptor.CanMatch(previousDeclinedSuggestion.ActivityDescriptor))
                        repeatingDeclinedSuggestion = true;
                    layouts.Add(this.makeLayout(suggestion, addDoNowButton, repeatingDeclinedSuggestion));
                    addDoNowButton = false;
                }
            }
            // Show an explanation about how multiple suggestions work (they're in chronological order) if there's room
            // Also be sure to save room for the suggestion buttons
            if (layouts.Count < this.maxNumSuggestions - 1)
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
                layouts.Add(this.topLayout);
            }

            this.SetContent(new Vertical_GridLayout_Builder().Uniform().AddLayouts(layouts).BuildAnyLayout());
        }

        private LayoutChoice_Set makeLayout(ActivitySuggestion suggestion, bool doNowButton, bool repeatingDeclinedSuggestion)
        {
            SuggestionView suggestionView = new SuggestionView(suggestion, doNowButton, repeatingDeclinedSuggestion, this.layoutStack);
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
            this.previousDeclinedSuggestion = suggestion;
            this.suggestions.Remove(suggestion);
            ActivitySkip skip = this.recommender.DeclineSuggestion(suggestion);
            double numSecondsThinking = skip.ThinkingTime.TotalSeconds;
            this.SetErrorMessage("Recorded " + (int)numSecondsThinking + " seconds (wasted) considering " + suggestion.ActivityDescriptor.ActivityName);
            this.Update_Suggestion_StartTimes();
            this.UpdateLayout_From_Suggestions();
        }


        RequestSuggestion_Layout requestSuggestion_layout;
        TextblockLayout askWhatIsNext_layout;
        LayoutChoice_Set helpButton_layout;
        Button experimentButton;
        LayoutChoice_Set startExperiment_layout;
        LayoutChoice_Set topLayout;
        LayoutStack layoutStack;
        List<ActivitySuggestion> suggestions = new List<ActivitySuggestion>();
        int maxNumSuggestions = 4;
        Engine engine;
        ActivityRecommender recommender;
        TextblockLayout messageLayout;
        LayoutChoice_Set noActivities_explanationLayout;
        ActivityDatabase activityDatabase;
        ActivitySuggestion previousDeclinedSuggestion;
        int numCandidates = -1;

        TextblockLayout numCandidates_layout = new TextblockLayout().AlignHorizontally(TextAlignment.Center).AlignVertically(TextAlignment.Center);
        LayoutChoice_Set newActivities_layout;
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
