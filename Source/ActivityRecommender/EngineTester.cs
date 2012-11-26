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


latest results (on 2012-4-11) after removing from the averages and participations that were known to not have been suggested
typicalScoreError = 0.129996871657446
typicalProbabilityError = 0.466402441220659

latest results (on 2012-4-13) after acquiring new data
typicalScoreError = 0.129628174126225
typicalProbabilityError = 0.466490032356424

latest results (on 2012-4-13) after adjusting the AdaptiveLinearInterpolator's input-splitting terminating condition to involve counting the number of points again,
 rather than looking at the stdDev of the inputs
typicalScoreError = 0.132018880366107
typicalProbabilityError = 0.448369070940053

latest results (on 2012-6-16) after acquiring new data 
typicalScoreError = 0.132725069636051
typicalProbabilityError = 0.462996444030975

latest results (on 2012-6-16) after changing the interpolator's input-splitting-termination-criteria to be based on the number of points, not the size of the inputs
The problem was that if coordinate was a constant, then the product of the input variations was zero
typicalScoreError = 0.135236469558133
typicalProbabilityError = 0.44306302626509

 
latest results (on 2012-8-30) after acquiring new data 
typicalScoreError = 0.140364754582535
typicalProbabilityError = 0.443507071557773

latest results (on 2012-8-30) after adjusting the interpolator to do a more sensible job when given data outside the promised input range
typicalScoreError = 0.140700447417066
typicalProbabilityError = 0.442070071914349

 
latest results (on 2012-8-31) after telling the interpolator to update more often
typicalScoreError = 0.140585869049799
typicalProbabilityError = 0.439053966981607 
equivalentProbability = 0.739231298282047

latest results (on 2012-10-7) after acquiring more data:
typicalScoreError = 0.147389266231023
typicalProbabilityError = 0.440218252424753
equivalentProbability = 0.737082032706184

latest results (on 2012-10-7) after using some of the unprompted participations as fake, prompted participations
typicalScoreError = 0.147260241883526
typicalProbabilityError = 0.430046711300231
equivalentProbability = 0.755068277329534

latest results (on 2012-10-21) after updating the engine to predict the (exponentially weighted) future ratings of all activities, but without
  having updated the EngineTester to adjust the target values accordingly:
typicalScoreError = 0.150655861416498
typicalProbabilityError = 0.431639712860378
equivalentProbability = 0.752363147630177
 

latest results (on 2012-11-17) after acquiring new data and then updating the engine to distinguish between expected immediate rating and expected overall future 
 rating (and computing error based on expected immediate rating)
typicalScoreError = 0.147185861338085
equivalentProbability = 0.744923948566977

latest results (on 2012-11-17) after 1. Adjusting the engine back to compute the difference between the predicted score and the updated score that is based on the
 relative rating that generated it, and 2. Having the engine compute the error of the long-term prediction
typical longtermPrediction error = 0.273375212068489
typicalScoreError = 0.163789122061962
equivalentProbability = 0.744923948566977


results (on 2012-11-17) if I have the EngineTester skip any predictions having weight of 0
typical longtermPrediction error = 0.0621895084186268
typicalScoreError = 0.16571109025582
typicalProbabilityError = 0.435903956644535
equivalentProbability = 0.744923948566977

 
results (on 2012-11-17) after having re-added the small amount of extra error to all predictions (which mattered for predictions having weight 0)
typical longtermPrediction error = 0.105942853885603
typicalScoreError = 0.16571109025582
typicalProbabilityError = 0.435903956644535
equivalentProbability = 0.744923948566977
 
 */

namespace ActivityRecommendation
{
    class EngineTester
    {
        public EngineTester()
        {
            this.engine = new Engine();
            this.activityDatabase = this.engine.ActivityDatabase;
            this.ratingSummarizer = new RatingSummarizer(UserPreferences.DefaultPreferences.HalfLife);
            this.squared_shortTermScore_error = new Distribution();
            this.squared_longTermValue_error = new Distribution();
            this.squaredParticipationProbabilityError = new Distribution();
            this.valuePredictions = new Dictionary<Prediction, RatingSummary>();
        }

