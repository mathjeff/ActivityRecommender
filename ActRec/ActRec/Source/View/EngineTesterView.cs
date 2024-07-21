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
            confirmButton.Clicked += ConfirmButton_Clicked;

            LayoutChoice_Set gridLayout = new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(explanationLayout)
                .AddLayout(new ButtonLayout(confirmButton, "Compute Overall Accuracy"))
                .Build();
            this.SetContent(gridLayout);
        }

        private void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            DateTime start = DateTime.Now;
            EngineTesterResults results = this.activityRecommender.TestEngine();
            DateTime end = DateTime.Now;
            TimeSpan duration = end.Subtract(start);

            GridLayout_Builder builder = new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(new TextblockLayout("Results"))
                .AddLayout(this.resultLayout("typical longtermHappinessPredictionIfSuggested error:\n", results.Longterm_PredictionIfSuggested_Error))
                .AddLayout(this.resultLayout("typical longtermHappinessPredictionIfParticipated error:\n", results.Longterm_PredictionIfParticipated_Error))
                .AddLayout(this.resultLayout("typicalScoreError:\n", results.TypicalScoreError))
                .AddLayout(new TextblockLayout("equivalentWeightedProbability:\n" + results.TypicalProbability))
                .AddLayout(this.resultLayout("typicalEfficiencyError:\n", results.TypicalEfficiencyError))
                .AddLayout(this.resultLayout("typical longtermEfficiencyIfParticipated error:\n", results.Longterm_EfficiencyIfPredicted_Error));

            ParticipationSurprise surprise = results.ParticipationHavingMostSurprisingScore;
            if (surprise != null)
            {
                builder = builder.AddLayout(new TextblockLayout("Most surprising participation: " + surprise.ActivityDescriptor.ActivityName + " at " +
                    surprise.Date + ": expected rating " + surprise.ExpectedRating + ", got " + surprise.ActualRating));
            }

            builder = builder.AddLayout(new TextblockLayout("Computed results in " + duration));
            LayoutChoice_Set resultsView = builder.Build();

            this.layoutStack.AddLayout(resultsView, "Test Results");
        }

        private LayoutChoice_Set resultLayout(string label, PredictionErrors errors)
        {
            string text = label + errors.ToString();
            Button button = new Button();
            button.Clicked += ResultButton_Clicked;
            this.errorsByButton[button] = errors;
            ButtonLayout buttonLayout = new ButtonLayout(button, text);
            return buttonLayout;
        }

        private void ResultButton_Clicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            PredictionErrors errors = this.errorsByButton[button];
            this.showErrors(errors);
        }

        private void showErrors(PredictionErrors errors)
        {
            PlotView plotView = new PlotView();
            List<Datapoint> correctValues = new List<Datapoint>();
            List<Datapoint> predictedValues = new List<Datapoint>();
            List<Datapoint> predictedPlusStdDev = new List<Datapoint>();
            if (!double.IsInfinity(errors.MinAllowedValue))
                plotView.MinY = errors.MinAllowedValue;
            if (!double.IsInfinity(errors.MaxAllowedValue))
                plotView.MaxY = errors.MaxAllowedValue;
            if (plotView.MinY != null && plotView.MaxY != null)
            {
                List<double> subdivisions = new List<double>();
                for (int i = 0; i < 10; i++)
                {
                    double maxWeight = (double)i / (double)10;
                    double minWeight = (1 - maxWeight);
                    double division = plotView.MinY.Value * minWeight + plotView.MaxY.Value * maxWeight;
                    subdivisions.Add(division);
                }
                plotView.YAxisSubdivisions = subdivisions;
            }

            DateTime referenceDate = new DateTime(2000, 1, 1);
            foreach (PredictionError error in errors.All)
            {
                double x = error.When.Subtract(referenceDate).TotalSeconds;
                correctValues.Add(new Datapoint(x, error.ActualMean, 1));
                predictedValues.Add(new Datapoint(x, error.Predicted.Mean, 1));
                predictedPlusStdDev.Add(new Datapoint(x, error.Predicted.Mean + error.Predicted.StdDev, 1));
            }
            if (errors.All.Count > 0)
            {
                double minX = correctValues[0].Input;
                double maxX = correctValues[correctValues.Count - 1].Input;
                plotView.XAxisSubdivisions = TimeProgression.AbsoluteTime.GetNaturalSubdivisions(minX, maxX);
                plotView.MinX = minX;
                plotView.MaxX = maxX;

            }
            // draw predicted values first (which have lots of noise and take up lots of visual space) so the correct values draw over them (and are visible too)
            plotView.AddSeries(predictedValues, false, true, 2);
            plotView.AddSeries(predictedPlusStdDev, false, true, 4);
            plotView.AddSeries(correctValues, false, true, 0);

            ImageLayout imageLayout = new ImageLayout(plotView, LayoutScore.Get_UsedSpace_LayoutScore(1));
            TextblockLayout legend = new TextblockLayout("Prediction errors over time. Green: actual value. Blue: predicted value. White: prediction mean + stddev.");

            LayoutChoice_Set layout = new Vertical_GridLayout_Builder()
                .AddLayout(imageLayout)
                .AddLayout(legend)
                .BuildAnyLayout();
            this.layoutStack.AddLayout(layout, "errors");
        }

        ActivityRecommender activityRecommender;
        LayoutStack layoutStack;
        Dictionary<Button, PredictionErrors> errorsByButton = new Dictionary<Button, PredictionErrors>();
    }
}
