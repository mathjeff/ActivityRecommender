﻿using System;
using System.Collections.Generic;

// the EngineTester makes an Engine and calculates its average squared error
// The first time (on 2012-1-15) that I calculated the Root(Mean(Squared(Error))), it was about 0.327

/*
Note that at some unknown time, the absolute ratings were removed from the error calculation because they were both unimportant and also easy to predict
The implementation of the RelativeRating was done on 2012-01-06, so it happened some time after that

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
 
results (on 2013-6-29) with new data, and also a new metric that weights predictions more heavily as the participation probability approaches 0 or 1
typicalScoreError = 0.181744174188761
typicalProbabilityError = 0.366714378232788
equivalentProbability = 0.839883163450824
weightedProbabilityScore = 0.320958529681715
equivalentWeightedProbability = 0.922818404503476
typical longtermPrediction error = 0.14057449369587

results (on 2014-1-05) with new data and after multiplying by 4 the frequency at which RatingSummaries get updated
typical longtermPrediction error = 0.139459605031934
typicalScoreError = 0.17654153306917
typicalProbabilityError = 0.360819685054036
equivalentProbability = 0.846134590697761
weightedProbabilityScore = 0.363960173151333
equivalentWeightedProbability = 0.929703311206423

results (on 2014-6-13) with new data
typical longtermPrediction error = forgot to check
typicalScoreError = 0.17827281478312368
typicalProbabilityError = 0.35591011031817193
equivalentProbability = 0.85118085564749491
weightedProbabilityScore = 0.38300734457703611
equivalentWeightedProbability = 0.93253643708711076

results (on 2014-06-14) with a slightly different algorithm in hopes of faster runtime, which was then reverted
typical longtermPrediction error = 0.137385512266307
typicalScoreError = 0.178869611844413
typicalProbabilityError = 0.357426209239901
equivalentProbability = 0.849637676671714
weightedProbabilityScore = 0.369311618622313
equivalentWeightedProbability = 0.930512129717936

results (on 2014-08-17) after making the activity progressions lazy but without yet incorporating data newer than 2014-06-08
typical longtermPrediction error = 0.137357092708936
typicalScoreError = 0.178515659914813
typicalProbabilityError = 0.369035683053002
equivalentProbability = 0.837361326523365
weightedProbabilityScore = 0.316766255842736
equivalentWeightedProbability = 0.922108643100204


results (on 2014-08-24) after splitting boxes (more quickly) using a better approximation of the median than just the middle of the bounding box, but without yet incorporating new data
typical longtermPrediction error = 0.137054692228206
typicalScoreError = 0.183816909567503
typicalProbabilityError = 0.380240802659689
equivalentProbability = 0.824679737576454
weightedProbabilityScore = 0.2146129963213
equivalentWeightedProbability = 0.902361777364356


results (on 2015-03-07) after getting more data (due to the passage of time)
typical longtermPrediction error = 0.133384666651803
typicalScoreError = 0.182199677921326
typicalProbabilityError = 0.389949448971249
equivalentProbability = 0.812952755614996
weightedProbabilityScore = 0.154158837166051
equivalentWeightedProbability = 0.888035588760429
 
 
results (on 2015-03-07) after making the AdaptiveInterpolator initially do some blind splits of some dimensions for a lot more startup speed and a little less complexity
typical longtermPrediction error = 0.133722875018643
typicalScoreError = 0.181529779100519
typicalProbabilityError = 0.387463782531814
equivalentProbability = 0.816025026265546
weightedProbabilityScore = 0.181927056060576
equivalentWeightedProbability = 0.894896166812173

results (on 2015-03-07) after new data (plus ignoring any parent that already has lots of considerations on startup)
typical longtermPrediction error = 0.133944756855872
typicalScoreError = 0.178996813609837
typicalProbabilityError = 0.393833200208718
equivalentProbability = 0.808050986710577
weightedProbabilityScore = 0.085217612980164
equivalentWeightedProbability = 0.868608281702737


results (on 2015-10-03) after new data
typical longtermPrediction error = 0.129196623418494
typicalScoreError = 0.180094412105045
typicalProbabilityError = 0.391209982636509
equivalentProbability = 0.811375576250839
weightedProbabilityScore = 0.0721504114768844
equivalentWeightedProbability = 0.864486053300514


results (on 2015-10-18) after new data and some refactoring
typical longtermPrediction error = 0.132303474241546
typicalScoreError = 0.179842980952335
typicalProbabilityError = 0.392650963144817
equivalentProbability = 0.809556491034266
weightedProbabilityScore = 0.0968530252359711
equivalentWeightedProbability = 0.872152190471373

same results, same day, after fixing the dates of the skips

updated results (still on 2015-10-18) after using the RatingRenormalizer to recompute the absolute portion of each RelativeRating
 (note that this ignores the previous absolute ratings that originally weren't ignored and starts the ratings at 0.5)
typical longtermPrediction error = 0.0306978007961018
typicalScoreError = 0.119055754172878
typicalProbabilityError = 0.392650963144817
equivalentProbability = 0.809556491034266
weightedProbabilityScore = 0.0968530252359711
equivalentWeightedProbability = 0.872152190471373
 
 
results (on 2017-08-05) after getting more data (due to the passage of time)
typical longtermPrediction error = 0.11623802086738
typicalScoreError = 0.129285744904529
typicalProbabilityError = 0.340633910429981
equivalentProbability = 0.866017129469619
weightedProbabilityScore = 0.293795953009491
equivalentWeightedProbability = 0.918089807237037


results on 2017-08-13 (using data through 2017-08-05) after having modified the algorithm
The algorithm compares the cumulative time spent against its least-squares-regression-line, for predicting ratings and participation probabilities
typical longtermPrediction error = 0.104498926030268
typicalScoreError = 0.133200373078405
typicalProbabilityError = 0.342376710582286
equivalentProbability = 0.864387414781101
weightedProbabilityScore = 0.31189500109831
equivalentWeightedProbability = 0.921274895962403

updated results on 2017-08-13 (with new data)
typical longtermPrediction error = 0.104718699486009
typicalScoreError = 0.133140687667085
typicalProbabilityError = 0.342444414621633
equivalentProbability = 0.864323788537679
weightedProbabilityScore = 0.311459030520166
equivalentWeightedProbability = 0.921199799038855
 
updated results on 2018-02-04 with new data
typical longtermPrediction error = 0.0995526585842484
typicalScoreError = 0.142199611173665
typicalProbabilityError = 0.342892978994635
equivalentProbability = 0.863901641870691
weightedProbabilityScore = 0.293463192543033
equivalentWeightedProbability = 0.918029930119905

updated results on 2018-02-19 with new data
typical longtermPrediction error = 0.10743172314635
typicalScoreError = 0.138030913442599
typicalProbabilityError = 0.343135756731594
equivalentProbability = 0.86367272712184
weightedProbabilityScore = 0.287756896146186
equivalentWeightedProbability = 0.916995556809255

updated results on 2018-02-19 when using estimated ratings to help compute longterm value
typical longtermPrediction error = 0.0938182895594615
typicalScoreError = 0.138030913442599
typicalProbabilityError = 0.343135756731594
equivalentProbability = 0.86367272712184
weightedProbabilityScore = 0.287756896146186
equivalentWeightedProbability = 0.916995556809255

updated results on 2018-02-19 after some fixes
typical longtermPrediction error = 0.0891772964258622
typicalScoreError = 0.138030913442599
typicalProbabilityError = 0.343135756731594
equivalentProbability = 0.86367272712184
weightedProbabilityScore = 0.287756896146186
equivalentWeightedProbability = 0.916995556809255

updated results on 2018-06-17 with new data
typical longtermPrediction error = 0.0833641928173899
typicalScoreError = 0.137588255915862
typicalProbabilityError = 0.342836995815694
equivalentProbability = 0.863954384916668
weightedProbabilityScore = 0.288373617715065
equivalentWeightedProbability = 0.917108043178257

updated results on 2018-06-17 after having tuned some constants
typical longtermPrediction error = 0.0796780520090059
typicalScoreError = 0.137588255915862
typicalProbabilityError = 0.342836995815694
equivalentProbability = 0.863954384916668
weightedProbabilityScore = 0.288373617715065
equivalentWeightedProbability = 0.917108043178257

updated results on 2018-06-19 after getting a little bit of new data and adding longtermPredictionIfParticipated
typical longtermPredictionIfSuggested error = 0.0803238577232935
typical longtermPredictionIfParticipated error = 0.232883937714255
typicalScoreError = 0.137588255915862
typicalProbabilityError = 0.342831553588421
equivalentProbability = 0.863959511297823
weightedProbabilityScore = 0.288292895034349
equivalentWeightedProbability = 0.917093329453631

updated results on 2018-06-19 after improving the accuracy of Engine.Get_Overall_ParticipationEstimate
typical longtermPredictionIfSuggested error = 0.0803238577232935
typical longtermPredictionIfParticipated error = 0.072638658082502
typicalScoreError = 0.137588255915862
typicalProbabilityError = 0.342831553588421
equivalentProbability = 0.863959511297823
weightedProbabilityScore = 0.288292895034349
equivalentWeightedProbability = 0.917093329453631

updated results on 2018-07-29 after getting some new data
typical longtermPredictionIfSuggested error = 0.0769317230966445
typical longtermPredictionIfParticipated error = 0.0687857499511654
typicalScoreError = 0.137300194760722
typicalProbabilityError = 0.342963751469292
equivalentProbability = 0.863834942217085
weightedProbabilityScore = 0.285088970267528
equivalentWeightedProbability = 0.916506989305579

updated results on 2018-07-29 after some fixups discovered when switching to EstimateSuggestionValue instead of MakeRecommendation
typical longtermPredictionIfSuggested error = 0.0776676670385656
typical longtermPredictionIfParticipated error = 0.0677669750654978
typicalScoreError = 0.136244463813144
typicalProbabilityError = 0.337362033552108
equivalentProbability = 0.869035036707338
weightedProbabilityScore = 0.287448503981239
equivalentWeightedProbability = 0.916939244447084

updated results again on 2018-07-29 after a tiny bit more data, plus some tuning of Get_Overall_SuggestionEstimate to favor parent activities more highly
typical longtermPredictionIfSuggested error = 0.072384304970362
typical longtermPredictionIfParticipated error = 0.067750639659817
typicalScoreError = 0.136360253685622
typicalProbabilityError = 0.3373837026757
equivalentProbability = 0.869015226201894
weightedProbabilityScore = 0.290649609017511
equivalentWeightedProbability = 0.917521710819241

updated results on 2018-08-12 with new data
typical longtermPredictionIfSuggested error = 0.068462838080553
typical longtermPredictionIfParticipated error = 0.0640969948485463
typicalScoreError = 0.13508617311407
typicalProbabilityError = 0.337732398701453
equivalentProbability = 0.868696117239337
weightedProbabilityScore = 0.288184129377369
equivalentWeightedProbability = 0.917073499618037

updated results on 2018-08-12 after some tuning: having AdaptiveInterpolator consider more points for splitting,
and using TimeProgression.AbsoluteTime as a factor for predicting activity ratings
typical longtermPredictionIfSuggested error = 0.0660279601785022
typical longtermPredictionIfParticipated error = 0.0455410485831656
typicalScoreError = 0.140825893664029
typicalProbabilityError = 0.3396555229262
equivalentProbability = 0.866925231819413
weightedProbabilityScore = 0.307009231292974
equivalentWeightedProbability = 0.920428780086327
EngineTester completed in 00:03:00.5293930
 */


