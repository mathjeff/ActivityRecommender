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
            this.FromUser = true;
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
            this.FromUser = original.FromUser;
            base.CopyFrom(original);
        }

        public override void FillInFromRequest(ActivityRequest request)
        {
            this.ActivityDescriptor = request.FromCategory;
            this.Date = request.Date;
            this.Score = 1;
            base.FillInFromRequest(request);
        }

        public ActivityDescriptor ActivityDescriptor { get; set; }
        public double Score { get; set; }
        public DateTime? Date { get; set; }
        public double Weight { get; set; }
        public bool FromUser { get; set; } // If true, the user entered this rating. If false, this was inferred by the engine

        public override double GetScoreForDescriptor(ActivityDescriptor descriptor)
        {
            if (descriptor == this.ActivityDescriptor)
                return this.Score;
            throw new ArgumentException("cannot ask an absolute rating for the score of a different activity");
        }

        public bool CanMatch(Participation participation)
        {
            if (participation == null)
                return false;
            if (this.ActivityDescriptor != null)
            {
                if (!this.ActivityDescriptor.CanMatch(participation.ActivityDescriptor))
                    return false;
            }
            if (this.Date != null)
            {
                if (!this.Date.Value.Equals(participation.StartDate))
                    return false;
            }
            return true;
        }
        public override void AttemptToMatch(Participation participation)
        {
            // check whether this rating might describe this participation
            if (!this.CanMatch(participation))
                return;
            // This rating seems to describe this participation, so fill in the corresponding data
            this.ActivityDescriptor = participation.ActivityDescriptor;
            this.Date = participation.StartDate;
            base.AttemptToMatch(participation);
        }
        #endregion

    }
}
