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
            if (probability <= 0.1)
                return "could be";
            if (probability <= 0.2)
                return "will occasionally be";
            if (probability <= 0.3)
                return "can potentially be";
            if (probability <= 0.4)
                return "might be";
            if (probability <= 0.5)
                return "may be";
            if (probability <= 0.6)
                return "often will be";
            if (probability < 0.7)
                return "is likely to be";
            if (probability <= 0.8)
                return "usually will be";
            if (probability <= 0.9)
                return "probably will be";
            if (probability < 1)
                return "will almost definitely be";
            return "will certainly be";
        }

        // given a rating (relative to the average rating), returns a verb to describe it
        private string getRatingAdjective(double ratingTimesAverage)
        {
            if (ratingTimesAverage <= 0.3)
                return "miserable";
            if (ratingTimesAverage <= 0.5)
                return "terrible";
            if (ratingTimesAverage <= 0.6)
                return "poor";
            if (ratingTimesAverage <= 0.7)
                return "bad";
            if (ratingTimesAverage <= 0.8)
                return "annoying";
            if (ratingTimesAverage <= 0.9)
                return "decent";
            if (ratingTimesAverage <= 1)
                return "worthwhile";
            if (ratingTimesAverage <= 1.1)
                return "ok";
            if (ratingTimesAverage <= 1.2)
                return "nice";
            if (ratingTimesAverage <= 1.3)
                return "good";
            if (ratingTimesAverage <= 1.4)
                return "great";
            if (ratingTimesAverage <= 1.5)
                return "awesome";
            if (ratingTimesAverage <= 1.6)
                return "incredible";
            return "spectacular";
        }

        private string decapitalize(string text)
        {
            return text.Substring(0, 1).ToLower() + text.Substring(1);
        }

        private string summarize(ActivitySuggestion suggestion, bool repeatingDeclinedSuggestion)
        {
            // Summarize participation probability and predicted score
            string text;
            string shortTimeFormat = "HH:mm";
            string longTimeFormat = "HH:mm:ss";

            string probabilityPhrase = "";
            string ratingAdjective = "";
            // How we expect the user to feel about this
            if (suggestion.ParticipationProbability != null && suggestion.PredictedScoreDividedByAverage != null)
            {
                probabilityPhrase = this.getProbabilityAdjective(suggestion.ParticipationProbability.Value);
                ratingAdjective = this.getRatingAdjective(suggestion.PredictedScoreDividedByAverage.Value);
            }
            if (!suggestion.Skippable)
            {
                // If this is an unskippable experiment, then remind the user that they promised to do it
                text = "You promised to do " + suggestion.ActivityDescriptor.ActivityName + " at " + suggestion.StartDate.ToString(shortTimeFormat) + ".";
                if (suggestion.PredictedScoreDividedByAverage != null)
                {
                    // Also tell the user how we think they'll feel about it
                    text += "I think it will be " + ratingAdjective + ".";
                }
                text += " Get started!";
            }
            else
            {
                // For a normal suggestion, we tell them how likely we think it is that they will do it, and
                // how we think they will feel about it

                // Optional emphasis
                if (repeatingDeclinedSuggestion)
                    text = "No, really: I think ";
                else
                    text = "";
                // activity name
                text += suggestion.ActivityDescriptor.ActivityName;

                // Start and end times
                string timeFormat;
                if (suggestion.Duration.HasValue && suggestion.Duration.Value.CompareTo(TimeSpan.FromMinutes(1)) >= 0)
                    timeFormat = shortTimeFormat;
                else
                    timeFormat = longTimeFormat;
                string whenText = suggestion.StartDate.ToString(timeFormat);
                if (suggestion.EndDate.HasValue)
                    whenText += " - " + suggestion.EndDate.Value.ToString(timeFormat);
                text += " " + whenText;

                // How we expect the user to feel about this
                if (suggestion.ParticipationProbability != null && suggestion.PredictedScoreDividedByAverage != null)
                {
                    text += "\n" + probabilityPhrase;
                    text += " " + ratingAdjective;
                    text += " because of " + this.decapitalize(suggestion.MostSignificantJustification.Label) + ".";
                }
                else
                {
                    // No data at the moment
                    text += " could be nice.";
                }
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