namespace ActivityRecommendation
{
    class EngineTester : HistoryReplayer
    {
        public EngineTester()
        {
            this.squared_shortTermScore_error = new Distribution();
            this.squared_longTermValue_error = new Distribution();
            this.squaredParticipationProbabilityError = new Distribution();
            this.participationPrediction_score = new Distribution();
            this.ratingSummarizer = new ExponentialRatingSummarizer(UserPreferences.DefaultPreferences.HalfLife);
            this.executionStart = DateTime.Now;
        }
        public override AbsoluteRating ProcessRating(AbsoluteRating newRating)
        {
            // ignore generated ratings
            if (!newRating.FromUser)
                return newRating;

            if (this.numRatings % 1000 == 0)
            {
                this.PrintResults();
                System.Diagnostics.Debug.WriteLine("Adding rating with date " + ((DateTime)newRating.Date).ToString());
            }
            this.numRatings++;


            this.UpdateScoreError(newRating.ActivityDescriptor, (DateTime)newRating.Date, newRating.Score);
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
            return newRating;
        }

        public override void PreviewParticipation(Participation newParticipation)
        {
            // update the error rate for the participation probability predictor

            if (newParticipation.Suggested == null || newParticipation.Suggested.Value == true)
            {
                // if the activity was certaintly not suggested, then we don't want to include it in our estimate 
                // of "the probability that the user would do the activity, given that it was suggested"
                this.UpdateParticipationProbabilityError(newParticipation.ActivityDescriptor, newParticipation.StartDate, newParticipation.TotalIntensity.Mean);
            }


            this.ratingSummarizer.AddParticipationIntensity(newParticipation.StartDate, newParticipation.EndDate, 1);

        }

