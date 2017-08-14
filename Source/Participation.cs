﻿using System;
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
            this.Suggested = null;
            this.Hypothetical = false;
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ActivityDescriptor ActivityDescriptor { get; set; }          // a description of what the user actually did
        public Consideration Consideration { get; set; }            // The user's thoughts that contributed to the fact that they did this activity
        public bool? Suggested { get; set; }     // tells whether the latest suggestion that the engine made was to do this activity

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
        public string Comment { get; set; }
        public bool Hypothetical { get; set; }  // false if it actually happened, true if we are supposing that it might happen

        public Distribution LogIdleTime { get; set; }   // the log of the time (in seconds) between sub-participations
        public Distribution LogActiveTime { get; set; } // the log of the duration (in seconds) of each sub-participation

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
        // Uses the given rawRating as the new RawRating except removes some redundant data from it first
        public void PutAndCompressRating(RelativeRating rawRating)
        {
            RelativeRating dehydrated = new RelativeRating();
            dehydrated.CopyFrom(rawRating);
            // check whether the existing rating specifies any duplicate data
            if (rawRating.BetterRating.ActivityDescriptor != null && rawRating.WorseRating.ActivityDescriptor != null)
            {
                // rating specifies data that is redundant with the participation, so find and remove the duplicate data
                bool betterMatchesThis = false;
                if (this.ActivityDescriptor.CanMatch(rawRating.BetterRating.ActivityDescriptor))
                {
                    if (rawRating.BetterRating.Date.Equals(this.StartDate))
                        betterMatchesThis = true;
                }

                bool worseMatchesThis = false;
                if (this.ActivityDescriptor.CanMatch(rawRating.WorseRating.ActivityDescriptor))
                {
                    if (rawRating.WorseRating.Date.Equals(this.StartDate))
                        worseMatchesThis = true;
                }
                if (betterMatchesThis && worseMatchesThis)
                    throw new Exception("Could not determine which AbsoluteRating in " + rawRating + " describes " + this + "(both match)");
                if ((!betterMatchesThis) && (!worseMatchesThis))
                    throw new Exception("Could not determine which AbsoluteRating in " + rawRating + " describes " + this + "(neither match)");

                AbsoluteRating ratingToDehydrate = null;
                if (betterMatchesThis)
                    ratingToDehydrate = dehydrated.BetterRating;
                else
                    ratingToDehydrate = dehydrated.WorseRating;
                // clear any fields that are implied due to being attached to this object
                ratingToDehydrate.Date = null;
                ratingToDehydrate.ActivityDescriptor = null;
            }
            // save the simplified rating
            this.rawRating = dehydrated;
        }
        // returns an AbsoluteRating that contains as much information as possible
        public AbsoluteRating GetAbsoluteRating()
        {
            Rating fullRating = this.GetCompleteRating();
            if (fullRating == null)
                return null;
            AbsoluteRating converted = new AbsoluteRating();
            converted.FillInFromParticipation(this);
            converted.Score = fullRating.GetScoreForDescriptor(this.ActivityDescriptor);
            return converted;
        }

        private Distribution totalIntensity;
        private Rating rawRating;

        #endregion

    }
}
