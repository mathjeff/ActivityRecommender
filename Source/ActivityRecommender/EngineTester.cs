using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the EngineTester makes an Engine and calculates its average squared error
// The first time (on 2012-1-15) that I calculated the Root(Mean(Squared(Error))), it was about 0.327

/*
The latest results (on 2012-2-17) using a bunch of independent, combined predictions is:
typicalScoreError = 0.147785971030127
typicalProbabilityError = 0.470028763507115

The latest results (on 2012-2-18) using a multidimensional interpolator (which is slower) are:
typicalScoreError = 0.113297119720235
typicalProbabilityError = 0.470853382639463

latest results (on 2012-2-20) using a slightly faster version are:
typicalScoreError = 0.118867951358166
typicalProbabilityError = 0.472384299236938

latest results (on 2012-2-25) using a slightly faster version (where the Interpolator skips any splits that will be overwritten by later splits) are:
typicalScoreError = 0.118790161274347
typicalProbabilityError = 0.470607845933648     // this is the error rate we'd expect if each participation probability was 0.669 (or 0.331)

latest results (on 2012-4-10) simply after having acquired additional data (due to the passage of time)
typicalScoreError = 0.131244338310556
typicalProbabilityError = 0.456090786674865
 
latest results (on 2012-4-10) after switching back to getValueExponentially in the ParticipationProgression
typicalScoreError = 0.130215012360051
typicalProbabilityError = 0.456090786674865     // this is the error rate we'd expect if each participation probability was 0.705 (or 0.295)
// I think that my increased usage of the RelativeRating is increasing the information content of my ratings, and decreasing the prediction accuracy
// I think the increased data is improving the accuracy of the participation probability


 
 */

namespace ActivityRecommendation
{
    class EngineTester
    {
        public EngineTester()
        {
            this.engine = new Engine();
            this.activityDatabase = this.engine.ActivityDatabase;
            this.squaredScoreError = new Distribution();
            this.squaredParticipationProbabilityError = new Distribution();
        }