        public override void PreviewSkip(ActivitySkip newSkip)
        {
            // update the error rate for the participation probability predictor
            this.UpdateParticipationProbabilityError(newSkip.ActivityDescriptor, newSkip.CreationDate, 0);
            // inform the ratingSummarizer that the user wasn't doing anything during this time
            this.ratingSummarizer.AddParticipationIntensity(newSkip.ConsideredSinceDate, newSkip.CreationDate, 0);
        }

        // runs the engine on the given activity at the given date, and keeps track of the overall error
        public void UpdateScoreError(ActivityDescriptor descriptor, DateTime when, double correctScore)
        {
            // compute estimated score
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            this.engine.EstimateSuggestionValue(activity, when);

            // compute error
            //System.Diagnostics.Debug.WriteLine(descriptor.ActivityName + " - expected : " + activity.PredictedScore.Distribution.Mean + " actual : " + correctScore);
            this.Update_ShortTerm_ScoreError(activity.PredictedScore.Distribution.Mean - correctScore);
        }
        public void Update_ShortTerm_ScoreError(double error)
        {
            if (Math.Abs(error) > 1)
                System.Diagnostics.Debug.WriteLine("error");

            Distribution errorDistribution = Distribution.MakeDistribution(error * error, 0, 1);
            this.squared_shortTermScore_error = this.squared_shortTermScore_error.Plus(errorDistribution);
        }
        public void UpdateParticipationProbabilityError(ActivityDescriptor descriptor, DateTime when, double actualIntensity)
        {
            // compute the estimate participation probability
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            this.engine.EstimateSuggestionValue(activity, when);

            // keep track of having made this prediction, so we can later return to it and compute the error
            Prediction predictionForSuggestion = activity.SuggestionValue;
            if (predictionForSuggestion.Distribution.Weight > 0)
            {
                RatingSummary summary = new RatingSummary(when);
                this.valueIfSuggested_predictions[predictionForSuggestion] = summary;

                if (actualIntensity >= 1)
                {
                    Prediction predictionIfParticipated = this.engine.Get_Overall_ParticipationEstimate(activity, when);
                    this.valueIfParticipated_predictions[predictionIfParticipated] = summary;
                }
            }

            double predictedProbability = activity.PredictedParticipationProbability.Distribution.Mean;
            double error = predictedProbability - actualIntensity;
            Distribution errorDistribution = Distribution.MakeDistribution(error * error, 0, 1);
            this.squaredParticipationProbabilityError = this.squaredParticipationProbabilityError.Plus(errorDistribution);
            // We could attempt to predict the difference between the actual and expected participation probability, but that isn't quite what we want
            // The difference is that there's an important difference between probability 0.01 and probability 0.1, whereas the difference between 0.5 and 1 is not so important

            // Need a function f(numSkips, numParticipations, p) such that:
            //   f is minimized when p = (numParticipations + 1) / (numSkips + numParticipations + 2)
            //   it is always better for a prediction to move closer to the right answer (it's not acceptable to average all the predictions and compare them to the final average)
            // abs(f') is large when numParticipations / (numSkips + numParticipations) is small
            //// previously I've been using (1 - p)^2 * participationFraction + (p)^2 * skipFraction
            //// Note df/dp = -2 (1 - p) * participationFraction + 2 * p * (1 - participationFraction) = 2 * p * participationFraction - 2 * participationFraction - 2 * p * participationFraction + 2 * p
            //// Which is zero when p = participationFraction
            // Consider this error function:
            //   If the user did the activity, then the score is -1 / p
            //   If the user did not do the activity, then the score is -ln(p)
            //// Then, the total score = -numParticipations / p + numSkips * -ln(p)
            //// Which has derivative = numParticipations / (p^2) + -numSkips / p = (1/p^2) * (numParticipations - numSkips * p)
            //// Which clearly is zero when p = numParticipations / numSkips, although unfortunately that's not quite the goal (but it's close)
            // Consider this error function"
            //   If the user did the activity, then the score is -1 / p - ln(p)
            //   If the user did not do the activity, then the score is -ln(p)
            //// Then, the total score = -numParticipations / p + numTrials * -ln(p)
            //// Which has derivative = numParticipations / (p^2) - numTrials / p = (p^2) * (numParticipations - numTrials * p)
            //// Which clearly is zero when p = numParticipations / numTrials
            //   Note, however, that this function causes the best score to be limited by the outcomes being predicted: if the user always does the activity, we can't get average score more than 0
            //   So, we add a flipped version of this function, too:
            // The final function, then, is:
            //   If the user did the activity, then the score is -1 / p - ln(p) - ln(1 - p)
            //   If the user did not do the activity, then the score is -1 / (1 - p) - ln(p) - ln(1 - p)

            if (predictedProbability != 0)
            {
                // This is the component that we care about, because it gives stronger weight to cases where the probability is very small
                double scoreComponent = -Math.Log(predictedProbability) + -actualIntensity / predictedProbability;
                this.participationPrediction_score = this.participationPrediction_score.Plus(Distribution.MakeDistribution(scoreComponent, 0, 1));
            }
            if (predictedProbability != 1)
            {
                // This component is added for symmetry, so it is possible to get an arbitrarily large score even if the user did do the activity
                double scoreComponent = -Math.Log(1 - predictedProbability) + (actualIntensity - 1) / (1 - predictedProbability);
                this.participationPrediction_score = this.participationPrediction_score.Plus(Distribution.MakeDistribution(scoreComponent, 0, 1));
            }
        }

