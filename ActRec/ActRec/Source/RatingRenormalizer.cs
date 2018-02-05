using System;
using System.IO;

// The RatingRenormalizer will scan the user's history of ratings and recompute the Rating for each Participation.
//  It will recompute the scores for each RelativeRating, and will also generate an absoluteRating if no rating was given
// It uses the current version of the Engine to do the computations
// Most importantly, this means that it ignores any user-entered AbsoluteRating in the rating calculations

// TODOS:
// IS string concatenation slow?
// Have to make the Participation serialization include FromUser
// Should make the Participation serialization include the newlines
// Should double-check that the data gets saved successfully
namespace ActivityRecommendation
{
    class RatingRenormalizer : HistoryWriter
    {
        public RatingRenormalizer(TextConverter textConverter) : base(textConverter)
        {
        }
        public override void PreviewParticipation(Participation newParticipation)
        {
            if (newParticipation.RawRating == null)
            {
                newParticipation.RawRating = this.engine.MakeEstimatedRating(newParticipation);
            }
        }
        public override RelativeRating ProcessRating(RelativeRating newRating)
        {
            System.Diagnostics.Debug.WriteLine("Adding rating with date " + ((DateTime)newRating.FirstRating.Date).ToString());

            AbsoluteRating otherRating = newRating.FirstRating;
            AbsoluteRating thisRating = newRating.SecondRating;
            if (newRating.Source != null)
            {
                Participation participation = newRating.Source.ConvertedAsParticipation;
                RelativeRating rawRating = participation.RawRating as RelativeRating;
                if (rawRating.BetterRating.Date == null && rawRating.WorseRating.Date != null)
                {
                    // This one is BetterRating
                    thisRating = newRating.BetterRating;
                    otherRating = newRating.WorseRating;
                }
                else
                {
                    // This one is WorseRating
                    thisRating = newRating.WorseRating;
                    otherRating = newRating.BetterRating;
                }
            }

            if (newRating.RawScoreScale == null)
            {
                // The relative rating simply said "activity X is better than Y"
                // We might as well assign the same scale that the previous algorithm had computed
                double scale = thisRating.Score / otherRating.Score;
                newRating.RawScoreScale = scale;
            }

            Participation otherParticipation = null;
            if (otherRating.Source != null)
                otherParticipation = otherRating.Source.ConvertedAsParticipation;
            Participation thisParticipation = null;
            if (thisRating.Source != null) 
                thisParticipation = thisRating.Source.ConvertedAsParticipation;
            if (otherParticipation != null && thisParticipation != null)
            {
                RelativeRating modifiedRating = this.engine.MakeRelativeRating(thisParticipation, newRating.RawScoreScale.Value, otherParticipation);
                if (modifiedRating.BetterRating.ActivityDescriptor != null)
                {
                    if (!modifiedRating.BetterRating.ActivityDescriptor.CanMatch(newRating.BetterRating.ActivityDescriptor))
                    {
                        throw new Exception("Internal error: RatingRenormalizer swapped ratings while rewriting them");
                    }
                }
                if (modifiedRating.WorseRating.ActivityDescriptor != null)
                {
                    if (!modifiedRating.WorseRating.ActivityDescriptor.CanMatch(newRating.WorseRating.ActivityDescriptor))
                    {
                        throw new Exception("Internal error: RatingRenormalizer swapped ratings while rewriting them");
                    }
                }
                if (modifiedRating.BetterRating.ActivityDescriptor == null && modifiedRating.WorseRating.ActivityDescriptor == null)
                {
                    throw new Exception("Internal error: RatingRenormalizer created rating that does not specify an activity");
                }
                return modifiedRating;
            }
            return newRating;
        }
        public override Engine Finish()
        {
            base.Finish();
            return this.engine;
        }
    }

}