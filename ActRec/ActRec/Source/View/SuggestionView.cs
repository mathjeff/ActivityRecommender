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

        public event VisitParticipationScreenHandler VisitParticipationScreen;
        public delegate void VisitParticipationScreenHandler();

        public SuggestionView(ActivitySuggestion suggestion, bool isFirstSuggestion, bool repeatingDeclinedSuggestion, LayoutStack layoutStack)
        {
            this.suggestion = suggestion;
            this.layoutStack = layoutStack;


            // set up grid for holding buttons and content
            // have the X button use a certain amount of space on the right
            BoundProperty_List widths = new BoundProperty_List(3);
            int titleWidthWeight = 6;
            widths.SetPropertyScale(0, 1);
            widths.SetPropertyScale(1, titleWidthWeight);
            widths.SetPropertyScale(2, 1);
            widths.BindIndices(0, 1);
            widths.BindIndices(0, 2);
            GridLayout mainGrid = GridLayout.New(new BoundProperty_List(1), widths, LayoutScore.Zero);

            // make a doNow button if needed
            if (isFirstSuggestion)
            {
                Button doNowButton = new Button();
                doNowButton.Clicked += DoNowButton_Clicked;
                ButtonLayout doButtonLayout = new ButtonLayout(doNowButton, "Doing it?");
                mainGrid.PutLayout(doButtonLayout, 0, 0);
            }
            else
            {
                mainGrid.PutLayout(new TextblockLayout(), 0, 0);
            }

            // add content
            mainGrid.PutLayout(new TextblockLayout(this.summarize(suggestion, repeatingDeclinedSuggestion)), 1, 0);

            // Add buttons on the right
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
            mainGrid.PutLayout(sideBuilder.Build(), 2, 0);
            this.SubLayout = mainGrid;
        }

        // given a probability, returns an adjective to describe it
        private string getProbabilityAdjective(double probability)
        {
            //if (probability <= 0)
            //    return "won't";
            if (probability <= 0.1)
                return "could";
            if (probability <= 0.2)
                return "will occasionally";
            if (probability <= 0.3)
                return "can sometimes";
            if (probability <= 0.4)
                return "might";
            if (probability <= 0.5)
                return "may";
            if (probability <= 0.6)
                return "often";
            if (probability < 0.7)
                return "are likely to";
            if (probability <= 0.8)
                return "usually";
            if (probability <= 0.9)
                return "probably will";
            if (probability < 1)
                return "will almost definitely";
            return "will certainly";
        }

        // given a rating (relative to the average rating), returns a verb to describe it
        private string getRatingVerb(double ratingTimesAverage)
        {
            if (ratingTimesAverage <= 0.3)
                return "despise";
            if (ratingTimesAverage <= 0.5)
                return "tolerate";
            if (ratingTimesAverage <= 0.6)
                return "dislike";
            if (ratingTimesAverage <= 0.7)
                return "not like";
            if (ratingTimesAverage <= 0.8)
                return "accept";
            if (ratingTimesAverage <= 0.9)
                return "do";
            if (ratingTimesAverage <= 1)
                return "appreciate";
            if (ratingTimesAverage <= 1.1)
                return "like";
            if (ratingTimesAverage <= 1.2)
                return "enjoy";
            if (ratingTimesAverage <= 1.5)
                return "love";
            return "be overjoyed with";
        }

        private string summarize(ActivitySuggestion suggestion, bool repeatingDeclinedSuggestion)
        {
            // Summarize participation probability and predicted score
            string text = "You";
            string longTimeFormat = "HH:mm:ss";
            string shortTimeFormat = "HH:mm";
            if (!suggestion.Skippable)
            {
                // If this is an unskippable experiment, then remind the user that they promised to do it
                text += " promised to do " + suggestion.ActivityDescriptor.ActivityName + " at " + suggestion.StartDate.ToString(shortTimeFormat) + ".";
                if (suggestion.PredictedScoreDividedByAverage != null)
                {
                    // Also tell the user how we think they'll feel about it
                    text += " I think you will " + this.getRatingVerb(suggestion.PredictedScoreDividedByAverage.Value) + " it.";
                }
                text += " Get started!";
            }
            else
            {
                // For a normal suggestion, we tell them how likely we think it is that they will do it, and
                // how we think they will feel about it
                if (repeatingDeclinedSuggestion)
                    text = "No, really: you";
                if (suggestion.ParticipationProbability != null && suggestion.PredictedScoreDividedByAverage != null)
                {
                    text += " " + this.getProbabilityAdjective(suggestion.ParticipationProbability.Value);
                    text += " " + this.getRatingVerb(suggestion.PredictedScoreDividedByAverage.Value);
                }
                else
                {
                    // No data at the moment
                    text += " may do";
                }
                text += " " + suggestion.ActivityDescriptor.ActivityName;

                // Also tell the user when we think they'll start and how long we think they'll do it
                // Include the seconds field only for participations shorter than 1 minute
                string timeFormat;
                if (suggestion.Duration.HasValue && suggestion.Duration.Value.CompareTo(TimeSpan.FromMinutes(1)) >= 0)
                    timeFormat = shortTimeFormat;
                else
                    timeFormat = longTimeFormat;
                string whenText = suggestion.StartDate.ToString(timeFormat);
                if (suggestion.EndDate.HasValue)
                    whenText += " - " + suggestion.EndDate.Value.ToString(timeFormat);

                text += " " + whenText + ".";
            }
            return text;
        }

        private void DoNowButton_Clicked(object sender, EventArgs e)
        {
            if (this.VisitParticipationScreen != null)
                this.VisitParticipationScreen.Invoke();
        }

        private void ExplainWhyYouCantSkipButton_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(new TextblockLayout("This suggestion is part of an experiment, so you're not allowed to skip it. " +
                "After spending some time on it, if you haven't completed it, then go to the participations page to record having worked on it. " +
                "That's where you can specify that you didn't complete it."), "Help");
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

            TextblockLayout nameLayout = new TextblockLayout(propertyName);
            nameLayout.AlignHorizontally(TextAlignment.Start);
            nameLayout.AlignVertically(TextAlignment.Center);
            centeredGrid.AddLayout(nameLayout);
            uncenteredGrid.AddLayout(nameLayout);

            TextblockLayout valueLayout = new TextblockLayout(propertyValue);
            valueLayout.AlignHorizontally(TextAlignment.Center);
            valueLayout.AlignVertically(TextAlignment.Center);
            centeredGrid.AddLayout(valueLayout);
            uncenteredGrid.AddLayout(valueLayout);

            LayoutUnion row = new LayoutUnion(centeredGrid, uncenteredGrid);

            return row;
            
        }
        
        Button cancelButton;
        Button explainWhyYouCantSkipButton;
        Button justifyButton;
        Button experimentButton;
        ActivitySuggestion suggestion;
        LayoutStack layoutStack;
        
    }
}
