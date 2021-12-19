using ActivityRecommendation.Effectiveness;
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
            this.Initialize(new DateTime(0), new DateTime(0), null);
        }
        public Participation(DateTime startDate, DateTime endDate, ActivityDescriptor activityDescriptor)
        {
            this.Initialize(startDate, endDate, activityDescriptor);
        }
        private void Initialize(DateTime startDate, DateTime endDate, ActivityDescriptor activityDescriptor)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.ActivityDescriptor = activityDescriptor;
            this.Hypothetical = false;
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ActivityDescriptor ActivityDescriptor { get; set; }          // a description of what the user actually did
        public Consideration Consideration { get; set; }            // The user's thoughts that contributed to the fact that they did this activity
        public bool Suggested { get; set; }     // tells whether the latest suggestion that the engine made was to do this activity
        public CompletionEfficiencyMeasurement EffectivenessMeasurement { get; set; }
        public RelativeEfficiencyMeasurement RelativeEfficiencyMeasurement
        {
            get
            {
                if (this.EffectivenessMeasurement == null)
                    return null;
                return this.EffectivenessMeasurement.Computation;
            }
        }
        public void setRelativeEfficiencyMeasurement(RelativeEfficiencyMeasurement measurement, Metric metric)
        {
            if (this.EffectivenessMeasurement == null)
            {
                bool successful = measurement.RecomputedEfficiency.Mean > 0;
                double helpFraction = 0;
                if (successful)
                    helpFraction = 1 - measurement.RecomputedEfficiency.Mean;
                
                this.EffectivenessMeasurement = new CompletionEfficiencyMeasurement(metric, successful, helpFraction);
            }
            this.EffectivenessMeasurement.Computation = measurement;
        }
        // Returns true if this Participation was assigned a completion metric that considers this Participation to have succeeded
        public bool CompletedMetric
        {
            get
            {
                if (this.EffectivenessMeasurement == null)
                    return false;
                return this.EffectivenessMeasurement.Successful;
            }
        }
        public double CompletionFraction
        {
            get
            {
                if (!this.CompletedMetric)
                    return 0;
                return 1 - this.EffectivenessMeasurement.HelpFraction;
            }
        }
        public double HelpFraction
        {
            get
            {
                if (this.EffectivenessMeasurement == null)
                    return 0;
                return this.EffectivenessMeasurement.HelpFraction;
            }
        }
        // Returns true if this Participation was assigned a completion metric that considers its Activity to no longer be doable
        public bool DismissedActivity
        {
            get
            {
                if (this.EffectivenessMeasurement == null)
                    return false;
                return this.EffectivenessMeasurement.DismissedActivity;
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

        
        // returns the exact rating that was given to this Participation
        public Rating Rating 
        {
            get
            {
                return this.rating;
            }
            set
            {
                Rating rating = value;
                if (rating != null)
                    rating.AttemptToMatch(this);
                this.rating = rating;
            }
        }
        public void AddPostComment(ParticipationComment comment)
        {
            if (this.postComments == null)
            {
                this.postComments = new List<ParticipationComment>();
            }
            this.postComments.Add(comment);
        }

        // returns the rating except removes some redundant data from it first
        public Rating CompressedRating
        {
            get
            {
                RelativeRating relative = this.rating as RelativeRating;
                if (relative == null)
                    return this.rating;

                RelativeRating dehydrated = new RelativeRating();
                dehydrated.CopyFrom(relative);
                // check whether the existing rating specifies any duplicate data
                if (relative.BetterRating.ActivityDescriptor != null && relative.WorseRating.ActivityDescriptor != null)
                {
                    // rating specifies data that is redundant with the participation, so find and remove the duplicate data
                    bool betterMatchesThis = false;
                    if (this.ActivityDescriptor.CanMatch(relative.BetterRating.ActivityDescriptor))
                    {
                        if (relative.BetterRating.Date.Equals(this.StartDate))
                            betterMatchesThis = true;
                    }

                    bool worseMatchesThis = false;
                    if (this.ActivityDescriptor.CanMatch(relative.WorseRating.ActivityDescriptor))
                    {
                        if (relative.WorseRating.Date.Equals(this.StartDate))
                            worseMatchesThis = true;
                    }
                    if (betterMatchesThis && worseMatchesThis)
                        throw new Exception("Could not determine which AbsoluteRating in " + relative + " describes " + this + "(both match)");
                    if ((!betterMatchesThis) && (!worseMatchesThis))
                        throw new Exception("Could not determine which AbsoluteRating in " + relative + " describes " + this + "(neither match)");

                    AbsoluteRating ratingToDehydrate = null;
                    if (betterMatchesThis)
                        ratingToDehydrate = dehydrated.BetterRating;
                    else
                        ratingToDehydrate = dehydrated.WorseRating;
                    // clear any fields that are implied due to being attached to this object
                    ratingToDehydrate.Date = null;
                    ratingToDehydrate.ActivityDescriptor = null;
                }

                return dehydrated;
            }
        }
        // Returns an AbsoluteRating describing how the user rated this participation
        public AbsoluteRating GetRecordedRatingAsAbsolute()
        {
            Rating fullRating = this.rating as RelativeRating;
            if (fullRating == null)
                return null;
            AbsoluteRating converted = new AbsoluteRating();
            converted.AttemptToMatch(this);
            converted.Score = fullRating.GetScoreForDescriptor(this.ActivityDescriptor);
            return converted;
        }

        // Returns an AbsoluteRating describing our best guess about how good this participation was
        public AbsoluteRating GetEstimatedRating()
        {
            AbsoluteRating estimated = this.Rating as AbsoluteRating;
            if (estimated != null)
                return estimated;

            AbsoluteRating assigned = this.GetRecordedRatingAsAbsolute();
            return assigned;
        }

        private Rating rating;

        public List<ParticipationComment> PostComments
        {
            get
            {
                if (this.postComments == null)
                {
                    return new List<ParticipationComment>();
                }
                return this.postComments;
            }
        }
        private List<ParticipationComment> postComments;
        #endregion
    }

    public class ParticipationScoreComparer : IComparer<Participation>
    {
        public int Compare(Participation a, Participation b)
        {
            AbsoluteRating ratingA = a.GetRecordedRatingAsAbsolute();
            AbsoluteRating ratingB = b.GetRecordedRatingAsAbsolute();
            double scoreA;
            double scoreB;
            if (ratingA != null)
                scoreA = ratingA.Score;
            else
                scoreA = double.NegativeInfinity;
            if (ratingB != null)
                scoreB = ratingB.Score;
            else
                scoreB = double.NegativeInfinity;
            return scoreA.CompareTo(scoreB);
        }
    }

    public class ParticipationEfficiencyComparer : IComparer<Participation>
    {
        public int Compare(Participation a, Participation b)
        {
            return this.getEfficiency(a).CompareTo(this.getEfficiency(b));
        }

        private double getEfficiency(Participation participation)
        {
            if (participation.RelativeEfficiencyMeasurement != null)
                return participation.RelativeEfficiencyMeasurement.RecomputedEfficiency.Mean;
            return 1; // no data, treat as default
        }
    }

    public class Participation_NetPresentHappiness_Comparer : IComparer<Participation>
    {
        public Participation_NetPresentHappiness_Comparer(ScoreSummarizer scoreSummarizer)
        {
            this.scoreSummarizer = scoreSummarizer;
        }
        public int Compare(Participation a, Participation b)
        {
            return this.get_netPresentHappiness(a).CompareTo(this.get_netPresentHappiness(b));
        }

        private double get_netPresentHappiness(Participation participation)
        {
            return this.scoreSummarizer.GetValueDistributionForDates(participation.StartDate, this.scoreSummarizer.LatestKnownDate, true, true).Mean;
        }
        private ScoreSummarizer scoreSummarizer;
    }
}
