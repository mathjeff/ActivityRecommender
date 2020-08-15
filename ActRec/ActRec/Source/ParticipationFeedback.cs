using ActivityRecommendation.TextSummary;
using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class ParticipationFeedback
    {
        public ParticipationFeedback(Activity activity, string summary, ValueProvider<LayoutChoice_Set> detailsProvider)
        {
            this.Activity = activity;
            this.Summary = summary;
            this.detailsProvider = detailsProvider;
        }
        public Activity Activity { get; set; }

        public string Summary { get; set; }
        private ValueProvider<LayoutChoice_Set> detailsProvider;
        public LayoutChoice_Set Details
        {
            get
            {
                return this.detailsProvider.Get();
            }
        }
    }

    class ParticipationNumericFeedback : ValueProvider<LayoutChoice_Set>
    {
        public ParticipationNumericFeedback()
        {
        }
        public LayoutChoice_Set Get()
        {
            if (this.layout == null)
                this.layout = this.MakeLayout();
            return this.layout;
        }

        public LayoutChoice_Set MakeLayout()
        {
            BoundProperty_List rowHeights = BoundProperty_List.Uniform(10);
            GridLayout detailsGrid = GridLayout.New(rowHeights, new BoundProperty_List(1), LayoutScore.Zero);
            detailsGrid.AddLayout(new TextblockLayout(ChosenActivity.Name + " " +
                TimeFormatter.summarizeTimespan(this.StartDate, this.EndDate) + ": " +
                "You spent " + ParticipationDurationDividedByAverage + " as long as average. I predict:"
                ));

            detailsGrid.AddLayout(new TextblockLayout("I predict:"));
            string funLabel = " * avg fun while doing it";
            detailsGrid.AddLayout(
                new Horizontal_GridLayout_Builder().Uniform()
                .AddLayout(coloredRatio(PredictedValue, ComparisonPredictedValue, PredictedCurrentValueStddev))
                .AddLayout(new TextblockLayout(funLabel))
                .BuildAnyLayout()
            );
            string longtermFunLabel = "days future fun (over next " + Math.Round(UserPreferences.DefaultPreferences.HalfLife.TotalDays / Math.Log(2), 0) + " days)";
            detailsGrid.AddLayout(
                new Horizontal_GridLayout_Builder().Uniform()
                .AddLayout(signedColoredValue(ExpectedFutureFun, ComparisonExpectedFutureFun, ExpectedFutureFunStddev))
                .AddLayout(new TextblockLayout(longtermFunLabel))
                .BuildAnyLayout()
            );
            string efficiencyLabel = "hours future efficiency (over next " + Math.Round(UserPreferences.DefaultPreferences.EfficiencyHalflife.TotalDays / Math.Log(2), 0) + " days)";
            detailsGrid.AddLayout(
                new Horizontal_GridLayout_Builder().Uniform()
                .AddLayout(signedColoredValue(ExpectedEfficiency, ComparisonExpectedEfficiency, ExpectedEfficiencyStddev))
                .AddLayout(new TextblockLayout(efficiencyLabel))
                .BuildAnyLayout()
            );

            detailsGrid.AddLayout(new TextblockLayout("If " + this.ComparisonDate + " had been when you had done this:"));
            detailsGrid.AddLayout(
                new Horizontal_GridLayout_Builder().Uniform()
                .AddLayout(coloredRatio(ComparisonPredictedValue, 1, 0))
                .AddLayout(new TextblockLayout(funLabel))
                .BuildAnyLayout()
            );
            detailsGrid.AddLayout(
                new Horizontal_GridLayout_Builder().Uniform()
                .AddLayout(signedColoredValue(ComparisonExpectedFutureFun, 1, 0))
                .AddLayout(new TextblockLayout(longtermFunLabel))
                .BuildAnyLayout()
            );
            detailsGrid.AddLayout(
                new Horizontal_GridLayout_Builder().Uniform()
                .AddLayout(signedColoredValue(ComparisonExpectedEfficiency, 0, 0))
                .AddLayout(new TextblockLayout(efficiencyLabel))
                .BuildAnyLayout()
            );

            ActivityRequest request = new ActivityRequest();
            request.ActivityToBeat = this.ChosenActivity.MakeDescriptor();
            request.Date = this.StartDate;
            request.RequestedProcessingTime = TimeSpan.FromSeconds(0.5);
            ActivitySuggestion suggestion = this.engine.MakeRecommendation(request);
            Activity betterActivity = this.ActivityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor);
            Prediction betterPrediction = this.engine.Get_OverallHappiness_ParticipationEstimate(betterActivity, this.StartDate);
            string redirectionText;
            Color redirectionColor;
            Distribution betterFutureHappinessImprovementInDays = this.engine.compute_longtermValue_increase_in_days(betterPrediction.Distribution);
            double improvementInDays = Math.Round(betterFutureHappinessImprovementInDays.Mean - this.ExpectedFutureFun, 1);
            if (improvementInDays <= 0)
            {
                if (ExpectedFutureFun >= 0)
                {
                    redirectionText = "Nice! I don't have an idea of anything better to do at this time.";
                }
                else
                {
                    redirectionText = "Not bad. I don't have an idea of anything better to do at this time.";
                }
                redirectionColor = Color.Green;
            }
            else
            {
                string improvementText = improvementInDays.ToString();
                if (this.Suggested)
                {
                    redirectionText = "I thought of a better idea: " + betterActivity.Name + ", better by +" + improvementText + " days fun. Sorry for not mentioning this earlier!";
                    redirectionColor = Color.Yellow;
                }
                else
                {
                    if (ExpectedFutureFun >= 0)
                    {
                        redirectionText = "I suggest that " + betterActivity.Name + " would be even better: +" + improvementText + " days fun.";
                        redirectionColor = Color.Yellow;
                    }
                    else
                    {
                        redirectionText = "I suggest that " + betterActivity.Name + " would improve your future happiness by " + improvementText + " days.";
                        redirectionColor = Color.Red;
                    }
                }
            }
            detailsGrid.AddLayout(new TextblockLayout(redirectionText, redirectionColor));


            return detailsGrid;
        }

        static TextblockLayout signedColoredValue(double value, double neutralColorThreshold, double stddev)
        {
            string text;
            if (value > 0)
                text = "+" + value;
            else
                text = "" + value;
            if (stddev != 0)
                text += " +/- (" + stddev + ")";
            Color textColor = chooseColor(value, 0, neutralColorThreshold);
            TextblockLayout layout = new TextblockLayout(text, textColor);
            return layout;
        }

        static TextblockLayout coloredRatio(double value, double neutralColorThreshold, double stddev)
        {
            Color color = chooseColor(value, 1, neutralColorThreshold);
            string text = "" + value;
            if (stddev != 0)
                text += " +/- (" + stddev + ")";
            TextblockLayout layout = new TextblockLayout(text, color);
            return layout;

        }

        static Color chooseColor(double value, double cutoffA, double cutoffB)
        {
            if (cutoffA > cutoffB)
            {
                double temp = cutoffA;
                cutoffA = cutoffB;
                cutoffB = temp;
            }
            if (value >= cutoffA)
            {
                if (value >= cutoffB)
                {
                    if (value == cutoffA)
                        return Color.White;
                    else
                        return Color.Green;
                }
                else
                {
                    return Color.Yellow;
                }
            }
            else
            {
                return Color.Red;
            }
        }


        public DateTime StartDate;
        public DateTime EndDate;
        public double ParticipationDurationDividedByAverage;

        public double PredictedValue;
        public double PredictedCurrentValueStddev;

        public double ExpectedFutureFun;
        public double ExpectedFutureFunStddev;

        public double ExpectedEfficiency;
        public double ExpectedEfficiencyStddev;

        public DateTime ComparisonDate;
        public double ComparisonPredictedValue;
        public double ComparisonExpectedFutureFun;
        public double ComparisonExpectedEfficiency;

        public bool Suggested;

        public Activity ChosenActivity;

        public Engine engine;
        public ActivityDatabase ActivityDatabase;

        private LayoutChoice_Set layout;
    }
}
