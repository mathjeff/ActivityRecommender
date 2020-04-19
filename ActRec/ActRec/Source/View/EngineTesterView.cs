using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class EngineTesterView : TitledControl
    {
        public EngineTesterView(ActivityRecommender recommender, LayoutStack layoutStack)
        {
            this.activityRecommender = recommender;
            this.layoutStack = layoutStack;

            this.SetTitle("For Developers: Back-Testing");

            TextblockLayout explanationLayout = new TextblockLayout("This feature is incredibly slow (may take several minutes) and mostly intended for developers. " 
                + "It does back-testing using all of your past data and computes the overall accuracy of the predictions that ActivityRecommender would make if it "
                + "were given all of the same data again (the intent is that you will change something about ActivityRecommender and then run this to see if "
                + "it made ActivityRecommender's predictions more accurate). Do you want to continue?");

            Button confirmButton = new Button();
            confirmButton.Text = "Compute Overall Accuracy";
            confirmButton.Clicked += ConfirmButton_Clicked;

            LayoutChoice_Set gridLayout = new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(explanationLayout)
                .AddLayout(new ButtonLayout(confirmButton))
                .Build();
            this.SetContent(gridLayout);
        }

        private void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            DateTime start = DateTime.Now;
            EngineTesterResults results = this.activityRecommender.TestEngine();
            DateTime end = DateTime.Now;
            TimeSpan duration = end.Subtract(start);

            HelpWindowBuilder builder = new HelpWindowBuilder().AddMessage("Results");
            builder
                .AddMessage("typical longtermHappinessPredictionIfSuggested error = " + results.Longterm_PredictionIfSuggested_Error)
                .AddMessage("typical longtermHappinessPredictionIfParticipated error = " + results.Longterm_PredictionIfParticipated_Error)
                .AddMessage("typicalScoreError = " + results.TypicalScoreError)
                .AddMessage("equivalentWeightedProbability = " + results.TypicalProbability)
                .AddMessage("typicalEfficiencyError = " + results.TypicalEfficiencyError)
                .AddMessage("typical longtermEfficiencyIfParticipated error = " + results.Longterm_EfficiencyIfPredicted_Error);

            ParticipationSurprise surprise = results.ParticipationHavingMostSurprisingScore;
            if (surprise != null)
            {
                builder = builder.AddMessage("Most surprising participation: " + surprise.ActivityDescriptor.ActivityName + " at " +
                    surprise.Date + ": expected rating " + surprise.ExpectedRating + ", got " + surprise.ActualRating);
            }

            builder = builder.AddMessage("Computed results in " + duration);
            LayoutChoice_Set resultsView = builder.Build();

            this.layoutStack.AddLayout(resultsView, "Test Results");
        }

        ActivityRecommender activityRecommender;
        LayoutStack layoutStack;
    }
}
