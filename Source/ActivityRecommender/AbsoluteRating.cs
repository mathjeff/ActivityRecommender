using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the AbsoluteRating class embodies the statement that the user would (or did) get a certain amount of value out of doing a specific activity on a certain date
namespace ActivityRecommendation
{
    public class AbsoluteRating
    {
        #region Public Member Functions
        public AbsoluteRating(double startingScore, DateTime startingDate, ActivityDescriptor startingDescriptor, RatingSource startingSource)
        {
            this.Score = startingScore;
            this.Date = startingDate;
            this.ActivityDescriptor = startingDescriptor;
            this.RatingSource = startingSource;
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
        public void CopyFrom(AbsoluteRating original)
        {
            this.Score = original.Score;
            this.Date = original.Date;
            this.ActivityDescriptor = original.ActivityDescriptor;
            this.RatingSource = original.RatingSource;
        }

        public ActivityDescriptor ActivityDescriptor { get; set; }
        public double Score { get; set; }
        public DateTime Date { get; set; }
        public double Weight { get; set; }
        public RatingSource RatingSource { get; set; }

        #endregion

        private void Initialize()
        {
            this.Weight = 1;
        }
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
