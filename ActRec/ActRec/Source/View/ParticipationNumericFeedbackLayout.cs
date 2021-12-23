using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class ParticipationNumericFeedbackLayout : ContainerLayout
    {
        public ParticipationNumericFeedbackLayout(ParticipationNumericFeedback feedback, UserSettings userSettings, LayoutStack layoutStack)
        {
            this.userSettings = userSettings;
            this.numericFeedback = feedback;
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();
            if (feedback.SuggestedBadIdea)
            {
                string apology;
                if (feedback.PredictedValue > 1)
                {
                    // A fun idea that we didn't expect the user to do
                    // We expected the user to get distracted while considering the fun idea
                    apology = "Sorry, I didn't expect you to actually do this! I thought that this suggestion would help you think of a better idea!";
                }
                else
                {
                    // An annoying idea that we didn't expect the user to do
                    // We expected the user to remember how annoying this was and to avoid it and things like it
                    apology = "Sorry, I didn't expect you to actually do this! I thought that this suggestion would remind you to look harder for something better!";
                }
                builder.AddLayout(new TextblockLayout(apology));
            }

            builder.AddLayout(new TextblockLayout(feedback.ChosenActivity.Name));
            builder.AddLayout(new TextblockLayout("From " + feedback.StartDate + " to " + feedback.EndDate + ", " + feedback.ParticipationDurationDividedByAverage + " as long as average. I predict:"));

            GridLayout_Builder nowBuilder = new Horizontal_GridLayout_Builder().Uniform();

            nowBuilder.AddLayout(
                new Vertical_GridLayout_Builder().Uniform()
                    .AddLayout(
                        new HelpButtonLayout("Fun (vs average):",
                            new TextblockLayout("This column shows the amount of happiness you are expected to have while doing this activity at this time, divided by the average amount of happiness you usually have doing other things"),
                            layoutStack)
                        )
                    .AddLayout(coloredRatio(feedback.PredictedValue, feedback.ComparisonPredictedValue, feedback.PredictedCurrentValueStddev))
                    .Build()
            );
            nowBuilder.AddLayout(
                new Vertical_GridLayout_Builder().Uniform()
                    .AddLayout(
                        new HelpButtonLayout("Future Fun (days):",
                            new TextblockLayout("This column shows an estimate of the net present value of your happiness at this time after doing this activity, compared to what it usually is. " +
                                "This is very similar to computing how many days of happiness you will gain or lose over the next " + Math.Round(CommonPreferences.Instance.HalfLife.TotalDays / Math.Log(2), 0) +
                                " days after doing feedback."),
                            layoutStack)
                        )
                    .AddLayout(signedColoredValue(feedback.ExpectedFutureFun, feedback.ComparisonExpectedFutureFun, feedback.ExpectedFutureFunStddev))
                    .Build()
            );
            nowBuilder.AddLayout(
                new Vertical_GridLayout_Builder().Uniform()
                    .AddLayout(
                        new HelpButtonLayout("Future Efficiency (hours):",
                            new TextblockLayout("This column shows an estimate of the net present value of your efficiency at this time after doing this activity, compared to what it usually is. " +
                                "This is very similar to computing how many hours of efficiency you will gain or lose over the next " + Math.Round(CommonPreferences.Instance.EfficiencyHalflife.TotalDays / Math.Log(2), 0) +
                                " days after doing feedback."),
                            layoutStack)
                        )
                    .AddLayout(signedColoredValue(feedback.ExpectedEfficiency, feedback.ComparisonExpectedEfficiency, feedback.ExpectedEfficiencyStddev))
                    .Build()
            );
            builder.AddLayout(nowBuilder.Build());

            builder.AddLayout(new TextblockLayout("If you had done this at " + feedback.ComparisonDate + ":"));

            GridLayout_Builder laterBuilder = new Horizontal_GridLayout_Builder().Uniform();
            laterBuilder.AddLayout(coloredRatio(feedback.ComparisonPredictedValue, 1, 0));
            laterBuilder.AddLayout(signedColoredValue(feedback.ComparisonExpectedFutureFun, 1, 0));
            laterBuilder.AddLayout(signedColoredValue(feedback.ComparisonExpectedEfficiency, 0, 0));
            builder.AddLayout(laterBuilder.Build());

            ActivityRequest request = new ActivityRequest();
            request.ActivityToBeat = feedback.ChosenActivity.MakeDescriptor();
            request.Date = feedback.StartDate;
            // Look for something better the user could be doing
            ActivitiesSuggestion suggestion = feedback.engine.MakeRecommendation(request);
            Activity betterActivity = feedback.ActivityDatabase.ResolveDescriptor(suggestion.ActivityDescriptors[0]);
            Prediction betterPrediction = feedback.engine.Get_OverallHappiness_ParticipationEstimate(betterActivity, request);
            string redirectionText;
            Color redirectionColor;
            Distribution betterFutureHappinessImprovementInDays = feedback.engine.compute_longtermValue_increase_in_days(betterPrediction.Distribution, feedback.StartDate, feedback.StartDate);
            double improvementInDays = Math.Round(betterFutureHappinessImprovementInDays.Mean - feedback.ExpectedFutureFun, 1);
            if (improvementInDays <= 0)
            {
                string noIdeasText = "I don't have any better suggestions for things to do at this time.";
                if (feedback.ExpectedFutureFun >= 0)
                {
                    if (feedback.PredictedValue >= 1)
                        redirectionText = "Nice! " + noIdeasText; // Happy now, happy later
                    else
                        redirectionText = noIdeasText + " Sorry!"; // Happy later, not happy now
                }
                else
                {
                    if (feedback.PredictedValue >= 1)
                        redirectionText = noIdeasText; // Happy now, not happy later
                    else
                        redirectionText = "How about adding a new activity? " + noIdeasText; // Not happy now or later
                }
                redirectionColor = Color.Green;
            }
            else
            {
                string improvementText = improvementInDays.ToString();
                if (feedback.Suggested)
                {
                    redirectionText = "I thought of a better idea: " + betterActivity.Name + ", better by +" + improvementText + " days fun. Sorry for not mentioning this earlier!";
                    redirectionColor = Color.Yellow;
                }
                else
                {
                    if (feedback.ExpectedFutureFun >= 0)
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

            ParticipationFeedbackTypeSelector selector = new ParticipationFeedbackTypeSelector(userSettings);
            this.feedbackSummary = new TextblockLayout().AlignHorizontally(TextAlignment.Center).AlignVertically(TextAlignment.Center);
            userSettings.Changed += UserSettings_Changed;
            this.updateSummary();
            builder.AddLayout(new Horizontal_GridLayout_Builder().AddLayout(selector).AddLayout(this.feedbackSummary).BuildAnyLayout());

            this.SubLayout = builder.BuildAnyLayout();
        }

        private void UserSettings_Changed()
        {
            this.updateSummary();
        }

        private void updateSummary()
        {
            ParticipationFeedbackFormatter formatter = new ParticipationFeedbackFormatter("", this.numericFeedback);
            this.feedbackSummary.setText(formatter.Get(this.userSettings.FeedbackType));
        }

        TextblockLayout signedColoredValue(double value, double neutralColorThreshold, double stddev)
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

        TextblockLayout coloredRatio(double value, double neutralColorThreshold, double stddev)
        {
            Color color = chooseColor(value, 1, neutralColorThreshold);
            string text = "" + value;
            if (stddev != 0)
                text += " +/- " + stddev;
            TextblockLayout layout = new TextblockLayout(text, color);
            return layout;

        }

        Color chooseColor(double value, double cutoffA, double cutoffB)
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

        private TextblockLayout feedbackSummary;
        private ParticipationNumericFeedback numericFeedback;
        private UserSettings userSettings;

    }
}
