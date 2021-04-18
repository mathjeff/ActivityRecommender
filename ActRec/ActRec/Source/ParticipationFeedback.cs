using ActivityRecommendation.TextSummary;
using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    public class ParticipationFeedback
    {
        public ParticipationFeedback(Activity activity, string summary, bool? happySummary, ParticipationNumericFeedback numericDetails)
        {
            this.Activity = activity;
            this.Summary = summary;
            this.happySummary = happySummary;
            this.numericDetails = numericDetails;
        }
        public ParticipationFeedback(Activity activity, string summary, bool? happySummary, LayoutChoice_Set details)
        {
            this.Activity = activity;
            this.Summary = summary;
            this.happySummary = happySummary;
            this.details = details;
        }

        public Activity Activity { get; set; }

        public string Summary { get; set; }
        public bool? happySummary { get; set; }
        public LayoutChoice_Set GetDetails(LayoutStack layoutStack)
        {
            if (this.details == null)
            {
                this.details = this.numericDetails.MakeLayout(layoutStack);
            }
            return this.details;
        }

        LayoutChoice_Set details;
        ParticipationNumericFeedback numericDetails;
    }

    public class ParticipationNumericFeedback
    {
        public ParticipationNumericFeedback()
        {
        }

        public LayoutChoice_Set MakeLayout(LayoutStack layoutStack)
        {
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();
            builder.AddLayout(new TextblockLayout(ChosenActivity.Name));
            builder.AddLayout(new TextblockLayout("From " + this.StartDate + " to " + this.EndDate + ", " + ParticipationDurationDividedByAverage + " as long as average. I predict:"));

            GridLayout_Builder nowBuilder = new Horizontal_GridLayout_Builder().Uniform();

            nowBuilder.AddLayout(
                new Vertical_GridLayout_Builder().Uniform()
                    .AddLayout(
                        new HelpButtonLayout("Fun (vs average):",
                            new TextblockLayout("This column shows the amount of happiness you are expected to have while doing this activity at this time, divided by the average amount of happiness you usually have doing other things"),
                            layoutStack)
                        )
                    .AddLayout(coloredRatio(PredictedValue, ComparisonPredictedValue, PredictedCurrentValueStddev))
                    .Build()
            );
            nowBuilder.AddLayout(
                new Vertical_GridLayout_Builder().Uniform()
                    .AddLayout(
                        new HelpButtonLayout("Future Fun (days):",
                            new TextblockLayout("This column shows an estimate of the net present value of your happiness at this time after doing this activity, compared to what it usually is. " +
                                "This is very similar to computing how many days of happiness you will gain or lose over the next " + Math.Round(UserPreferences.DefaultPreferences.HalfLife.TotalDays / Math.Log(2), 0) +
                                " days after doing this."),
                            layoutStack)
                        )
                    .AddLayout(signedColoredValue(ExpectedFutureFun, ComparisonExpectedFutureFun, ExpectedFutureFunStddev))
                    .Build()
            );
            nowBuilder.AddLayout(
                new Vertical_GridLayout_Builder().Uniform()
                    .AddLayout(
                        new HelpButtonLayout("Future Efficiency (hours):",
                            new TextblockLayout("This column shows an estimate of the net present value of your efficiency at this time after doing this activity, compared to what it usually is. " +
                                "This is very similar to computing how many hours of efficiency you will gain or lose over the next " + Math.Round(UserPreferences.DefaultPreferences.EfficiencyHalflife.TotalDays / Math.Log(2), 0) +
                                " days after doing this."),
                            layoutStack)
                        )
                    .AddLayout(signedColoredValue(ExpectedEfficiency, ComparisonExpectedEfficiency, ExpectedEfficiencyStddev))
                    .Build()
            );
            builder.AddLayout(nowBuilder.Build());

            builder.AddLayout(new TextblockLayout("If you had done this at " + this.ComparisonDate + ":"));

            GridLayout_Builder laterBuilder = new Horizontal_GridLayout_Builder().Uniform();
            laterBuilder.AddLayout(coloredRatio(ComparisonPredictedValue, 1, 0));
            laterBuilder.AddLayout(signedColoredValue(ComparisonExpectedFutureFun, 1, 0));
            laterBuilder.AddLayout(signedColoredValue(ComparisonExpectedEfficiency, 0, 0));
            builder.AddLayout(laterBuilder.Build());

            ActivityRequest request = new ActivityRequest();
            request.ActivityToBeat = this.ChosenActivity.MakeDescriptor();
            request.Date = this.StartDate;
            request.RequestedProcessingTime = TimeSpan.FromSeconds(0.5);
            ActivitySuggestion suggestion = this.engine.MakeRecommendation(request);
            Activity betterActivity = this.ActivityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor);
            Prediction betterPrediction = this.engine.Get_OverallHappiness_ParticipationEstimate(betterActivity, request);
            string redirectionText;
            Color redirectionColor;
            Distribution betterFutureHappinessImprovementInDays = this.engine.compute_longtermValue_increase_in_days(betterPrediction.Distribution);
            double improvementInDays = Math.Round(betterFutureHappinessImprovementInDays.Mean - this.ExpectedFutureFun, 1);
            if (improvementInDays <= 0)
            {
                int numOthersConsidered = suggestion.NumActivitiesConsidered - 1;
                string othersConsidered = "I considered " + numOthersConsidered + " other idea";
                if (numOthersConsidered != 1)
                    othersConsidered += "s";
                if (ExpectedFutureFun >= 0)
                    redirectionText = "Nice! " + othersConsidered + " and don't have any better suggestions for things to do at this time.";
                else
                    redirectionText = "Not bad. " + numOthersConsidered + " and don't have any better suggestions for things to do at this time.";
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
            builder.AddLayout(new TextblockLayout(redirectionText, redirectionColor));


            return builder.BuildAnyLayout();
        }

        static TextblockLayout signedColoredValue(double value, double neutralColorThreshold, double stddev)
        {
            string text;
            if (value > 0)
                text = "+" + value;
            else
                text = "" + value;
            if (stddev != 0)
                text += " +/- " + stddev;
            Color textColor = chooseColor(value, 0, neutralColorThreshold);
            TextblockLayout layout = new TextblockLayout(text, textColor);
            return layout;
        }

        static TextblockLayout coloredRatio(double value, double neutralColorThreshold, double stddev)
        {
            Color color = chooseColor(value, 1, neutralColorThreshold);
            string text = "" + value;
            if (stddev != 0)
                text += " +/- " + stddev;
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
