﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    public abstract class Rating
    {
        public abstract Rating MakeCopy();

        public void CopyFrom(Rating original)
        {
            this.Source = original.Source;
        }
        public RatingSource Source { get; set; }

        // adds any additional data based on the fact that this rating was generated by the given Participation
        public virtual void FillInFromParticipation(Participation participation)
        {
            this.Source = RatingSource.FromParticipation(participation);
        }
        // adds any additional data based on the fact that this rating was generated by the given Skip
        public virtual void FillInFromSkip(ActivitySkip skip)
        {
            this.Source = RatingSource.FromSkip(skip);
        }
        // adds any additional data based on the fact that this rating was generated by the given ActivityRequest
        public virtual void FillInFromRequest(ActivityRequest request)
        {
            this.Source = RatingSource.FromRequest(request);
        }
        // returns the score that this Rating has for the activity with the given descriptor
        public abstract double GetScoreForDescriptor(ActivityDescriptor descriptor);
    }
}
