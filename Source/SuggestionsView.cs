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
        public SuggestionsView()
        {
            this.SetTitle("Get Suggestions");
            this.content = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);

            TextBlock suggestionTextBlock = new TextBlock();
            suggestionTextBlock.Text = "Suggest";

            this.suggestionButton = new Button();
            ButtonLayout buttonLayout = new ButtonLayout(this.suggestionButton, new TextblockLayout(suggestionTextBlock));
            //this.suggestionButton.VerticalAlignment = VerticalAlignment.Center;
            //this.suggestionButton.Width = 100;
            //this.suggestionButton.Height = 30;
            //this.content.AddItem(this.suggestionButton);
            //this.content.AddLayout(buttonLayout);

            this.categoryBox = new ActivityNameEntryBox("from category (optional)");
            //this.content.AddLayout(this.categoryBox);

            GridLayout selectionLayout = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);
            selectionLayout.AddLayout(buttonLayout);
            selectionLayout.AddLayout(this.categoryBox);
            this.content.AddLayout(selectionLayout);

            /*
            this.suggestionNameBlock = new TitledTextblock("Suggested activity:");
            //this.suggestionNameBlock.Text = "<Push the button to see a suggestion here>";
            //this.suggestionNameBlock.TextAlignment = System.Windows.TextAlignment.Center;
            content.AddItem(this.suggestionNameBlock);

            this.justificationBlock = new TitledTextblock("Primary Justification:");
            //this.justificationBlock.Text = "<When there is a suggestion, a very short explanation will be here>";
            //this.justificationBlock.TextAlignment = System.Windows.TextAlignment.Center;
            //content.AddItem(this.justificationBlock);

            this.durationNameBlock = new TitledTextblock("Suggested duration:");
            //this.suggestionNameBlock.Text = "<Push the button to see a suggestion here>";
            //this.suggestionNameBlock.TextAlignment = System.Windows.TextAlignment.Center;
            content.AddItem(this.durationNameBlock);

            this.scoreBlock = new TitledTextblock("Expected Score:");
            //this.scoreBlock.Text = "<This will be an estimate of the rating you will give to the activity if you do it>";
            content.AddItem(this.scoreBlock);

            this.stdDevBlock = new TitledTextblock("Standard Deviation:");
            //this.stdDevBlock.Text = "<This will be an estimate of the uncertainty in the expected score>";
            //content.AddItem(this.stdDevBlock);

            this.participationProbabilityBlock = new TitledTextblock("Participation Probability:");
            //this.participationProbabilityBlock.Text = "<This will be an estimate of the probability that you will take the suggestion>";
            content.AddItem(this.participationProbabilityBlock);

            */

            this.ResetText();

            this.SetContent(this.content);
        }
        public void AddSuggestionClickHandler(RoutedEventHandler e)
        {
            this.suggestionButton.Click += e;
        }
        /*public string SuggestionText
        {
            get
            {
                return this.suggestionNameBlock.Text;
            }
            set
            {
                this.suggestionNameBlock.Text = value;
            }
        }*/
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

        /*
        public string JustificationText
        {
            get
            {
                return this.justificationBlock.Text;
            }
            set
            {
                this.justificationBlock.Text = value;
            }
        }
        public string DurationText
        {
            get
            {
                return this.durationNameBlock.Text;
            }
            set
            {
                this.durationNameBlock.Text = value;
            }
        }
        public string ExpectedScoreText
        {
            set
            {
                this.scoreBlock.Text = value;
            }
        }
        public string ScoreStdDevText
        {
            set
            {
                this.stdDevBlock.Text = value;
            }
        }
        public string ParticipationProbabilityText
        {
            set
            {
                this.participationProbabilityBlock.Text = value;
            }
        }
        */
        public ActivityDatabase ActivityDatabase
        {
            set
            {
                this.categoryBox.Database = value;
            }
        }
        public void ResetText()
        {
            this.Suggestions = null;
            /*
            this.SuggestionText = "<Click \"Suggest\" for a suggestion>";
            this.JustificationText = "<Here will be a short justification>";
            this.DurationText = "<Here will be the recommended time to spend on the activity";
            this.ExpectedScoreText = "<Here will be the expected score>";
            this.ScoreStdDevText = "<Here will be a measure of the uncertainty of the score>";
            this.ParticipationProbabilityText = "<This will be an estimate of the probability that you will take the suggestion>";
            */
        }
        public List<ActivitySuggestion> Suggestions
        {
            set
            {
                // TODO: change this display grid with a ScrollView (which does not exist yet)
                // set up a new grid to hold the new suggestions
                List<ActivitySuggestion> newSuggestions = value;
                GridLayout newGrid = null;
                if (newSuggestions != null) 
                {
                    newGrid = GridLayout.New(BoundProperty_List.Uniform(newSuggestions.Count), new BoundProperty_List(1), LayoutScore.Zero);
                    foreach (ActivitySuggestion suggestion in newSuggestions)
                    {
                        SuggestionView subView = new SuggestionView(suggestion);
                        newGrid.AddLayout(subView);
                    }
                }
                // update our contents
                this.content.PutLayout(newGrid, 0, 1);
            }
        }
        public Composite_ActivitySuggestion Suggestion
        {
            set
            {
                Composite_ActivitySuggestion rootSuggestion = value;
                int numColumns = rootSuggestion.CountNumLeaves();
                int numRows = rootSuggestion.CountNumLevelsFromLeaf() + 1;
                // TODO: compute the title in a more maintainable manner
                BoundProperty_List rowHeights = new BoundProperty_List(numRows);
                for (int i = rowHeights.NumProperties - 1; i >= 1; i--)
                {
                    rowHeights.BindIndices(i - 1, i);
                }
                GridLayout newGrid = GridLayout.New(rowHeights, BoundProperty_List.Uniform(numColumns), LayoutScore.Zero);
                for (int i = 0; i < numColumns; i++)
                {
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
                this.content.PutLayout(newGrid, 0, 1);
            }
        }

        private Button suggestionButton;
        private ActivityNameEntryBox categoryBox;
        private GridLayout content;
        /*private TitledTextblock suggestionNameBlock;
        private TitledTextblock durationNameBlock;
        private TitledTextblock justificationBlock;
        private TitledTextblock scoreBlock;
        private TitledTextblock stdDevBlock;
        private TitledTextblock participationProbabilityBlock;
        private DisplayGrid suggestionsView;
        */
    }
}
