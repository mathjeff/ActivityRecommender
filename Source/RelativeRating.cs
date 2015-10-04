﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the RelativeRating class represents the statement that the rating of one Activity at one time is better than the rating of another Activity at another time
namespace ActivityRecommendation
{
    public class RelativeRating : Rating
    {
        #region Public Member Functions

        public RelativeRating()
        {
            this.BetterRating = new AbsoluteRating();
            this.WorseRating = new AbsoluteRating();
        }
        public override Rating MakeCopy()
        {
            RelativeRating copy = new RelativeRating();
            copy.CopyFrom(this);
            return copy;
        }

        public void CopyFrom(RelativeRating original)
        {
            base.CopyFrom(original);
            this.RawScoreScale = original.RawScoreScale;
            this.BetterRating.CopyFrom(original.BetterRating);
            this.WorseRating.CopyFrom(original.WorseRating);
        }


        public AbsoluteRating BetterRating { get; set; }
        public AbsoluteRating WorseRating { get; set; }

        public bool BetterRatingIsFirst
        {
            get
            {
                return (this.BetterRating.Date.Value.CompareTo(this.WorseRating.Date.Value) < 0);
            }
        }
        public AbsoluteRating FirstRating
        {
            get
            {
                if (this.BetterRatingIsFirst)
                    return this.BetterRating;
                else
                    return this.WorseRating;
            }
        }
        public AbsoluteRating SecondRating
        {
            get
            {
                if (this.BetterRatingIsFirst)
                    return this.WorseRating;
                else
                    return this.BetterRating;
            }
        }
        public double? RawScoreScale  // the value the user provided for (the score of the better activity divided by the score of the worse activity)
        {
            get;
            set;
        }
        public double? BetterScoreScale
        {
            get
            {
                if (this.RawScoreScale == null)
                    return null;
                if (this.RawScoreScale < 1)
                    return 1 / this.RawScoreScale;
                return this.RawScoreScale;
            }
        }
        // The participation itself has one Rating, and another Activity has the other rating
        // Now we figure out which Rating gets assigned to this Participation and give it as much data as possible
        public override void FillInFromParticipation(Participation participation)
        {
            // figure out whether the better activity or the worse activity is the one that generated this rating
            if (this.WorseRating.IsComplete() && !this.BetterRating.IsComplete())
            {
                // if we get here, the better participation generated this RelativeRating
                this.BetterRating.FillInFromParticipation(participation);
            }
            if (this.BetterRating.IsComplete() && !this.WorseRating.IsComplete())
            {
                // if we get here, the worse participation generated this RelativeRating
                this.WorseRating.FillInFromParticipation(participation);
            }
            base.FillInFromParticipation(participation);
        }

        public override double GetScoreForDescriptor(ActivityDescriptor descriptor)
        {
            if (descriptor == this.BetterRating.ActivityDescriptor)
                return this.BetterRating.Score;
            if (descriptor == this.WorseRating.ActivityDescriptor)
                return this.WorseRating.Score;
            throw new ArgumentException("cannot ask for the score for an activity not known to the RelativeRating");
        }

        public override void AttemptToMatch(Participation participation)
        {
            if (this.BetterRating.IsComplete() && !this.WorseRating.IsComplete())
            {
                this.BetterRating.AttemptToMatch(participation);
            }
            if (this.WorseRating.IsComplete() && !this.BetterRating.IsComplete())
            {
                this.WorseRating.AttemptToMatch(participation);
            }
        }

        #endregion

        #region Private Member Variables


        #endregion

    }
}