        public void AddInheritance(Inheritance newInheritance)
        {
            this.engine.PutInheritanceInMemory(newInheritance);
        }
        public void AddRequest(ActivityRequest newRequest)
        {
            this.engine.PutActivityRequestInMemory(newRequest);
        }
        public void AddSkip(ActivitySkip newSkip)
        {
            // update the error rate for the participation probability predictor
            this.UpdateParticipationProbabilityError(newSkip.ActivityDescriptor, newSkip.Date, 0);
            this.engine.PutSkipInMemory(newSkip);
            if (newSkip.SuggestionDate != null)
            {
                // inform the ratingSummarizer that the user wasn't doing anything during this time
                this.ratingSummarizer.AddParticipationIntensity((DateTime)newSkip.SuggestionDate, newSkip.Date, 0);
            }

        }
        public void AddParticipation(Participation newParticipation)
        {
            // update the error rate for the participation probability predictor

            if (newParticipation.Suggested == null || newParticipation.Suggested.Value == true)
            {
                // if the activity was certaintly not suggested, then we don't want to include it in our estimate 
                // of "the probability that the user would do the activity, given that it was suggested"
                this.UpdateParticipationProbabilityError(newParticipation.ActivityDescriptor, newParticipation.StartDate, newParticipation.TotalIntensity.Mean);
            }

            Rating rating = newParticipation.GetCompleteRating();
            if (rating != null)
                this.AddRating(rating);

            this.engine.PutParticipationInMemory(newParticipation);
            this.ratingSummarizer.AddParticipationIntensity(newParticipation.StartDate, newParticipation.EndDate, 1);
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

            this.Compute_FutureEstimate_Errors();

            Console.WriteLine("Adding rating with date" + ((DateTime)newRating.Date).ToString());
            this.UpdateScoreError(newRating.ActivityDescriptor, (DateTime)newRating.Date, newRating.Score);
            //this.engine.PutRatingInMemory(newRating); // this gets done when we put the participation in memory
            RatingSource ratingSource = newRating.Source;
            // the code that figures out where a rating came from only checks the most-recently-entered participation
            // Occasionally this participation doesn't match (and we don't yet bother scanning further back), so it's posible that the rating source can be null
            if (ratingSource != null)
            {
                Participation sourceParticipation = ratingSource.ConvertedAsParticipation;
                if (sourceParticipation != null)
                {
                    this.ratingSummarizer.AddRating(sourceParticipation.StartDate, sourceParticipation.EndDate, newRating.Score);
                }
            }
        }
        public void AddRating(RelativeRating newRating)
        {
#if true
            this.AddRating(newRating.FirstRating);
            this.AddRating(newRating.SecondRating);
#else

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
                // if the relative rating simply said "activity X is better than Y", then we compute the penalty based on the difference from what we computed
                double error;
                if (predictedBetterScore.Mean > predictedWorseScore.Mean)
                    error = 0;
                else
                    error = predictedWorseScore.Mean - predictedBetterScore.Mean;
                this.Update_ShortTerm_ScoreError(error);
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
                this.Update_ShortTerm_ScoreError(error);
            }

            this.engine.PutRatingInMemory(newRating);
#endif
        }
        // runs the engine on the given activity at the given date, and keeps track of the overall error
        public void UpdateScoreError(ActivityDescriptor descriptor, DateTime when, double correctScore)
        {
            /*if (((int)this.squaredScoreError.Weight) % 100 == 0)
            {
                Console.WriteLine("finished a hundred scores");
            }*/
            // update everything
            this.engine.MakeRecommendation(when);

            // compute the estimated score
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);

