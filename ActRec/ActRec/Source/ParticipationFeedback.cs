using ActivityRecommendation.TextSummary;
using ActivityRecommendation.View;
using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Microsoft.Maui.Controls;

namespace ActivityRecommendation
{
    public class ParticipationFeedback
    {
        public ParticipationFeedback(Activity activity, ValueConverter<ParticipationFeedbackType, string> summary, bool? happySummary, ParticipationNumericFeedback numericDetails)
        {
            this.Activity = activity;
            this.Summary = summary;
            this.happySummary = happySummary;
            this.numericDetails = numericDetails;
        }
        public ParticipationFeedback(Activity activity, string summary, bool? happySummary, LayoutChoice_Set details)
        {
            this.Activity = activity;
            this.Summary = new ConstantValueConverter<ParticipationFeedbackType, string>(summary);
            this.happySummary = happySummary;
            this.details = details;
        }

        public Activity Activity { get; set; }

        public string getSummary(UserSettings userSettings)
        {
            return this.Summary.Get(userSettings.FeedbackType);
        }
        public ValueConverter<ParticipationFeedbackType, string> Summary { get; set; }
        public bool? happySummary { get; set; }
        public LayoutChoice_Set GetDetails(LayoutStack layoutStack, UserSettings userSettings)
        {
            if (this.details == null)
            {
                this.details = this.numericDetails.MakeLayout(layoutStack, userSettings);
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

        public LayoutChoice_Set MakeLayout(LayoutStack layoutStack, UserSettings userSettings)
        {
            return new ParticipationNumericFeedbackLayout(this, userSettings, layoutStack);
        }

        // Sometimes the Engine will make a suggestion and not expect the user to take that suggestion, and we want to apologize if the user actually takes that suggestion.
        // SuggestedBadIdea indicates whether we suggested a bad idea that we didn't expect the user to do
        public bool SuggestedBadIdea
        {
            get
            {
                return this.Suggested && this.ExpectedFutureFun < 0;
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
