using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the AbsoluteRating class embodies the statement that the user would (or did) get a certain amount of value out of doing a specific activity on a certain date
namespace ActivityRecommendation
{
    public class AbsoluteRating : Rating
    {
        #region Public Member Functions

        public AbsoluteRating(double score, DateTime date, ActivityDescriptor activityDescriptor, RatingSource source)
        {
            this.Score = score;
            this.Date = date;
            this.ActivityDescriptor = activityDescriptor;
            this.Source = source;
            this.Initialize();
        }

        public AbsoluteRating(AbsoluteRating original)
        {
            this.Initialize();
            this.CopyFrom(original);
        }

        public AbsoluteRating()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this.Weight = 1;
        }

        public override Rating MakeCopy()
        {
            AbsoluteRating copy = new AbsoluteRating();
            copy.CopyFrom(this);
            return copy;
        }
        public void CopyFrom(AbsoluteRating original)
        {
            this.Score = original.Score;
            this.Date = original.Date;
            this.ActivityDescriptor = original.ActivityDescriptor;
            base.CopyFrom(original);
        }

        public override void FillInFromParticipation(Participation participation)
        {
            this.ActivityDescriptor = participation.ActivityDescriptor;
            this.Date = participation.StartDate;
            base.FillInFromParticipation(participation);
        }

        public override void FillInFromSkip(ActivitySkip skip)
        {
            this.ActivityDescriptor = skip.ActivityDescriptor;
            this.Date = skip.Date;
            base.FillInFromSkip(skip);
        }

        public override void FillInFromRequest(ActivityRequest request)
        {
            this.ActivityDescriptor = request.ActivityDescriptor;
            this.Date = request.Date;
            this.Score = 1;
            base.FillInFromRequest(request);
        }

        public ActivityDescriptor ActivityDescriptor { get; set; }
        public double Score { get; set; }
        public DateTime? Date { get; set; }
        public double Weight { get; set; }
        // returns true if this Rating has all the necessary data; returns false if this rating is missing some important information
        public bool IsComplete()
        {
            if (this.ActivityDescriptor == null)
                return false;
            if (this.Date == null)
                return false;
            return true;
        }

        #endregion

        /*
        public void SetActivityDescriptor(ActivityDescriptor descriptor)
        {
            this.activityDescriptor = descriptor;
        }
        public ActivityDescriptor GetActivityDescriptor()
        {
            return this.activityDescriptor;
        }
        public void SetScore(double score)
        {
            this.value = score;
        }
        public double GetScore()
        {
            return this.value;
        }
        public void SetDate(DateTime when)
        {
            this.date = when;
        }
        public DateTime GetDate()
        {
            return this.date;
        }
        public void SetRatingSource(RatingSource source)
        {
            this.ratingSource = source;
        }
        public RatingSource GetRatingSource()
        {
            return this.ratingSource;
        }

        #endregion

        #region Private Member Variables

        private ActivityDescriptor activityDescriptor;
        private double value;
        private DateTime date;
        private RatingSource ratingSource;

        #endregion
        */
    }
}
