using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    abstract class RatingReplayer
    {
        public RatingReplayer()
        {
            this.engine = new Engine();
            this.activityDatabase = this.engine.ActivityDatabase;
        }

        public void AddInheritance(Inheritance newInheritance)
        {
            this.engine.PutInheritanceInMemory(newInheritance);
        }
        public void AddRequest(ActivityRequest newRequest)
        {
            this.PreviewRequest(newRequest);
            this.engine.PutActivityRequestInMemory(newRequest);
        }
        public virtual void PreviewRequest(ActivityRequest request) { }

        public void AddSkip(ActivitySkip newSkip)
        {
            this.PreviewSkip(newSkip);
            this.engine.PutSkipInMemory(newSkip);
        }
        public virtual void PreviewSkip(ActivitySkip newSkip) { }

        public void AddParticipation(Participation newParticipation)
        {
            this.PreviewParticipation(newParticipation);
            Rating rating = newParticipation.GetCompleteRating();
            if (rating != null)
            {
               RelativeRating newRating = this.AddRating(rating) as RelativeRating;
               newParticipation.PutAndCompressRating(newRating);
            }

            this.engine.PutParticipationInMemory(newParticipation);
            this.PostParticipation(newParticipation);

        }
        public virtual void PreviewParticipation(Participation newParticipation) { }
        public virtual void PostParticipation(Participation newParticipation) { }

        public Rating AddRating(Rating newRating)
        {
            if (newRating is RelativeRating)
                return this.ProcessRating((RelativeRating)newRating);
            if (newRating is AbsoluteRating)
                return this.ProcessRating((AbsoluteRating)newRating);
            return null;
        }
        public virtual AbsoluteRating ProcessRating(AbsoluteRating newRating) { return newRating; }
        public virtual RelativeRating ProcessRating(RelativeRating newRating) 
        {
            this.ProcessRating(newRating.FirstRating);
            this.ProcessRating(newRating.SecondRating);
            return newRating;
        }
        public RelativeRating AddRating(RelativeRating newRating)
        {
            this.ProcessRating(newRating.FirstRating);
            this.ProcessRating(newRating.SecondRating);
            return this.ProcessRating(newRating);
        }
        public void AddSuggestion(ActivitySuggestion suggestion)
        {
            this.PreviewSuggestion(suggestion);
            this.engine.PutSuggestionInMemory(suggestion);
        }
        public virtual void PreviewSuggestion(ActivitySuggestion suggestion) { }
        public virtual void Finish() { }


        protected Engine engine;
        protected ActivityDatabase activityDatabase;

    }
}