        private double Compute_FutureEstimateIfSuggested_Errors()
        {
            Distribution errorsSquared = new Distribution();
            foreach (Prediction prediction in this.valueIfSuggested_predictions.Keys)
            {
                RatingSummary summary = this.valueIfSuggested_predictions[prediction];
                summary.Update(this.ratingSummarizer);
                if (summary.Item.Weight > 0)
                {
                    double predictedScore = prediction.Distribution.Mean;
                    double actualScore = summary.Item.Mean;
                    double error = actualScore - predictedScore;
                    errorsSquared = errorsSquared.Plus(Distribution.MakeDistribution(error * error, 0, 1));
                }
            }
            double errorSquared = errorsSquared.Mean;
            double typicalPredictionError = Math.Sqrt(errorSquared);
            return typicalPredictionError;
        }
        private double  Compute_FutureEstimateIfParticipated_Errors()
        {
            Distribution errorsSquared = new Distribution();
            foreach (Prediction prediction in this.valueIfParticipated_predictions.Keys)
            {
                RatingSummary summary = this.valueIfParticipated_predictions[prediction];
                summary.Update(this.ratingSummarizer);
                if (summary.Item.Weight > 0)
                {
                    double predictedScore = prediction.Distribution.Mean;
                    double actualScore = summary.Item.Mean;
                    double error = actualScore - predictedScore;
                    errorsSquared = errorsSquared.Plus(Distribution.MakeDistribution(error * error, 0, 1));
                }
            }
            double errorSquared = errorsSquared.Mean;
            double typicalPredictionError = Math.Sqrt(errorSquared);
            return typicalPredictionError;
        }

