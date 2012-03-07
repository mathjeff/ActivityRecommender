using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    class SuggestionView : TitledControl
    {
        public SuggestionView()
        {
            this.SetTitle("Get Suggestions");
            DisplayGrid content = new DisplayGrid(7, 1);

            this.suggestionButton = new ResizableButton();
            this.suggestionButton.Content = "Suggest";
            this.suggestionButton.VerticalAlignment = VerticalAlignment.Center;
            //this.suggestionButton.HorizontalAlignment = HorizontalAlignment.Center;
            this.suggestionButton.Width = 100;
            this.suggestionButton.Height = 30;
            content.AddItem(this.suggestionButton);

            this.categoryBox = new ActivityNameEntryBox("from category (optional)");
            content.AddItem(this.categoryBox);


            this.suggestionNameBlock = new TitledTextblock("Suggestion:");
            //this.suggestionNameBlock.Text = "<Push the button to see a suggestion here>";
            //this.suggestionNameBlock.TextAlignment = System.Windows.TextAlignment.Center;
            content.AddItem(this.suggestionNameBlock);

            this.justificationBlock = new TitledTextblock("Primary Justification:");
            //this.justificationBlock.Text = "<When there is a suggestion, a very short explanation will be here>";
            //this.justificationBlock.TextAlignment = System.Windows.TextAlignment.Center;
            content.AddItem(this.justificationBlock);

            this.scoreBlock = new TitledTextblock("Expected Score:");
            //this.scoreBlock.Text = "<This will be an estimate of the rating you will give to the activity if you do it>";
            content.AddItem(this.scoreBlock);

            this.stdDevBlock = new TitledTextblock("Standard Deviation:");
            //this.stdDevBlock.Text = "<This will be an estimate of the uncertainty in the expected score>";
            content.AddItem(this.stdDevBlock);

            this.participationProbabilityBlock = new TitledTextblock("Participation Probability:");
            //this.participationProbabilityBlock.Text = "<This will be an estimate of the probability that you will take the suggestion>";
            content.AddItem(this.participationProbabilityBlock);

            this.ResetText();

            this.SetContent(content);
        }
        public void AddSuggestionClickHandler(RoutedEventHandler e)
        {
            this.suggestionButton.Click += e;
        }
        public string SuggestionText
        {
            get
            {
                return this.suggestionNameBlock.Text;
            }
            set
            {
                this.suggestionNameBlock.Text = value;
            }
        }
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
        public string ParticipationProbabilityText
        {
            set
            {
                this.participationProbabilityBlock.Text = value;
            }
        }
        public ActivityDatabase ActivityDatabase
        {
            set
            {
                this.categoryBox.Database = value;
            }
        }
        public void ResetText()
        {
            this.SuggestionText = "<Click \"Suggest\" for a suggestion>";
            this.JustificationText = "<Here will be a short justification>";
            this.ExpectedScoreText = "<Here will be the expected score>";
            this.ScoreStdDevText = "<Here will be a measure of the uncertainty of the score>";
            this.ParticipationProbabilityText = "<This will be an estimate of the probability that you will take the suggestion>";
        }
        private Button suggestionButton;
        private ActivityNameEntryBox categoryBox;
        private TitledTextblock suggestionNameBlock;
        private TitledTextblock justificationBlock;
        private TitledTextblock scoreBlock;
        private TitledTextblock stdDevBlock;
        private TitledTextblock participationProbabilityBlock;
    }
}
