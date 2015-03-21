﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using VisiPlacement;

// a SuggestionsView provides a user interface for requesting and receiving suggestions for which Activity to do
namespace ActivityRecommendation
{
    class SuggestionsView : TitledControl
    {
        public SuggestionsView(ActivityRecommender recommenderToInform, LayoutStack layoutStack)
        {
            this.recommender = recommenderToInform;

            this.layoutStack = layoutStack;

            this.SetTitle("Get Suggestions");

            TextBlock suggestionTextBlock = new TextBlock();
            suggestionTextBlock.Text = "Suggest";

            this.suggestionButton = new Button();
            ButtonLayout buttonLayout = new ButtonLayout(this.suggestionButton, new TextblockLayout(suggestionTextBlock));

            this.categoryBox = new ActivityNameEntryBox("Category (optional)");

            // It's acceptable to put the Suggest button to the side of its text box, but that looks worse and we count it as uncentered
            GridLayout horizontalSelectionLayout = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Get_UnCentered_LayoutScore(1));
            horizontalSelectionLayout.AddLayout(buttonLayout);
            horizontalSelectionLayout.AddLayout(this.categoryBox);
            this.selectorLayout = horizontalSelectionLayout;


            this.helpWindow = (new HelpWindowBuilder()).AddMessage("Use this page to ask for a suggested activity")
                .AddMessage("You can optionally enter a category (an activity containing other activities) from which to choose the first activity, or leave it blank to consider all activities")
                .AddMessage("Then, push Suggest and you will receive a few suggestions.")
                .AddMessage("Each suggestion will list an activity name, followed by the time to start the activity, an estimate of the probability that you will actually do that activity, and"
            + " an estimate of the rating that you are expected to give to that activity (if you provide a rating).")
                .AddMessage("Enjoy!")
                .Build();

            Button helpButton = new Button();
            
            helpButton.Click += helpButton_Click;
            this.helpButton_layout = new ButtonLayout(helpButton, new TextblockLayout("Help"));

            this.UpdateSuggestions();
        }

        void helpButton_Click(object sender, RoutedEventArgs e)
        {
            this.layoutStack.AddLayout(this.helpWindow);
        }
        public void AddSuggestionClickHandler(RoutedEventHandler e)
        {
            this.suggestionButton.Click += e;
        }
        public string CategoryText
        {
            get
            {
                return this.categoryBox.NameText;
            }
            set
            {
                this.categoryBox.NameText = value;
            }
        }

        public ActivityDatabase ActivityDatabase
        {
            set
            {
                this.categoryBox.Database = value;
            }
        }

        public void RemoveSuggestion(ActivitySuggestion suggestion)
        {
            this.suggestions.Remove(suggestion);
            this.suggestionLayouts.Remove(suggestion);
            this.UpdateSuggestions();
        }

        public void DeclineSuggestion(ActivitySuggestion suggestion)
        {
            this.RemoveSuggestion(suggestion);
            this.recommender.DeclineSuggestion(suggestion);
        }
        public void ClearSuggestions()
        {
            this.suggestions.Clear();
            this.suggestionLayouts.Clear();
            this.UpdateSuggestions();
        }
        public void AddSuggestion(ActivitySuggestion suggestion)
        {
            this.suggestions.Add(suggestion);
            this.UpdateSuggestions();
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
                layouts.AddLast(this.helpButton_layout);
            if (this.errorLayout != null)
                layouts.AddLast(errorLayout);
            foreach (ActivitySuggestion suggestion in this.suggestions)
            {
                layouts.AddLast(this.getLayout(suggestion));
            }
            if (this.suggestions.Count < this.maxNumSuggestions)
                layouts.AddLast(this.selectorLayout);

            GridLayout grid = GridLayout.New(BoundProperty_List.Uniform(layouts.Count), new BoundProperty_List(1), LayoutScore.Zero);
            foreach (LayoutChoice_Set layout in layouts)
            {
                grid.AddLayout(layout);
            }

            this.SetContent(grid);
        }

        private LayoutChoice_Set getLayout(ActivitySuggestion suggestion)
        {
            if (this.suggestionLayouts.ContainsKey(suggestion))
            {
                return this.suggestionLayouts[suggestion];
            }
            return this.makeLayout(suggestion);
        }

        private LayoutChoice_Set makeLayout(ActivitySuggestion suggestion)
        {
            return new LayoutCache(new SuggestionView(suggestion, this));
        }


        LayoutChoice_Set selectorLayout;
        Button suggestionButton;
        ActivityNameEntryBox categoryBox;
        LayoutChoice_Set helpWindow;
        LayoutChoice_Set helpButton_layout;
        LayoutStack layoutStack;
        List<ActivitySuggestion> suggestions = new List<ActivitySuggestion>();
        Dictionary<ActivitySuggestion, LayoutChoice_Set> suggestionLayouts = new Dictionary<ActivitySuggestion, LayoutChoice_Set>();
        int maxNumSuggestions = 4;
        ActivityRecommender recommender;
        LayoutChoice_Set errorLayout;
    }
}