        public EngineTesterResults Results
        {
            get
            {
                EngineTesterResults results = new EngineTesterResults();
                results.Longterm_PredictionIfSuggested_Error = this.Compute_FutureEstimateIfSuggested_Errors();
                results.Longterm_PredictionIfParticipated_Error = this.Compute_FutureEstimateIfParticipated_Errors();
                
                // how well the score prediction does
                results.TypicalScoreError = Math.Sqrt(this.squared_shortTermScore_error.Mean);

                // how well the probability prediction does (first using the mean-squared-error, which doesn't take into account what we want to do with this data)
                //double typicalProbabilityError = Math.Sqrt(this.squaredParticipationProbabilityError.Mean);
                //System.Diagnostics.Debug.WriteLine("typicalProbabilityError = " + typicalProbabilityError.ToString());
                // X * (1 - X) ^ 2 + (1 - X) * X ^ 2 = this.squaredParticipationProbabilityError.Mean
                // X * (1 - X) = this.squaredParticipationProbabilityError.Mean
                // X ^ 2 - X + this.squaredParticipationProbabilityError.Mean = 0
                //double equivalentProbability = (1 + Math.Sqrt(1 - 4 * this.squaredParticipationProbabilityError.Mean)) / 2;
                //System.Diagnostics.Debug.WriteLine("equivalentProbability = " + equivalentProbability.ToString());
                // Now recompute how well the probability prediction does (weighting smaller probabilities more heavily)
                //double weightedProbabilityScore = this.participationPrediction_score.Mean;
                //System.Diagnostics.Debug.WriteLine("weightedProbabilityScore = " + weightedProbabilityScore);
                // scoreComponent = 0.5 * (-Math.Log(predictedProbability) + -actualIntensity / predictedProbability + -Math.Log(1 - predictedProbability) + (actualIntensity - 1) / (1 - predictedProbability))
                // scoreComponent = 0.5 * (-Math.Log(predictedProbability) + -Math.Log(1 - predictedProbability) - 2)
                // 2 * scoreComponent + 2 = (-Math.Log(predictedProbability * (1 - predictedProbability)))
                // predictedProbability * (1 - predictedProbability) = e ^ (-2 * this.participationPrediction_score.Mean - 2);
                // X * X - X + e ^ (-2 * this.participationPrediction_score.Mean - 2) = 0
                // X = (1 + sqrt(1 - 4 * e ^ (-2 * this.participationPrediction_score.Mean - 2))) / 2;
                results.TypicalProbability = (1 + Math.Sqrt(1 - 4 * Math.Exp(-2 * this.participationPrediction_score.Mean - 2))) / 2;
                //System.Diagnostics.Debug.WriteLine("equivalentWeightedProbability = " + equivalentWeightedProbability);
                return results;
            }
        }

