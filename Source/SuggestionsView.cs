using System;
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
            //this.content = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);

            TextBlock suggestionTextBlock = new TextBlock();
            suggestionTextBlock.Text = "Suggest";

            this.suggestionButton = new Button();
            ButtonLayout buttonLayout = new ButtonLayout(this.suggestionButton, new TextblockLayout(suggestionTextBlock));

            this.categoryBox = new ActivityNameEntryBox("Category (optional)");

            /* // Try to put the Suggest button above its text box
            GridLayout verticalSelectionLayout = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);
            verticalSelectionLayout.AddLayout(buttonLayout);
            verticalSelectionLayout.AddLayout(this.categoryBox);
            */
            // It's acceptable to put the Suggest button to the side of its text box, but that looks worse and we count it as uncentered
            GridLayout horizontalSelectionLayout = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Get_UnCentered_LayoutScore(1));
            horizontalSelectionLayout.AddLayout(buttonLayout);
            horizontalSelectionLayout.AddLayout(this.categoryBox);
            this.selectorLayout = horizontalSelectionLayout;
            //LayoutUnion selectionLayout = new LayoutUnion(verticalSelectionLayout, horizontalSelectionLayout);

            //this.content.PutLayout(horizontalSelectionLayout, 0, 1);

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
            //this.SetContent(this.content);
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

        private void UpdateSuggestions()
        {
            this.Update_Suggestion_StartTimes();
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
            foreach (ActivitySuggestion suggestion in this.suggestions)
            {
                layouts.AddLast(this.getLayout(suggestion));
            }
            if (this.suggestions.Count < this.maxNumSuggestions)
                layouts.AddLast(this.selectorLayout);
            if (this.suggestions.Count == 0)
                layouts.AddLast(this.helpButton_layout);

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

        /*public Composite_ActivitySuggestion Suggestion
        {
            set
            {
                Composite_ActivitySuggestion rootSuggestion = value;
                int numColumns = rootSuggestion.CountNumLeaves();
                int numRows = rootSuggestion.CountNumLevelsFromLeaf() + 1;
                // TODO figure out how to make the grid be fast and look nice without enforcing here that everything have the same height
                BoundProperty_List rowHeights = BoundProperty_List.Uniform(numRows);
                for (int i = rowHeights.NumProperties - 1; i >= 1; i--)
                {
                    rowHeights.BindIndices(i - 1, i);
                }
                GridLayout newGrid = GridLayout.New(rowHeights, BoundProperty_List.Uniform(numColumns), LayoutScore.Zero);
                for (int i = 0; i < numColumns; i++)
                {
                    // TODO: compute the title in a more maintainable manner
                    String title = null;
                    switch (i)
                    {
                        case 0:
                            title = "Best For Longterm";
                            break;
                        case 1:
                            title = "Best For Shortterm";
                            break;
                    }
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = title;
                    textBlock.TextAlignment = TextAlignment.Center;
                    newGrid.PutLayout(new TextblockLayout(textBlock), i, 0);
                }
                int x, y;
                x = y = 0;
                List<List<ActivitySuggestion>> suggestionStack = new List<List<ActivitySuggestion>>();
                List<ActivitySuggestion> row = new List<ActivitySuggestion>();
                row.Add(rootSuggestion);
                suggestionStack.Add(row);
                while (true)
                {
                    List<ActivitySuggestion> latestSuggestions = suggestionStack[suggestionStack.Count - 1];
                    while (latestSuggestions.Count <= 0)
                    {
                        suggestionStack.RemoveAt(suggestionStack.Count - 1);
                        if (suggestionStack.Count == 0)
                            break;
                        latestSuggestions = suggestionStack[suggestionStack.Count - 1];
                        y--;
                    }
                    if (suggestionStack.Count == 0)
                        break;

                    ActivitySuggestion currentSuggestion = latestSuggestions[0];
                    if (currentSuggestion.ActivityDescriptor != null)
                    {
                        // put this item into the tree
                        newGrid.PutLayout(new SuggestionView(currentSuggestion), x, y);
                    }
                    // remove the current suggestion because we don't need it any more
                    suggestionStack[suggestionStack.Count - 1].RemoveAt(0);
                    // follow the tree down another level
                    if (currentSuggestion is Composite_ActivitySuggestion)
                    {
                        Composite_ActivitySuggestion converted = currentSuggestion as Composite_ActivitySuggestion;
                        suggestionStack.Add(new List<ActivitySuggestion>(converted.ChildSuggestions));
                        y++;
                    }
                    if (currentSuggestion.CountNumLevelsFromLeaf() <= 0)
                        x++;
                }
                // update our contents
                this.content.PutLayout(newGrid, 0, 0);
            }
        }*/

        LayoutChoice_Set selectorLayout;
        Button suggestionButton;
        ActivityNameEntryBox categoryBox;
        //GridLayout content;
        LayoutChoice_Set helpWindow;
        LayoutChoice_Set helpButton_layout;
        LayoutStack layoutStack;
        List<ActivitySuggestion> suggestions = new List<ActivitySuggestion>();
        Dictionary<ActivitySuggestion, LayoutChoice_Set> suggestionLayouts = new Dictionary<ActivitySuggestion, LayoutChoice_Set>();
        int maxNumSuggestions = 4;
        ActivityRecommender recommender;
    }
}
