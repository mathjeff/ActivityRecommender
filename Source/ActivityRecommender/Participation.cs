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
        private Distribution totalIntensity;
        /*public TimeSpan TotalIntensity { get; set; }  // number of seconds
        public double AverageIntensity 
        {
            get
            {
                double numerator = this.TotalIntensity.TotalSeconds;
                double denominator = this.Duration.TotalSeconds;
                if (denominator != 0)
                {
                    return numerator / denominator;
                }
                else
                {
                    return numerator;
                }
            }
        }
        public TimeSpan Duration
        {
            get
            {
                return this.EndDate.Subtract(this.StartDate);
            }
        }*/

        #endregion

    }
}
