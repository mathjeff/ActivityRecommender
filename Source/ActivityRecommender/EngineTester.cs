using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the EngineTester makes an Engine and calculates its average squared error
// The first time (on 2012-1-15) that I calculated the Root(Mean(Squared(Error))), it was about 0.327

/*
The latest data (on 2012-1-25) is:
typicalScoreError = 0.209970298665407
typicalProbabilityError = 0.471008001759027
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
            this.engine.PutParticipationInMemory(newParticipation);

            Rating rating = newParticipation.GetCompleteRating();
            if (rating != null)
                this.AddRating(rating);

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
            this.UpdateScoreError(newRating.ActivityDescriptor, (DateTime)newRating.Date, newRating.Score);
            this.engine.PutRatingInMemory(newRating);
        }
        public void AddRating(RelativeRating newRating)
        {
            AbsoluteRating betterRating = newRating.BetterRating;
            this.UpdateScoreError(betterRating.ActivityDescriptor, (DateTime)betterRating.Date, betterRating.Score);

            AbsoluteRating worseRating = newRating.WorseRating;
            this.UpdateScoreError(worseRating.ActivityDescriptor, (DateTime)worseRating.Date, worseRating.Score);

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
            double error = activity.PredictedScore.Distribution.Mean - correctScore;
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
