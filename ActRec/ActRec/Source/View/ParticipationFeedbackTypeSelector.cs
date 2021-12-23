using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    public class ParticipationFeedbackTypeSelector : ContainerLayout
    {
        public ParticipationFeedbackTypeSelector(UserSettings userSettings)
        {
            this.userSettings = userSettings;
            List<string> feedbackTypeLabels = new List<string>();
            int selectIndex = 0;
            foreach (ParticipationFeedbackType feedbackType in this.feedbackTypes)
            {
                if (userSettings.FeedbackType == feedbackType)
                    selectIndex = feedbackTypeLabels.Count;
                feedbackTypeLabels.Add(this.get_feedbackType_displayText(feedbackType));
            }
            SingleSelect singleSelect = new SingleSelect("Change summary:", feedbackTypeLabels);
            singleSelect.Updated += SingleSelect_Updated;
            singleSelect.SelectedIndex = selectIndex;
            this.SubLayout = singleSelect;
        }

        private void SingleSelect_Updated(SingleSelect singleSelect)
        {
            string text = singleSelect.SelectedItem;
            ParticipationFeedbackType feedbackType = this.getSelectedType(text);
            this.userSettings.FeedbackType = feedbackType;
        }

        private string get_feedbackType_displayText(ParticipationFeedbackType feedbackType)
        {
            if (feedbackType == ParticipationFeedbackType.LONGTERM_HAPPINESS)
                return "Future fun";
            if (feedbackType == ParticipationFeedbackType.SHORTTERM_HAPPINESS)
                return "Fun (vs average)";
            if (feedbackType == ParticipationFeedbackType.LONGTERM_EFFICIENCY)
                return "Future Efficiency (hours)";
            if (feedbackType == ParticipationFeedbackType.DURATION_VS_AVERAGE)
                return "Duration";
            throw new ArgumentException("Internal error: unrecognized participation feedback type " + feedbackType);
        }

        private ParticipationFeedbackType getSelectedType(string text)
        {
            foreach (ParticipationFeedbackType feedbackType in this.feedbackTypes)
            {
                if (this.get_feedbackType_displayText(feedbackType) == text)
                    return feedbackType;
            }
            throw new ArgumentOutOfRangeException("Unrecognized participation feedback type '" + text + "'");
        }

        private List<ParticipationFeedbackType> feedbackTypes
        {
            get
            {
                return new List<ParticipationFeedbackType>() {
                    ParticipationFeedbackType.SHORTTERM_HAPPINESS,
                    ParticipationFeedbackType.LONGTERM_HAPPINESS,
                    ParticipationFeedbackType.LONGTERM_EFFICIENCY,
                    ParticipationFeedbackType.DURATION_VS_AVERAGE
                };
            }
        }

        private UserSettings userSettings;
    }
}
