using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The Participation class represents an instance of a user performing an Activity
namespace ActivityRecommendation
{
    public class Participation
    {

        #region Public Member functions
        public Participation()
        {
            this.Initialize(new DateTime(0), new DateTime(0), null, 0);
        }
        public Participation(DateTime startDate, DateTime endDate, ActivityDescriptor activityDescriptor, double averageIntensity)
        {
            this.Initialize(startDate, endDate, activityDescriptor, averageIntensity);
        }
        public Participation(DateTime startDate, DateTime endDate, ActivityDescriptor activityDescriptor)
        {
            this.Initialize(startDate, endDate, activityDescriptor, 1);
        }
        private void Initialize(DateTime startDate, DateTime endDate, ActivityDescriptor activityDescriptor, double averageIntensity)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.ActivityDescriptor = activityDescriptor;
            double totalIntensity = this.Duration.TotalSeconds * averageIntensity;
            this.totalIntensity = new Distribution(totalIntensity, totalIntensity * averageIntensity, totalIntensity);
            this.RawRating = null;
            this.LogIdleTime = new Distribution(0, 0, 0);
            double numSeconds = this.Duration.TotalSeconds;
            if (numSeconds > 0)
                this.LogActiveTime = Distribution.MakeDistribution(Math.Log(numSeconds), 0, 1);
            else
                this.LogActiveTime = new Distribution();
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ActivityDescriptor ActivityDescriptor { get; set; }

        public Distribution TotalIntensity // intensity measured in seconds
        {
            get
            {
                return this.totalIntensity;
            }
            set
            {
                this.totalIntensity = value;
            }
        }
        public TimeSpan Duration
        {
            get
            {
                return this.EndDate.Subtract(this.StartDate);
            }
        }
        public Distribution LogIdleTime { get; set; }   // the log of the time (in seconds) between sub-participations
        public Distribution LogActiveTime { get; set; } // the log of the duration (in seconds) of each sub-participation
        public string Comment { get; set; }

        // returns the exact rating that was given to this Participation
        public Rating RawRating 
        {
            get
            {
                return this.rawRating;
            }
            set
            {
                this.rawRating = value;
            }
        }
        // returns a Rating with as much information filled in as possible based on the data in this participation
        public Rating GetCompleteRating()
        {
            if (this.RawRating == null)
                return null;
            Rating completeRating = this.rawRating.MakeCopy();
            completeRating.FillInFromParticipation(this);
            return completeRating;
        }

        private Distribution totalIntensity;
        private Rating rawRating;

        #endregion

    }
}
