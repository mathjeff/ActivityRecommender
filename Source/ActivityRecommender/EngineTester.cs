using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the EngineTester makes an Engine and calculates its average squared error
// The first time (on 2012-1-15) that I calculated the Root(Mean(Squared(Error))), it was about 0.327

// The latest Root(Mean(Squared(Error))) on 2012-1-15 is: 0.323552683405183 (including the ratings caused by Skips)
namespace ActivityRecommendation
{
    class EngineTester
    {
        public EngineTester()
        {
            this.engine = new Engine();
            this.activityDatabase = this.engine.ActivityDatabase;
            this.squaredError = new Distribution();
        }

        public void AddInheritance(Inheritance newInheritance)
        {
            this.engine.PutInheritanceInMemory(newInheritance);
        }
        public void AddRequest(ActivityRequest newRequest)
        {
            Rating rating = newRequest.GetCompleteRating();
            if (rating != null)
                this.AddRating(rating);
            this.engine.PutActivityRequestInMemory(newRequest);
        }
        public void AddSkip(ActivitySkip newSkip)
        {
            Rating rating = newSkip.GetCompleteRating();
            if (rating != null)
                this.AddRating(rating);
        }
        public void AddParticipation(Participation newParticipation)
        {
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
            this.UpdateError(newRating.ActivityDescriptor, (DateTime)newRating.Date, newRating.Score);
            this.engine.PutRatingInMemory(newRating);
        }
        public void AddRating(RelativeRating newRating)
        {
            AbsoluteRating betterRating = newRating.BetterRating;
            this.UpdateError(betterRating.ActivityDescriptor, (DateTime)betterRating.Date, betterRating.Score);

            AbsoluteRating worseRating = newRating.WorseRating;
            this.UpdateError(worseRating.ActivityDescriptor, (DateTime)worseRating.Date, worseRating.Score);

            this.engine.PutRatingInMemory(newRating);
        }
        // runs the engine on the given activity at the given date, and keeps track of the overall error
        public void UpdateError(ActivityDescriptor descriptor, DateTime when, double correctScore)
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
            this.squaredError = this.squaredError.Plus(errorDistribution);
        }

        public void PrintResults()
        {
            double typicalError = Math.Sqrt(this.squaredError.Mean);
            Console.WriteLine("typicalError = " + typicalError.ToString());
        }
        public Distribution SquaredError
        {
            get
            {
                return this.squaredError;
            }
        }

        private Engine engine;
        private ActivityDatabase activityDatabase;
        private Distribution squaredError;
    }
}
