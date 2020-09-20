using System;
using System.IO;
using ActivityRecommendation.Effectiveness;

// A few things computed by the Engine over time are too expensive to always recompute
//  So, some computations are saved into the data files
//  The RatingRenormalizer redoes all of these computations and as a result is rather slow.
//   Specifically:
//    The RatingRenormalizer recomputes absolute ratings from relative ratings
//    The RatingRenormalizer recalculates experiment predicted difficulties
//    The RatingRenormalizer recalculates experiment efficiencies
namespace ActivityRecommendation
{
    class RatingRenormalizer : HistoryWriter
    {
        public RatingRenormalizer(bool recomputeRatings, bool recomputeEfficiencies)
        {
            this.recomputeRatings = recomputeRatings;
            this.recomputeEfficiencies = recomputeEfficiencies;
        }
        public override void PreviewParticipation(Participation newParticipation)
        {
            if (this.recomputeRatings)
            {
                if (newParticipation.RawRating == null)
                    newParticipation.RawRating = this.engine.MakeEstimatedRating(newParticipation);
            }
            if (this.recomputeEfficiencies)
            {
                if (newParticipation.RelativeEfficiencyMeasurement != null)
                {
                    newParticipation.setRelativeEfficiencyMeasurement(this.engine.Make_CompletionEfficiencyMeasurement(newParticipation), newParticipation.EffectivenessMeasurement.Metric);
                }
            }
        }
        public override RelativeRating ProcessRating(RelativeRating newRating)
        {
            if (!this.recomputeRatings)
                return newRating;
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
        public override void PreviewExperiment(PlannedExperiment experiment)
        {
            this.engine.ReplanExperiment(experiment);
            base.PreviewExperiment(experiment);
        }
        public override Engine Finish()
        {
            base.Finish();
            return this.engine;
        }
        private bool recomputeRatings;
        private bool recomputeEfficiencies;
    }

}