        public void PrintResults()
        {
            EngineTesterResults results = this.Results;

            System.Diagnostics.Debug.WriteLine("typical longtermPredictionIfSuggested error = " + results.Longterm_PredictionIfSuggested_Error);
            System.Diagnostics.Debug.WriteLine("typical longtermPredictionIfParticipated error = " + results.Longterm_PredictionIfParticipated_Error);
            System.Diagnostics.Debug.WriteLine("typicalScoreError = " + results.TypicalScoreError);
            System.Diagnostics.Debug.WriteLine("equivalentWeightedProbability = " + results.TypicalProbability);
        }

        private void PrintFinalResults()
        {
            this.PrintResults();
            DateTime executionEnd = DateTime.Now;
            TimeSpan duration = executionEnd.Subtract(this.executionStart);
            System.Diagnostics.Debug.WriteLine("EngineTester completed in " + duration);
        }
        public override Engine Finish()
        {
            this.PrintFinalResults();
            return null;
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


        private Distribution squared_shortTermScore_error;
        private Distribution squared_longTermValue_error;
        private Distribution squaredParticipationProbabilityError;
        private Distribution participationPrediction_score;
        private RatingSummarizer ratingSummarizer;
        // the Engine predicts longterm value based on its suggestions. valueIfSuggested_predictions maps the predictions made to the actual observed longterm value
        private Dictionary<Prediction, RatingSummary> valueIfSuggested_predictions = new Dictionary<Prediction, RatingSummary>();
        // the Engine predicts longterm value based on what the user participates in. valueIfParticipated_predictions maps the predictions made to the actual observed longterm value
        private Dictionary<Prediction, RatingSummary> valueIfParticipated_predictions = new Dictionary<Prediction, RatingSummary>();
        private int numRatings;
        private DateTime executionStart;
    }

    public class EngineTesterResults
    {
        // overall error in (the estimated and actual (longterm happiness prediction if (the given activity is suggested)))
        public double Longterm_PredictionIfSuggested_Error;
        // overall error in (the estimated and actual (longterm happiness prediction if (the given activity is participated in)))
        public double Longterm_PredictionIfParticipated_Error;
        // typical error in the score prediction
        public double TypicalScoreError;
        // An estimate of the accuracy of the estimates of participation probability.
        // This is the probability such that if the true probability were this value, and if all estimates were perfect, the overall error would be what was observed
        public double TypicalProbability;
    }
}