            // keep track of having made this prediction, so we can later return to it and compute the error
            Prediction prediction = activity.SuggestionValue;
            RatingSummary summary = new RatingSummary(when);
            this.valuePredictions[prediction] = summary;
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
            this.Update_ShortTerm_ScoreError(activity.PredictedScore.Distribution.Mean - correctScore);
        }
        public void Update_ShortTerm_ScoreError(double error)
        {
            if (Math.Abs(error) > 1)
                Console.WriteLine("error");

            Distribution errorDistribution = Distribution.MakeDistribution(error * error, 0, 1);
            this.squared_shortTermScore_error = this.squared_shortTermScore_error.Plus(errorDistribution);
        }
        public void Update_LongTerm_ValueError(double error)
        {
            if (Math.Abs(error) > 1)
                Console.WriteLine("error");

            Distribution errorDistribution = Distribution.MakeDistribution(error * error, 0, 1);
            this.squared_longTermValue_error = this.squared_longTermValue_error.Plus(errorDistribution);
        }
        public void UpdateParticipationProbabilityError(ActivityDescriptor descriptor, DateTime when, double actualIntensity)
        {
            // update everything
            this.engine.MakeRecommendation(when);
            // compute the estimate participation probability
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            double error = activity.PredictedParticipationProbability.Distribution.Mean - actualIntensity;
            /*if (Math.Abs(error) > 0.7 && when.CompareTo(DateTime.Parse("2012-3-1")) > 0)
            {
                engine.EstimateValue(activity, when);
            }*/
            /*if (this.squaredParticipationProbabilityError.Weight > 100 && Math.Abs(error) > 0.75)
            {
                Console.WriteLine("fairly large error");
            }*/
            Distribution errorDistribution = Distribution.MakeDistribution(error * error, 0, 1);
            this.squaredParticipationProbabilityError = this.squaredParticipationProbabilityError.Plus(errorDistribution);
        }
        public void Finish()
        {
            this.Compute_FutureEstimate_Errors();
            this.PrintResults();
        }

        // computes the error for each prediction that was made about the future
        public void Compute_FutureEstimate_Errors()
        {
            Distribution errorsSquared = new Distribution();
            foreach (Prediction prediction in this.valuePredictions.Keys)
            {
                RatingSummary summary = this.valuePredictions[prediction];
                summary.Update(this.ratingSummarizer);
                double predictedScore = prediction.Distribution.Mean;
                double actualScore = summary.Score.Mean;
                double error = actualScore - predictedScore;
                errorsSquared = errorsSquared.Plus(Distribution.MakeDistribution(error * error, 0, 1));
            }
            double errorSquared = errorsSquared.Mean;
            double typicalPredictionError = Math.Sqrt(errorSquared);
            Console.WriteLine("typical longtermPrediction error = " + typicalPredictionError);
        }
        public void PrintResults()
        {
            double typicalScoreError = Math.Sqrt(this.squared_shortTermScore_error.Mean);
            Console.WriteLine("typicalScoreError = " + typicalScoreError.ToString());
            double typicalProbabilityError = Math.Sqrt(this.squaredParticipationProbabilityError.Mean);
            Console.WriteLine("typicalProbabilityError = " + typicalProbabilityError.ToString());
            // X * (1 - X) ^ 2 + (1 - X) * X ^ 2 = this.squaredParticipationProbabilityError.Mean
            // X * (1 - X) = this.squaredParticipationProbabilityError.Mean
            // X ^ 2 - X + this.squaredParticipationProbabilityError.Mean = 0
            double equivalentProbability = (1 + Math.Sqrt(1 - 4 * this.squaredParticipationProbabilityError.Mean)) / 2;
            Console.WriteLine("equivalentProbability = " + equivalentProbability.ToString());
        }
        // a measure of how far off the engine's predicted scores are from the scores the user actually provides
        public Distribution SquaredScoreError
        {
            get
            {
                return this.squared_shortTermScore_error;
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
        private Distribution squared_shortTermScore_error;
        private Distribution squared_longTermValue_error;
        private Distribution squaredParticipationProbabilityError;
        private RatingSummarizer ratingSummarizer;
        private Dictionary<Prediction, RatingSummary> valuePredictions; // given a prediction, returns an object that can determine what the actual ratings were

    }
}
