using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using VisiPlacement;
using Xamarin.Forms;

// a SuggestionsView provides a user interface for requesting and receiving suggestions for which Activity to do
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

        public SuggestionsView(ActivityRecommender recommenderToInform, LayoutStack layoutStack, ActivityDatabase activityDatabase)
        {
            this.recommender = recommenderToInform;

            this.layoutStack = layoutStack;

            this.SetTitle("Get Suggestions");

            RequestSuggestion_Layout requestSuggestion_layout = new RequestSuggestion_Layout(activityDatabase, true);
            requestSuggestion_layout.RequestSuggestion += RequestSuggestion_layout_RequestSuggestion;

            this.requestSuggestion_layout = requestSuggestion_layout;


            LayoutChoice_Set helpWindow = (new HelpWindowBuilder()).AddMessage("Use this page to ask for a activity recommendations.")
                .AddMessage("By default, the recommendation will attempt to maximize your long-term happiness.")
                .AddMessage("The recommendation may be slightly randomized if ActivityRecommender doesn't have enough time to consider every activity within a couple seconds.")
                .AddMessage("If you are sure that you want an activity from a certain category, then you can enter its name into the category box, and ActivityRecommender will make sure " +
                "that your suggestion will be an instance of that activity. For example, if you have previously entered an activity named Checkers and listed it as a child activity " +
                "of Game, then when you ask for a Game, one possible suggestion will be Checkers.")
                .AddMessage("If you're looking for an activity to have a high amount of enjoyability right now, then think of what you would do if you couldn't ask ActivityRecommender for " +
                "help, and type the name of that activity into the second box. If you do, then ActivityRecommender will make sure to provide a suggestion that it thinks you will like as " +
                "much as it thinks you will like as much as the one you entered.")
                .AddMessage("Then, push Suggest to see the activity that is expected to maximize the overall long-term value to you, among the available choices.")
                .AddMessage("Each suggestion will list an activity name, followed by the time to start the activity, an estimate of the probability that you will actually do that activity, " +
                "and an estimate of the rating that you'd be expected to give to that activity.")
                .AddMessage("If you don't like a suggestion, press the X button next to it. The duration between when you ask for a suggestion and when you press the X is considered to " +
                "be worth 0 happiness to you, so ActivityRecommender tries to avoid giving you too many suggestions that you don't take. However, if there's a certain activity that " +
                "ActivityRecommender thinks would be awesome for you despite your current disinterest, then ActivityRecommender may repeat its suggestions a few times, in an effort to " +
                "get you to reconsider.")
                .AddMessage("You can also plan out several participations in a row by pressing the Suggest button multiple times in a row.")
                .AddMessage("Whenever you record a participation matching the first suggestion in the list, then that suggestion will be removed.")
                .AddMessage("Enjoy!")
                .Build();

            this.helpButton_layout = new HelpButtonLayout(helpWindow, this.layoutStack);
            this.experimentButton = new Button();
            this.startExperiment_layout = new ButtonLayout(this.experimentButton, "New Experiment (Advanced!)");
            this.experimentButton.Clicked += ExperimentButton_Clicked;
            this.topLayout = new Horizontal_GridLayout_Builder().Uniform().AddLayout(this.startExperiment_layout).AddLayout(this.helpButton_layout).Build();

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
            this.errorLayout = new TextblockLayout(errorMessage);
            this.UpdateLayout();
        }
        
        private void UpdateSuggestions()
        {
            this.Update_Suggestion_StartTimes();
            this.errorLayout = null;
            this.UpdateLayout();
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
        private void UpdateLayout()
        {
            LinkedList<LayoutChoice_Set> layouts = new LinkedList<LayoutChoice_Set>();
            if (this.suggestions.Count == 0)
                layouts.AddLast(this.topLayout);
            if (this.errorLayout != null)
                layouts.AddLast(errorLayout);
            foreach (ActivitySuggestion suggestion in this.suggestions)
            {
                layouts.AddLast(this.makeLayout(suggestion));
            }
            if (this.suggestions.Count < this.maxNumSuggestions)
                layouts.AddLast(this.requestSuggestion_layout);

            GridLayout grid = GridLayout.New(BoundProperty_List.Uniform(layouts.Count), new BoundProperty_List(1), LayoutScore.Zero);
            foreach (LayoutChoice_Set layout in layouts)
            {
                grid.AddLayout(layout);
            }

            this.SetContent(grid);
        }

        private LayoutChoice_Set makeLayout(ActivitySuggestion suggestion)
        {
            SuggestionView suggestionView = new SuggestionView(suggestion, this.layoutStack);
            suggestionView.Dismissed += SuggestionView_Dismissed;
            suggestionView.JustifySuggestion += SuggestionView_JustifySuggestion;
            return new LayoutCache(suggestionView);
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
            this.RemoveSuggestion(suggestion);
            this.recommender.DeclineSuggestion(suggestion);
        }


        LayoutChoice_Set requestSuggestion_layout;
        LayoutChoice_Set helpButton_layout;
        Button experimentButton;
        LayoutChoice_Set startExperiment_layout;
        LayoutChoice_Set topLayout;
        LayoutStack layoutStack;
        List<ActivitySuggestion> suggestions = new List<ActivitySuggestion>();
        int maxNumSuggestions = 4;
        ActivityRecommender recommender;
        LayoutChoice_Set errorLayout;
    }
}
