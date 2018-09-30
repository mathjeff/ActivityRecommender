using System;
using VisiPlacement;
using Xamarin.Forms;

// a SuggestionView displays one suggested Activity and some details of the suggestion
namespace ActivityRecommendation
{
    class SuggestionView : ContainerLayout
    {
        public event SuggestionDismissed Dismissed;
        public delegate void SuggestionDismissed(ActivitySuggestion suggestion);

        public event RequestedExperiment ExperimentRequested;
        public delegate void RequestedExperiment(ActivitySuggestion suggestion);

        public event JustifySuggestionHandler JustifySuggestion;
        public delegate void JustifySuggestionHandler(ActivitySuggestion suggestion);

        public SuggestionView(ActivitySuggestion suggestion, LayoutStack layoutStack)
        {
            this.suggestion = suggestion;
            this.layoutStack = layoutStack;

            // have the X button use a certain amount of space on the right
            BoundProperty_List widths = new BoundProperty_List(2);

            int titleWidthWeight = 6;
            widths.SetPropertyScale(0, titleWidthWeight + 1);
            widths.SetPropertyScale(1, 1);
            widths.BindIndices(0, 1);
            GridLayout mainGrid = GridLayout.New(new BoundProperty_List(1), widths, LayoutScore.Zero);
            Vertical_GridLayout_Builder contentBuilder = new Vertical_GridLayout_Builder().Uniform();
            
            // Attempt to center the activity name, but allow it to be off-center if necessary
            TextblockLayout titleLayout = new TextblockLayout(suggestion.ActivityDescriptor.ActivityName, TextAlignment.Center);
            BoundProperty_List titleComponentWidths = new BoundProperty_List(2);
            titleComponentWidths.BindIndices(0, 1);
            titleComponentWidths.SetPropertyScale(0, 1);
            titleComponentWidths.SetPropertyScale(1, titleWidthWeight);
            GridLayout centeredTitle = GridLayout.New(BoundProperty_List.Uniform(1), titleComponentWidths, LayoutScore.Zero);
            centeredTitle.PutLayout(titleLayout, 1, 0);
            GridLayout offsetTitle = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(1), LayoutScore.Get_UnCentered_LayoutScore(1));
            offsetTitle.PutLayout(titleLayout, 0, 0);
            contentBuilder.AddLayout(new LayoutUnion(centeredTitle, offsetTitle));

            // Add the remaining fields
            
            // Include the seconds field only for participations shorter than 1 minute
            string timeFormat = "HH:mm:ss";
            if (suggestion.Duration.HasValue && suggestion.Duration.Value.CompareTo(TimeSpan.FromMilliseconds(1)) >= 0)
                timeFormat = "HH:mm";
            string whenText = suggestion.StartDate.ToString(timeFormat);
            if (suggestion.EndDate.HasValue)
                whenText += " - " + suggestion.EndDate.Value.ToString(timeFormat);
            contentBuilder.AddLayout(this.make_displayField("When:", whenText));
            if (suggestion.ParticipationProbability != null)
                contentBuilder.AddLayout(this.make_displayField("Probability:", Math.Round(suggestion.ParticipationProbability.Value, 3).ToString()));
            if (suggestion.PredictedScoreDividedByAverage != null)
                contentBuilder.AddLayout(this.make_displayField("Rating:", Math.Round(suggestion.PredictedScoreDividedByAverage.Value, 3).ToString() + " x avg"));
            this.contentGrid = contentBuilder.Build();

            // Add buttons
            mainGrid.AddLayout(this.contentGrid);
            this.cancelButton = new Button();
            this.cancelButton.Clicked += cancelButton_Click;
            this.justifyButton = new Button();
            this.justifyButton.Clicked += justifyButton_Click;
            this.experimentButton = new Button();
            this.experimentButton.Clicked += ExperimentButton_Clicked;
            this.explainWhyYouCantSkipButton = new Button();
            this.explainWhyYouCantSkipButton.Clicked += ExplainWhyYouCantSkipButton_Clicked;
            ButtonLayout cancelLayout;
            if (suggestion.Skippable)
                cancelLayout = new ButtonLayout(this.cancelButton, "X");
            else
                cancelLayout = new ButtonLayout(this.explainWhyYouCantSkipButton, "!");
            Vertical_GridLayout_Builder sideBuilder = new Vertical_GridLayout_Builder()
                .Uniform()
                .AddLayout(cancelLayout);
            if (this.suggestion.PredictedScoreDividedByAverage != null)
                sideBuilder.AddLayout(new ButtonLayout(this.justifyButton, "?"));
            mainGrid.AddLayout(sideBuilder.Build());
            this.SubLayout = mainGrid;

        }

        private void ExplainWhyYouCantSkipButton_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(new TextblockLayout("This suggestion is part of an experiment, so you're not allowed to skip it. " +
                "After spending some time on it, if you haven't completed it, then go to the participations page to record having worked on it. " +
                "That's where you can specify that you didn't complete it."));
        }

        private void ExperimentButton_Clicked(object sender, EventArgs e)
        {
            if (this.ExperimentRequested != null)
                this.ExperimentRequested.Invoke(this.suggestion);
        }

        void justifyButton_Click(object sender, EventArgs e)
        {
            if (this.JustifySuggestion != null)
                this.JustifySuggestion.Invoke(this.suggestion);
        }

        void cancelButton_Click(object sender, EventArgs e)
        {
            if (this.Dismissed != null)
                this.Dismissed.Invoke(this.suggestion);
        }

        private LayoutChoice_Set make_displayField(string propertyName, string propertyValue)
        {
            GridLayout centeredGrid = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            GridLayout uncenteredGrid = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Get_UnCentered_LayoutScore(1));

            Label nameBlock = new Label();
            nameBlock.Text = propertyName;
            nameBlock.HorizontalTextAlignment = TextAlignment.Start;
            nameBlock.VerticalTextAlignment = TextAlignment.Center;
            //nameBlock.TextAlignment = System.Windows.TextAlignment.Left;
            //nameBlock.VerticalAlignment = VerticalAlignment.Center;
            TextblockLayout nameLayout = new TextblockLayout(nameBlock);
            centeredGrid.AddLayout(nameLayout);
            uncenteredGrid.AddLayout(nameLayout);

            Label valueBlock = new Label();
            valueBlock.Text = propertyValue;
            valueBlock.HorizontalTextAlignment = TextAlignment.Center;
            valueBlock.VerticalTextAlignment = TextAlignment.Center;
            //valueBlock.TextAlignment = System.Windows.TextAlignment.Center;
            //valueBlock.VerticalAlignment = VerticalAlignment.Center;
            TextblockLayout valueLayout = new TextblockLayout(valueBlock);
            centeredGrid.AddLayout(valueLayout);
            uncenteredGrid.AddLayout(valueLayout);

            LayoutUnion row = new LayoutUnion(centeredGrid, uncenteredGrid);

            return row;
            
        }
        
        GridLayout contentGrid;
        Button cancelButton;
        Button explainWhyYouCantSkipButton;
        Button justifyButton;
        Button experimentButton;
        ActivitySuggestion suggestion;
        LayoutStack layoutStack;
        
    }
}
