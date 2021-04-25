using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ParticipationView : ContainerLayout
    {
        public ParticipationView(Participation participation, ScoreSummarizer ratingSummarizer, LayoutStack layoutStack, Engine engine, bool showCalculatedValues = true)
        {
            this.participation = participation;
            this.engine = engine;

            Vertical_GridLayout_Builder mainGrid_builder = new Vertical_GridLayout_Builder();
            mainGrid_builder.AddLayout(new TextblockLayout(participation.ActivityDescriptor.ActivityName, 30));
            mainGrid_builder.AddLayout(new TextblockLayout(participation.StartDate.ToString() + " - " + participation.EndDate.ToString(), 16));
            if (showCalculatedValues)
            {
                if (participation.GetAbsoluteRating() != null)
                    mainGrid_builder.AddLayout(new TextblockLayout("Score: " + participation.GetAbsoluteRating().Score, 16));
                if (participation.RelativeEfficiencyMeasurement != null)
                {
                    string message = "Efficiency: " + participation.RelativeEfficiencyMeasurement.RecomputedEfficiency.Mean;
                    mainGrid_builder.AddLayout(new HelpButtonLayout(message, new ExperimentResultsView(participation), layoutStack, 16));
                }
                Distribution netPresentHappiness = ratingSummarizer.GetValueDistributionForDates(participation.StartDate, ratingSummarizer.LatestKnownDate, true, true);
                if (netPresentHappiness.Weight > 0)
                    mainGrid_builder.AddLayout(new TextblockLayout("Net present happiness: " + netPresentHappiness.Mean));
            }
            if (participation.Comment != null)
                mainGrid_builder.AddLayout(new TextblockLayout("Comment: " + participation.Comment, 16));
            if (showCalculatedValues)
            {
                this.feedbackHolder = new ContainerLayout();
                Button feedbackButton = new Button();
                feedbackButton.Clicked += FeedbackButton_Clicked;
                this.feedbackHolder.SubLayout = new ButtonLayout(feedbackButton, "Compute Feedback");
                mainGrid_builder.AddLayout(this.feedbackHolder);
            }
            this.SubLayout = mainGrid_builder.Build();
        }

        private void FeedbackButton_Clicked(object sender, EventArgs e)
        {
            this.updateFeedback();
        }

        private void updateFeedback()
        {
            Activity activity = this.engine.ActivityDatabase.ResolveDescriptor(this.participation.ActivityDescriptor);
            ParticipationFeedback feedback = this.engine.computeStandardParticipationFeedback(activity, this.participation.StartDate, this.participation.EndDate);
            string text;
            if (feedback == null)
                text = "Sorry, not enough data for feedback!";
            else
                text = "Feedback: " + feedback.Summary;
            this.feedbackHolder.SubLayout = new TextblockLayout(text);
        }
        ContainerLayout feedbackHolder;
        Engine engine;
        Participation participation;
    }
}
