using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    // A ParticipationComment is a comment that refers to a participation
    public class ParticipationComment
    {
        public ParticipationComment(string text, DateTime applicableDate, DateTime createdDate, ActivityDescriptor activity)
        {
            this.text = text;
            this.applicableDate = applicableDate;
            this.createdDate = createdDate;
            this.activityDescriptor = activity;
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }

        public DateTime ApplicableDate
        {
            get
            {
                return this.applicableDate;
            }
        }
        public DateTime CreatedDate
        {
            get
            {
                return this.createdDate;
            }
        }

        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                return this.activityDescriptor;
            }
        }

        private string text;
        private DateTime applicableDate;
        private DateTime createdDate;
        private ActivityDescriptor activityDescriptor;
    }
}
