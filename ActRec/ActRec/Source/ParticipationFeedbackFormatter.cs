using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation
{
    class ParticipationFeedbackFormatter : ValueConverter<ParticipationFeedbackType, string>
    {
        public ParticipationFeedbackFormatter(string remark, ParticipationNumericFeedback feedback)
        {
            this.remark = remark;
            this.feedback = feedback;
        }
        public string Get(ParticipationFeedbackType feedbackType)
        {
            return this.formatNumber(this.feedback, feedbackType) + this.remark;
        }
        private string formatNumber(ParticipationNumericFeedback feedback, ParticipationFeedbackType formatType)
        {
            switch (formatType)
            {
                case ParticipationFeedbackType.LONGTERM_HAPPINESS:
                    return this.numberWithExclamationPoints(Math.Round(feedback.ExpectedFutureFun, 1));
                case ParticipationFeedbackType.SHORTTERM_HAPPINESS:
                    return "" + feedback.PredictedValue + " ";
                case ParticipationFeedbackType.LONGTERM_EFFICIENCY:
                    return this.numberWithExclamationPoints(Math.Round(feedback.ExpectedEfficiency, 0));
                case ParticipationFeedbackType.DURATION_VS_AVERAGE:
                    return "x" + Math.Round(feedback.ParticipationDurationDividedByAverage, 2) + " ";
            }
            throw new ArgumentException("Unrecognized feedback type " + formatType);

        }

        // adds a sign and exclamation points based on the number, and returns the result
        private string numberWithExclamationPoints(double value)
        {
            int numExclamationPoints = (int)Math.Min((Math.Abs(value) / 10), 10);
            string exclamationPoints = ":";
            if (numExclamationPoints > 0)
            {
                exclamationPoints = "";
                for (int i = 0; i < numExclamationPoints; i++)
                {
                    exclamationPoints += "!";
                }
            }

            string result;
            if (value > 0)
            {
                result = "+" + value + exclamationPoints + " ";
            }
            else
            {
                if (value < 0)
                    result = "" + value + exclamationPoints + " ";
                else
                    result = "+0";
            }
            return result;
        }

        string remark;
        ParticipationNumericFeedback feedback;
    }
}