        public void AddInheritance(Inheritance newInheritance)
        {
            this.engine.PutInheritanceInMemory(newInheritance);
        }
        public void AddRequest(ActivityRequest newRequest)
        {
            //Rating rating = newRequest.GetCompleteRating();
            //if (rating != null)
            //    this.AddRating(rating);
            this.engine.PutActivityRequestInMemory(newRequest);
        }
        public void AddSkip(ActivitySkip newSkip)
        {
            // update the error rate for the participation probability predictor
            this.UpdateParticipationProbabilityError(newSkip.ActivityDescriptor, newSkip.Date, 0);
            this.engine.PutSkipInMemory(newSkip);

            //Rating rating = newSkip.GetCompleteRating();
            //if (rating != null)
            //    this.AddRating(rating);
        }
        public void AddParticipation(Participation newParticipation)
        {
            // update the error rate for the participation probability predictor
            this.UpdateParticipationProbabilityError(newParticipation.ActivityDescriptor, newParticipation.StartDate, newParticipation.TotalIntensity.Mean);

            Rating rating = newParticipation.GetCompleteRating();
            if (rating != null)
                this.AddRating(rating);

            this.engine.PutParticipationInMemory(newParticipation);
        }
        public void AddRating(Rating newRating)
        {
            if (newRating is RelativeRating)
                this.AddRating((RelativeRating)newRating);
            if (newRating is AbsoluteRating)
                this.AddRating((AbsoluteRating)newRating);
            //this.engine.PutRatingInMemory(newRating);

            //Activity activity = this.activityDatabase.ResolveDescriptor(newRating.
        }
        public void AddRating(AbsoluteRating newRating)
        {
            this.PrintResults();
            Console.WriteLine("Adding rating with date" + ((DateTime)newRating.Date).ToString());
            this.UpdateScoreError(newRating.ActivityDescriptor, (DateTime)newRating.Date, newRating.Score);
            this.engine.PutRatingInMemory(newRating);
        }
        public void AddRating(RelativeRating newRating)
        {

            AbsoluteRating betterRating = newRating.BetterRating;
            this.engine.MakeRecommendation((DateTime)betterRating.Date);
            Activity betterActivity = this.activityDatabase.ResolveDescriptor(betterRating.ActivityDescriptor);
            Distribution predictedBetterScore = betterActivity.PredictedScore.Distribution;

            AbsoluteRating worseRating = newRating.WorseRating;
            this.engine.MakeRecommendation((DateTime)worseRating.Date);
            Activity worseActivity = this.activityDatabase.ResolveDescriptor(betterRating.ActivityDescriptor);
            Distribution predictedWorseScore = worseActivity.PredictedScore.Distribution;

            if (newRating.RawScoreScale == null)
            {
                // if the relative rating simply said "activity X is better than Y", then we compute a penalty if we computed something different
                double error;
                if (predictedBetterScore.Mean > predictedWorseScore.Mean)
                    error = 0;
                else
                    error = predictedWorseScore.Mean - predictedBetterScore.Mean;
                this.UpdateScoreError(error);
            }
            else
            {
                // if the relative rating said "Activity X is better than Y by a factor of z", then we can compute the discrepancy
                double scale = (double)newRating.BetterScoreScale;
                Distribution rescaledBetterDistribution = predictedBetterScore.CopyAndReweightTo(1);
                Distribution rescaledWorseDistribution = predictedWorseScore.CopyAndReweightTo(1);
                Distribution total = rescaledBetterDistribution.Plus(rescaledWorseDistribution);
                double mean = total.Mean;
                double improvedWorseEstimate = mean * 2 / (scale + 1);
                double improvedBetterEstimate = improvedWorseEstimate * scale;

                double err2 = improvedBetterEstimate - predictedBetterScore.Mean;
                double err1 = improvedWorseEstimate - predictedWorseScore.Mean;
                double error = Math.Sqrt(err1 * err1 + err2 * err2);
                this.UpdateScoreError(error);
            }

            this.engine.PutRatingInMemory(newRating);
        }
        // runs the engine on the given activity at the given date, and keeps track of the overall error
        public void UpdateScoreError(ActivityDescriptor descriptor, DateTime when, double correctScore)
        {
            // update everything
            this.engine.MakeRecommendation(when);

            // compute the estimated score
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            /*
            if (activity == null)
            {
                activity = this.activityDatabase.ResolveDescriptor(descriptor);
            }
            else
            {
                // update only what we need
                this.engine.EstimateRating(activity, when);
            }
            */
            // compute error
            this.UpdateScoreError(activity.PredictedScore.Distribution.Mean - correctScore);
        }
        public void UpdateScoreError(double error)
        {
            Distribution errorDistribution = Distribution.MakeDistribution(error * error, 0, 1);
            this.squaredScoreError = this.squaredScoreError.Plus(errorDistribution);
        }
        public void UpdateParticipationProbabilityError(ActivityDescriptor descriptor, DateTime when, double actualIntensity)
        {
            // update everything
            this.engine.MakeRecommendation(when);
            // compute the estimate participation probability
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            double error = activity.PredictedParticipationProbability.Distribution.Mean - actualIntensity;
            Distribution errorDistribution = Distribution.MakeDistribution(error * error, 0, 1);
            this.squaredParticipationProbabilityError = this.squaredParticipationProbabilityError.Plus(errorDistribution);
        }

        public void PrintResults()
        {
            double typicalScoreError = Math.Sqrt(this.squaredScoreError.Mean);
            Console.WriteLine("typicalScoreError = " + typicalScoreError.ToString());
            double typicalProbabilityError = Math.Sqrt(this.squaredParticipationProbabilityError.Mean);
            Console.WriteLine("typicalProbabilityError = " + typicalProbabilityError.ToString());
        }
        public Distribution SquaredScoreError
        {
            get
            {
                return this.squaredScoreError;
            }
        }
        public Distribution SquaredParticipationProbabilityError
        {
            get
            {
                return this.squaredParticipationProbabilityError;
            }
        }

        private Engine engine;
        private ActivityDatabase activityDatabase;
        private Distribution squaredScoreError;
        private Distribution squaredParticipationProbabilityError;
    }
}
