using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    // a Persona describes longterm preferences that the user can specify, like what ActivityRecommender looks like
    public class UserSettings
    {
        public delegate void SettingsChanged();
        public event SettingsChanged Changed;

        // The name that ActivityRecommender uses to refer to itself
        public string PersonaName
        {
            get
            {
                return this.personaName;
            }
            set
            {
                if (value != this.personaName)
                {
                    this.personaName = value;
                    if (this.Changed != null)
                        this.Changed.Invoke();
                }
            }
        }
        public string LayoutDefaults_Name
        {
            get
            {
                return this.layoutDefaults_name;
            }
            set
            {
                this.layoutDefaults_name = value;
            }
        }

        public ParticipationFeedbackType FeedbackType
        {
            get
            {
                return this.feedbackType;
            }
            set
            {
                if (value != this.feedbackType)
                {
                    this.feedbackType = value;
                    if (this.Changed != null)
                        this.Changed.Invoke();
                }
            }
        }

        private string personaName = "ActivityRecommender";
        private string layoutDefaults_name;
        private ParticipationFeedbackType feedbackType = ParticipationFeedbackType.LONGTERM_HAPPINESS;
    }
}
