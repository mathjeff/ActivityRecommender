﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the RelativeRating class represents the statement that the rating of one Activity at one time is better than the rating of another Activity at another time
namespace ActivityRecommendation
{
    class RelativeRating : Rating
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
            this.BetterRating.CopyFrom(original.BetterRating);
            this.WorseRating.CopyFrom(original.WorseRating);
        }


        public AbsoluteRating BetterRating { get; set; }
        public AbsoluteRating WorseRating { get; set; }

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
        #endregion

        #region Private Member Variables


        #endregion

    }
}