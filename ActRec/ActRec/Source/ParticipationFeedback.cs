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
            BoundProperty_List rowHeights = new BoundProperty_List(8);
            // row 0 is a summary
            rowHeights.BindIndices(1, 3); // 1, 3, and 5 are titles
            rowHeights.BindIndices(1, 5);
            rowHeights.BindIndices(2, 4); // 2, 4, and 6 are details
            rowHeights.BindIndices(2, 6);
            // row 7 is a different activity
            GridLayout detailsGrid = GridLayout.New(rowHeights, new BoundProperty_List(1), LayoutScore.Zero); ;
            detailsGrid.AddLayout(new TextblockLayout(ChosenActivity.Name + " " +
                TimeFormatter.summarizeTimespan(this.StartDate, this.EndDate) + ": " +
                "You spent " + ParticipationDurationDividedByAverage + " as long as average. I predict:"
                ));

            detailsGrid.AddLayout(new TextblockLayout("Current fun:"));
            GridLayout funWhileDoingIt_layout = GridLayout.New(BoundProperty_List.Uniform(3), new BoundProperty_List(2), LayoutScore.Zero);
            funWhileDoingIt_layout.AddLayout(coloredRatio(PredictedCurrentValueForThisActivity, PredictedAverageValueForThisActivity));
            funWhileDoingIt_layout.AddLayout(new TextblockLayout("* avg fun while doing it at this time"));
            funWhileDoingIt_layout.AddLayout(new TextblockLayout("" + PredictedCurrentValueStddev));
            funWhileDoingIt_layout.AddLayout(new TextblockLayout("stddev"));
            funWhileDoingIt_layout.AddLayout(coloredRatio(PredictedAverageValueForThisActivity, 1));
            funWhileDoingIt_layout.AddLayout(new TextblockLayout("overall average for this activity"));
            detailsGrid.AddLayout(funWhileDoingIt_layout);

            detailsGrid.AddLayout(new TextblockLayout("Future fun:"));
            GridLayout futureFun_layout = GridLayout.New(BoundProperty_List.Uniform(3), new BoundProperty_List(2), LayoutScore.Zero);
            futureFun_layout.AddLayout(signedColoredValue(ExpectedFutureFunAfterDoingThisActivityNow, ExpectedFutureFunAfterDoingThisActivitySometime));
            futureFun_layout.AddLayout(new TextblockLayout("days future fun (over next " + Math.Round(UserPreferences.DefaultPreferences.HalfLife.TotalDays / Math.Log(2), 0) + " days) from doing this now"));
            futureFun_layout.AddLayout(new TextblockLayout("" + ExpectedFutureFunStddev));
            futureFun_layout.AddLayout(new TextblockLayout("stddev (days)"));
            futureFun_layout.AddLayout(signedColoredValue(ExpectedFutureFunAfterDoingThisActivitySometime, 0));
            futureFun_layout.AddLayout(new TextblockLayout("overall average (days) after having done this activity"));
            detailsGrid.AddLayout(futureFun_layout);

            detailsGrid.AddLayout(new TextblockLayout("Future efficiency:"));
            GridLayout futureEfficiency_layout = GridLayout.New(BoundProperty_List.Uniform(3), new BoundProperty_List(2), LayoutScore.Zero);
            futureEfficiency_layout.AddLayout(signedColoredValue(ExpectedEfficiencyAfterDoingThisActivityNow, ExpectedEfficiencyAfterDoingThisActivitySometime));
            futureEfficiency_layout.AddLayout(new TextblockLayout("hours future efficiency (over next " + Math.Round(UserPreferences.DefaultPreferences.EfficiencyHalflife.TotalDays / Math.Log(2), 0) + " days) from doing this now"));
            futureEfficiency_layout.AddLayout(new TextblockLayout("" + ExpectedEfficiencyStddev));
            futureEfficiency_layout.AddLayout(new TextblockLayout("stddev (hours)"));
            futureEfficiency_layout.AddLayout(signedColoredValue(ExpectedEfficiencyAfterDoingThisActivitySometime, 0));
            futureEfficiency_layout.AddLayout(new TextblockLayout("overall average (hours) after having done this activity"));
            detailsGrid.AddLayout(futureEfficiency_layout);

            ActivityRequest request = new ActivityRequest();
            request.ActivityToBeat = this.ChosenActivity.MakeDescriptor();
            request.Date = this.StartDate;
            request.RequestedProcessingTime = TimeSpan.FromSeconds(0.5);
            ActivitySuggestion suggestion = this.engine.MakeRecommendation(request);
            Activity betterActivity = this.ActivityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor);
            Prediction betterPrediction = this.engine.Get_OverallHappiness_ParticipationEstimate(betterActivity, this.StartDate);
            Prediction current = this.engine.Get_OverallHappiness_ParticipationEstimate(this.ChosenActivity, this.StartDate);
            string redirectionText;
            Color redirectionColor;
            Distribution betterFutureHappinessImprovementInDays = this.engine.compute_longtermValue_increase_in_days(betterPrediction.Distribution);
            double improvementInDays = Math.Round(betterFutureHappinessImprovementInDays.Mean - this.ExpectedFutureFunAfterDoingThisActivityNow, 1);
            if (improvementInDays <= 0)
            {
                if (ExpectedFutureFunAfterDoingThisActivityNow >= 0)
                {
                    redirectionText = "Nice! I don't have any ideas of something better to do at this time.";
                }
                else
                {
                    redirectionText = "Not bad. I don't have any ideas of something better to do at this time.";
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
                    if (ExpectedFutureFunAfterDoingThisActivityNow >= 0)
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

        static TextblockLayout signedColoredValue(double value, double neutralColorThreshold)
        {
            string text;
            if (value > 0)
                text = "+" + value;
            else
                text = "" + value;
            Color textColor = chooseColor(value, 0, neutralColorThreshold);
            TextblockLayout layout = new TextblockLayout(text, textColor);
            return layout;
        }

        static TextblockLayout coloredRatio(double value, double neutralColorThreshold)
        {
            Color color = chooseColor(value, 1, neutralColorThreshold);
            string text = "" + value;
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

        public double PredictedCurrentValueForThisActivity;
        public double PredictedCurrentValueStddev;
        public double PredictedAverageValueForThisActivity;

        public double ExpectedFutureFunAfterDoingThisActivityNow;
        public double ExpectedFutureFunStddev;
        public double ExpectedFutureFunAfterDoingThisActivitySometime;

        public double ExpectedEfficiencyAfterDoingThisActivityNow;
        public double ExpectedEfficiencyStddev;
        public double ExpectedEfficiencyAfterDoingThisActivitySometime;

        public bool Suggested;

        public Activity ChosenActivity;

        public Engine engine;
        public ActivityDatabase ActivityDatabase;

        private LayoutChoice_Set layout;
    }
}
