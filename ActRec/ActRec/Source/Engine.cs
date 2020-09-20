﻿using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;

namespace ActivityRecommendation
{
    // The Engine class analyzes information in various Activity objects and can, for example, make recommendations
    // The Engine class doesn't have any knowledge of the user interface or about persisting any data across application restarts; that's handled by ActivityRecommender
    public class Engine
    {
        public Engine()
        {
            this.weightedRatingSummarizer = new ExponentialRatingSummarizer(UserPreferences.DefaultPreferences.HalfLife);
            this.efficiencySummarizer = new ExponentialRatingSummarizer(UserPreferences.DefaultPreferences.EfficiencyHalflife);
            this.activityDatabase = new ActivityDatabase(this.weightedRatingSummarizer, this.efficiencySummarizer);
            this.activityDatabase.ActivityAdded += ActivityDatabase_ActivityAdded;
            foreach (Activity activity in this.activityDatabase.AllActivities)
            {
                this.ActivityDatabase_ActivityAdded(activity);
            }
            this.activityDatabase.InheritanceAdded += ActivityDatabase_InheritanceAdded;
            this.unappliedRatings = new List<AbsoluteRating>();
            this.unappliedParticipations = new List<Participation>();
            this.unappliedSkips = new List<ActivitySkip>();
            this.unappliedSuggestions = new List<ActivitySuggestion>();
            this.firstInteractionDate = DateTime.Now;
            this.latestInteractionDate = new DateTime(0);
            this.thinkingTime = Distribution.MakeDistribution(60, 0, 1);      // default amount of time thinking about a suggestion is 1 minute
            this.ratingsOfUnpromptedActivities = Distribution.Zero;

            this.happinessFromEfficiency_predictor = new LongtermValuePredictor(
                new AdaptiveLinearInterpolation.HyperBox<Distribution>(
                    new AdaptiveLinearInterpolation.FloatRange[] {
                        new AdaptiveLinearInterpolation.FloatRange(0, true, 1, true)
                    }
                ),
                this.weightedRatingSummarizer
            );
        }

        // gives to the necessary objects the data that we've read. Optimized for when there are large quantities of data to give to the different objects
        public void FullUpdate()
        {
            DateTime start = DateTime.Now;
            this.ApplyParticipationsAndRatings();
            this.requiresFullUpdate = false;
            DateTime end = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Engine.FullUpdate completed in " + (end - start));
        }
        public void EnsureRatingsAreAssociated()
        {
            if (this.requiresFullUpdate)
                this.FullUpdate();
            else
                this.ApplyParticipationsAndRatings();
        }

        // moves the ratings and participations from the pending queues into the Activities
        // Note that there is a bug at the moment: when an inheritance is added to an activity, all of its ratings need to cascade appropriately
        // currently they don't. However, as long as the inheritances are always added before this function is called, then it's fine
        public void ApplyParticipationsAndRatings()
        {
            DateTime nextDate;
            int numRatingsNewlyApplied = this.unappliedRatings.Count;
            int ratingIndex = 0;
            int participationIndex = 0;
            int skipIndex = 0;
            int suggestionIndex = 0;
            while (true)
            {
                // find the next date at which something happened
                List<DateTime> dates = new List<DateTime>(4);
                if (this.unappliedRatings.Count > ratingIndex)
                    dates.Add((DateTime)this.unappliedRatings[ratingIndex].Date);
                if (this.unappliedParticipations.Count > participationIndex)
                    dates.Add((DateTime)this.unappliedParticipations[participationIndex].StartDate);
                if (this.unappliedSkips.Count > skipIndex)
                    dates.Add((DateTime)this.unappliedSkips[skipIndex].CreationDate);
                if (this.unappliedSuggestions.Count > suggestionIndex)
                    dates.Add(this.unappliedSuggestions[suggestionIndex].CreatedDate);
                if (dates.Count == 0)
                    break;
                nextDate = dates[0];
                foreach (DateTime date in dates)
                {
                    if (date.CompareTo(nextDate) < 0)
                        nextDate = date;
                }
                // apply any data for the next date            
                // Optimization opportunity for the future: cache the lists of superCategories
                // finally, cascade all of the ratings and participations to each activity
                while (this.unappliedRatings.Count > ratingIndex && ((DateTime)this.unappliedRatings[ratingIndex].Date).CompareTo(nextDate) == 0)
                {
                    this.CascadeRating(this.unappliedRatings[ratingIndex]);
                    ratingIndex++;
                }
                while (this.unappliedParticipations.Count > participationIndex && ((DateTime)this.unappliedParticipations[participationIndex].StartDate).CompareTo(nextDate) == 0)
                {
                    this.CascadeParticipation(this.unappliedParticipations[participationIndex]);
                    participationIndex++;
                }
                while (this.unappliedSkips.Count > skipIndex && ((DateTime)this.unappliedSkips[skipIndex].CreationDate).CompareTo(nextDate) == 0)
                {
                    this.CascadeSkip(this.unappliedSkips[skipIndex]);
                    skipIndex++;
                }
                while (this.unappliedSuggestions.Count > suggestionIndex && ((DateTime)this.unappliedSuggestions[suggestionIndex].CreatedDate).CompareTo(nextDate) == 0)
                {
                    this.CascadeSuggestion(this.unappliedSuggestions[suggestionIndex]);
                    suggestionIndex++;
                }
            }
            this.unappliedRatings.Clear();
            this.unappliedParticipations.Clear();
            this.unappliedSkips.Clear();
            this.unappliedSuggestions.Clear();
            this.Update_RatingSummaries(numRatingsNewlyApplied);
        }



        private void ActivityDatabase_ActivityAdded(Activity activity)
        {
            this.CreatingActivity(activity);
        }

        // this function gets called when a new activity gets created
        public void CreatingActivity(Activity activity)
        {
            activity.SetDefaultDiscoveryDate(this.firstInteractionDate);
            activity.engine = this;
        }
        // gives the Rating to all Activities to which it applies
        public void CascadeRating(AbsoluteRating newRating)
        {
            ActivityDescriptor descriptor = newRating.ActivityDescriptor;
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            if (activity != null)
            {
                List<Activity> superCategories = this.FindAllSupercategoriesOf(activity);
                foreach (Activity parent in superCategories)
                {
                    parent.AddRating(newRating);
                }
            }
        }
        // gives the Participation to all Activities to which it applies
        public void CascadeParticipation(Participation newParticipation)
        {
            // give the participation to any relevant Activity
            ActivityDescriptor descriptor = newParticipation.ActivityDescriptor;
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            if (activity != null)
            {
                List<Activity> superCategories = this.FindAllSupercategoriesOf(activity);
                foreach (Activity parent in superCategories)
                {
                    parent.AddParticipation(newParticipation);
                }
            }
            RelativeEfficiencyMeasurement efficiencyMeasurement = newParticipation.RelativeEfficiencyMeasurement;
            this.addEfficiencyMeasurement(efficiencyMeasurement);
        }
        private void addEfficiencyMeasurement(RelativeEfficiencyMeasurement measurement)
        {
            if (measurement == null)
                return;
            this.addEfficiencyMeasurement(measurement.Earlier);
            this.efficiencySummarizer.AddRating(measurement.StartDate, measurement.EndDate, measurement.RecomputedEfficiency.Mean);
            Activity activity = this.ActivityDatabase.ResolveDescriptor(measurement.ActivityDescriptor);
            foreach (Activity parent in this.FindAllSupercategoriesOf(activity))
            {
                parent.AddEfficiencyMeasurement(measurement);
            }
            this.happinessFromEfficiency_predictor.AddDatapoint(measurement.StartDate, new double[] { measurement.RecomputedEfficiency.Mean });
        }
        // gives the Skip to all Activities to which it applies
        public void CascadeSkip(ActivitySkip newSkip)
        {
            ActivityDescriptor descriptor = newSkip.ActivityDescriptor;
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            if (activity != null)
            {
                List<Activity> superCategories = this.FindAllSupercategoriesOf(activity);
                foreach (Activity parent in superCategories)
                {
                    parent.AddSkip(newSkip);
                }
            }
        }
        public void CascadeSuggestion(ActivitySuggestion newSuggestion)
        {
            ActivityDescriptor descriptor = newSuggestion.ActivityDescriptor;
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            if (activity != null)
            {
                List<Activity> superCategories = this.FindAllSupercategoriesOf(activity);
                foreach (Activity parent in superCategories)
                {
                    parent.AddSuggestion(newSuggestion);
                }
            }
        }
        // performs Depth First Search to find all superCategories of the given Activity
        public List<Activity> FindAllSupercategoriesOf(Activity child)
        {
            return this.activityDatabase.GetAllSuperactivitiesOf(child);
        }
        // performs Depth First Search to find all subCategories of the given Activity
        public List<Activity> FindAllSubCategoriesOf(Activity parent)
        {
            return parent.GetChildrenRecursive();
        }
        public ActivitySuggestion MakeRecommendation()
        {
            ActivityRequest request = new ActivityRequest();
            return this.MakeRecommendation(request);
        }
        public ActivitySuggestion MakeRecommendation(DateTime when)
        {
            ActivityRequest request = new ActivityRequest(null, null, when);
            return this.MakeRecommendation(request);
        }

        private List<Activity> getActivitiesToConsider(ActivityRequest request)
        {
            List<Activity> candidates;
            if (request.LeafActivitiesToConsider != null)
            {
                candidates = request.LeafActivitiesToConsider;
            }
            else
            {
                if (request.FromCategory != null)
                    candidates = this.activityDatabase.ResolveDescriptor(request.FromCategory).GetChildrenRecursive();
                else
                    candidates = new List<Activity>(this.activityDatabase.AllActivities);
            }
            return candidates;
        }
        public ActivitySuggestion MakeRecommendation(ActivityRequest request)
        {
            DateTime when = request.Date;
            Activity activityToBeat = this.activityDatabase.ResolveDescriptor(request.ActivityToBeat);

            List<Activity> candidates = this.getActivitiesToConsider(request);
            TimeSpan? requestedProcessingTime = request.RequestedProcessingTime;

            DateTime processingStartTime = DateTime.Now;
            ActivityRecommendationsAnalysis recommendationsCache = this.cacheForDate(when);

            // First, go update the stats for existing activities
            this.EnsureRatingsAreAssociated();

            List<Activity> consideredCandidates = new List<Activity>();

            // Now we determine which activity is most important to suggest
            // That requires first finding the one with the highest mean
            Activity bestActivity = null;
            Activity mostFunActivity = null;
            if (activityToBeat != null)
            {
                // If the user has given another activity that they're tempted to try instead, then evaluate that activity
                // Use its short-term value as a minimum when considering other activities
                this.UpdateSuggestionValue(activityToBeat, when);
                recommendationsCache.suggestionValues[activityToBeat] = new Prediction(activityToBeat, Distribution.MakeDistribution(0, 0, 1), when, "You asked for an activity at least as fun as this one");
                //recommendationsCache.utilities[activityToBeat] = 0; // if they're asking for us to beat this activity then it means they would rather do something else
                //Prediction participationPrediction = new Prediction(activityToBeat, Distribution.MakeDistribution(0, 0, 1), when, "You asked for an activity at least as fun as this one");
                //recommendationsCache.participationProbabilities[activityToBeat] = participationPrediction;
                if (candidates.Contains(activityToBeat))
                {
                    candidates.Remove(activityToBeat);
                }
                // Here we add the activityToBeat as a candidate so that if activityToBeat is the best activity, then we still have give a suggestion
                //bestActivity = activityToBeat;
                consideredCandidates.Add(activityToBeat);
            }
            while (candidates.Count > 0)
            {
                // Choose a random activity.
                // Note that it would be more representative to weight activities by how often they're suggested
                // However, those activities take longer to process, so for now the hope is that if we choose a rare activity that it'll be fast
                // to process and to move on to the next activity
                int index = this.randomGenerator.Next(candidates.Count);
                Activity candidate = candidates[index];
                candidates.RemoveAt(index);
                if (candidate.Suggestible)
                {
                    // estimate how good it is for us to suggest this particular activity
                    this.UpdateSuggestionValue(candidate, when);
                    Distribution currentRating = recommendationsCache.suggestionValues[candidate].Distribution;
                    bool better = false;
                    if (activityToBeat != null && recommendationsCache.utilities[candidate] < recommendationsCache.utilities[activityToBeat])
                    {
                        better = false; // user doesn't have enough will power to do this activity
                    }
                    else
                    {
                        if (bestActivity == null)
                            better = true; // found a better activity
                        else
                        {
                            switch (request.Optimize)
                            {
                                case ActivityRequestOptimizationProperty.LONGTERM_HAPPINESS:
                                    better = recommendationsCache.suggestionValues[candidate].Distribution.Mean >= recommendationsCache.suggestionValues[bestActivity].Distribution.Mean;
                                    break;
                                case ActivityRequestOptimizationProperty.PARTICIPATION_PROBABILITY:
                                    better = recommendationsCache.participationProbabilities[candidate].Distribution.Mean >= recommendationsCache.participationProbabilities[bestActivity].Distribution.Mean;
                                    break;
                                default:
                                    throw new ArgumentException("Unsupported activity request optimization property: " + request.Optimize);
                            }
                        }
                    }
                    if (better)
                    {
                        /*
                        if (bestActivity != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Candidate " + candidate + " with suggestion value " + recommendationsCache.suggestionValues[candidate].Distribution.Mean + " replaced " + bestActivity + " with suggestion value " + recommendationsCache.suggestionValues[bestActivity].Distribution.Mean + " as highest-value suggestion");
                        }
                        */
                        bestActivity = candidate;
                    }
                    if (activityToBeat != null)
                    {
                        if (mostFunActivity == null || recommendationsCache.utilities[candidate] >= recommendationsCache.utilities[mostFunActivity])
                        {
                            /*
                            if (mostFunActivity != null)
                            {
                                System.Diagnostics.Debug.WriteLine("Candidate " + candidate + " with utility value " + recommendationsCache.utilities[candidate] + " replaced " + mostFunActivity + " with suggestion value " + recommendationsCache.utilities[mostFunActivity] + " as most-fun suggestion");
                            }
                            */
                            mostFunActivity = candidate;
                        }
                    }
                    consideredCandidates.Add(candidate);
                    if (consideredCandidates.Count >= 2 && requestedProcessingTime != null)
                    {
                        // check whether we've used up all of our processing time, but always consider at least two activities
                        DateTime now = DateTime.Now;
                        TimeSpan spentDuration = now.Subtract(processingStartTime);
                        if (spentDuration.CompareTo(requestedProcessingTime.Value) >= 0)
                        {
                            // we've used up all of our processing time; clean up and return
                            System.Diagnostics.Debug.WriteLine("Spent " + spentDuration + " to consider " + consideredCandidates.Count + " activities");
                            break;
                        }
                    }
                }
            }
            // some fallbacks in case no activity matched
            if (bestActivity == null)
            {
                bestActivity = mostFunActivity;
                if (bestActivity == null)
                    bestActivity = activityToBeat;
            }
            // After finding the activity with the highest expected rating, we need to check for other activities having high variance but almost-as-good values
            Activity bestActivityToPairWith = bestActivity;
            if (bestActivity == null)
                return null;
            double bestCombinedScore = GetCombinedValue(bestActivity, bestActivityToPairWith, recommendationsCache);
            foreach (Activity candidate in consideredCandidates)
            {
                double currentScore = this.GetCombinedValue(bestActivity, candidate, recommendationsCache);
                if (currentScore > bestCombinedScore)
                {
                    //System.Diagnostics.Debug.WriteLine("Candidate " + candidate + " with suggestion value " + candidate.SuggestionValue.Distribution.Mean + " replaced " + bestActivityToPairWith + " as most important suggestion to make");
                    bestActivityToPairWith = candidate;
                    bestCombinedScore = currentScore;
                }
            }
            // If there was a pair of activities that could do strictly better than two of the best activity, then we must actually choose the second-best
            // If there was no such pair, then we just want to choose the best activity because no others could help
            // Remember that the reason the activity with second-highest rating might be a better choice is that it might have a higher variance
            return this.SuggestActivity(bestActivityToPairWith, when);
        }
        private ActivitySuggestion SuggestActivity(Activity activity, DateTime when)
        {
            DateTime now = DateTime.Now;
            ActivitySuggestion suggestion = new ActivitySuggestion(activity.MakeDescriptor());
            suggestion.CreatedDate = now;
            suggestion.StartDate = when;
            suggestion.EndDate = this.GuessParticipationEndDate(activity, when);
            suggestion.ParticipationProbability = this.EstimateParticipationProbability(activity, when).Distribution.Mean;
            double average = this.ActivityDatabase.RootActivity.Ratings.Mean;
            if (average == 0)
                average = 1;
            suggestion.PredictedScoreDividedByAverage = this.EstimateRating(activity, when).Distribution.Mean / average;
            return suggestion;
        }
        public DateTime GuessParticipationEndDate(Activity activity, DateTime start)
        {
            ParticipationsSummary participationSummary = activity.SummarizeParticipationsBetween(new DateTime(), start);
            double typicalNumSeconds = Math.Exp(participationSummary.LogActiveTime.Mean);
            return start.Add(TimeSpan.FromSeconds(typicalNumSeconds));
        }
        public DateTime chooseRandomBelievableParticipationStart(Activity activity, DateTime actualStart)
        {
            if (activity.NumParticipations < 1)
            {
                // not enough data
                return actualStart;
            }
            TimeSpan averageIdlenessDuration = activity.ComputeAverageIdlenessDuration(actualStart);
            double averageIdleSeconds = averageIdlenessDuration.TotalSeconds;
            if (averageIdleSeconds <= 0)
            {
                // not enough data
                return actualStart;
            }
            DateTime latestParticipationEnd = activity.LatestParticipationDate;
            double currentIdleSeconds = actualStart.Subtract(latestParticipationEnd).TotalSeconds;
            double randomSeconds = (currentIdleSeconds + averageIdleSeconds) * this.randomGenerator.NextDouble();
            TimeSpan randomIdleTime = TimeSpan.FromSeconds(randomSeconds);
            return latestParticipationEnd.Add(randomIdleTime);
        }
        // This function essentially addresses the well-known multi-armed bandit problem
        // Given two distributions, we estimate the expected total value from choosing values from them
        private double GetCombinedValue(Activity activityA, Activity activityB, ActivityRecommendationsAnalysis recommendationsCache)
        {
            Distribution a = recommendationsCache.suggestionValues[activityA].Distribution;
            Distribution b = recommendationsCache.suggestionValues[activityB].Distribution;
            TimeSpan interval1 = activityA.AverageTimeBetweenConsiderations;
            TimeSpan interval2 = activityB.AverageTimeBetweenConsiderations;
            // Convert to BinomialDistribution
            BinomialDistribution distribution1 = new BinomialDistribution((1 - a.Mean) * 0.5 * a.Weight, (a.Mean + 1) * 0.5 * a.Weight);
            BinomialDistribution distribution2 = new BinomialDistribution((1 - b.Mean) * 0.5 * b.Weight, (b.Mean + 1) * 0.5 * b.Weight);

            // We weight our time exponentially, estimating that it takes two years for our time to double            
            // Here are the scales by which the importances are expected to multiply every time we consider these activities
            TimeSpan halfLife = this.Get_UserPreferences().HalfLife;
            double scale1 = Math.Pow(0.5, (interval1.TotalSeconds / halfLife.TotalSeconds));
            double scale2 = Math.Pow(0.5, (interval2.TotalSeconds / halfLife.TotalSeconds));

            // For now we just do a brief brute-force analysis
            Distribution distribution = this.GetCombinedValue(distribution1, scale1, distribution2, scale2, 4);

            return distribution.Mean;
        }
        // This function essentially addresses the multi-armed bandit problem
        // Given two distributions, we estimate the expected total value from choosing values from them
        // distributionA is the previously viewed values for option A
        // weightScaleA is a multiplier about how much we care about the results of the next iteration
        // We also assume here that each distribution is a binomial distribution
        private Distribution GetCombinedValue(BinomialDistribution distributionA, double weightScaleA, BinomialDistribution distributionB, double weightScaleB, int numIterations)
        {
            // For now we just do a brief brute-force analysis
            if (numIterations <= 0)
            {
                // simply choose the better activity and be done with it
                if (distributionA.Mean > distributionB.Mean)
                    return Distribution.MakeDistribution(distributionA.Mean, 0, 1);
                else
                    return Distribution.MakeDistribution(distributionB.Mean, 0, 1);
            }
            numIterations--;
            // We can choose A or B. Consider both and choose the better option
            BinomialDistribution luckyA = new BinomialDistribution(distributionA);
            BinomialDistribution unluckyA = new BinomialDistribution(distributionA);
            luckyA.NumOnes++;
            unluckyA.NumZeros++;
            BinomialDistribution luckyB = new BinomialDistribution(distributionB);
            BinomialDistribution unluckyB = new BinomialDistribution(distributionB);
            luckyB.NumOnes++;
            unluckyB.NumZeros++;
            // TODO: use Dynamic Programming to make this require O(n^2) time steps rather than O(4^n)
            Distribution luckyScoreA = GetCombinedValue(luckyA, weightScaleA, distributionB, weightScaleB, numIterations);
            Distribution unluckyScoreA = GetCombinedValue(unluckyA, weightScaleA, distributionB, weightScaleB, numIterations);
            Distribution scoreA = luckyScoreA.CopyAndReweightBy(distributionA.Mean).Plus(unluckyScoreA.CopyAndReweightBy(1 - distributionA.Mean));
            Distribution luckyScoreB = GetCombinedValue(distributionA, weightScaleA, luckyB, weightScaleB, numIterations);
            Distribution unluckyScoreB = GetCombinedValue(distributionA, weightScaleA, unluckyB, weightScaleB, numIterations);
            Distribution scoreB = luckyScoreB.CopyAndReweightBy(distributionB.Mean).Plus(unluckyScoreB.CopyAndReweightBy(1 - distributionB.Mean));

            Distribution earlyScore, lateScore;
            double lateWeight;
            if (scoreA.Mean > scoreB.Mean)
            {
                // choose option A
                earlyScore = Distribution.MakeDistribution(distributionA.Mean, 0, 1);
                lateScore = scoreA;
                lateWeight = weightScaleA;
            }
            else
            {
                // choose option B
                earlyScore = Distribution.MakeDistribution(distributionB.Mean, 0, 1);
                lateScore = scoreB;
                lateWeight = weightScaleB;
            }
            double earlyWeight = 1;
            double totalWeight = earlyWeight + lateWeight;
            Distribution score = earlyScore.Plus(lateScore.CopyAndReweightBy(lateWeight));
            return score;
        }
        // update the estimate of what rating the user would give to this activity now
        public Prediction EstimateRating(Activity activity, DateTime when)
        {
            // If we've already estimated the rating at this date, then just return what we calculated
            //DateTime latestUpdateDate = activity.PredictedScore.ApplicableDate;
            ActivityRecommendationsAnalysis recommendationsCache = this.cacheForDate(when);
            if (recommendationsCache.ratings.ContainsKey(activity))
                return recommendationsCache.ratings[activity];
            // If we get here, then we have to do some calculations
            // estimate the rating 
            activity.SetupPredictorsIfNeeded();
            // First make sure that all parents' ratings are up-to-date
            foreach (Activity parent in activity.ParentsUsedForPrediction)
            {
                this.EstimateRating(parent, when);
            }
            // Estimate the rating that the user would give to this activity if it were done
            List<Prediction> ratingPredictions = this.Get_ShortTerm_RatingEstimates(activity, when);
            // now that we've made a list of guesses, combine them to make one final guess of what we expect the user's rating to be
            Prediction ratingPrediction = this.CombineRatingPredictions(ratingPredictions);
            recommendationsCache.ratings[activity] = ratingPrediction;

            // Estimate the probability that the user would do this activity
            List<Prediction> probabilityPredictions = activity.GetParticipationProbabilityEstimates(when);
            Prediction probabilityPrediction = this.CombineProbabilityPredictions(probabilityPredictions);
            recommendationsCache.participationProbabilities[activity] = probabilityPrediction;


            recommendationsCache.utilities[activity] = this.RatingAndProbability_Into_Value(ratingPrediction.Distribution, probabilityPrediction.Distribution.Mean, activity.MeanParticipationDuration).Mean;
            return ratingPrediction;
        }

        private Distribution RatingAndProbability_Into_Value(Distribution rating, double suggestedParticipation_probability, double meanParticipationDuration)
        {
            // It's probably more accurate to assume that the user will make their own selection of an activity to do, but the user doesn't want us to model it like that
            // Because if we do model the possibility that the user will choose their own activity, then that can incentivize purposely bad suggestions to prompt the user to have to think of something
            double skipProbability = 1 - suggestedParticipation_probability;
            // double skipProbability = (1 - suggestedParticipation_probability) * (double)(this.numSkips + 1) / ((double)this.numSkips + (double)this.numUnpromptedParticipations + 2);
            double nonsuggestedParticipation_probability = 1 - (suggestedParticipation_probability + skipProbability);

            // Now compute the probability that the eventual selection will be a suggested activity (assuming that all activities are like this one)
            double probabilitySuggested = suggestedParticipation_probability / (suggestedParticipation_probability + nonsuggestedParticipation_probability);

            // Now compute the expected amount of wasted time
            double averageWastedSeconds = this.thinkingTime.Mean * (-1 + 1 / (1 - skipProbability));

            // Let p be the probability of skipping this activity. The expected number of skips then is p + p^2 ... = -1 + 1 + p + p^2... = -1 + 1 / (1 - p)
            // = -1 + 1 / (1 - (the probability that the user will skip the activity))
            // So the amount of waste is (the average length of a skip) * (-1 + 1 / (1 - (the probability that the user will skip the activity)))

            Distribution valueWhenChosen = rating.CopyAndReweightTo(probabilitySuggested).Plus(this.ratingsOfUnpromptedActivities.CopyAndReweightTo(1 - probabilitySuggested));
            // TODO: for activities other than this one, use a better estimate of the participation duration than just the usual duration of the current activity
            Distribution overallValue = valueWhenChosen.CopyAndReweightTo(meanParticipationDuration).Plus(Distribution.MakeDistribution(0, 0, averageWastedSeconds)).CopyAndReweightTo(rating.Weight);

            return overallValue;
        }

        // recompute the estime of how good it would be to suggest this activity now
        public Prediction EstimateSuggestionValue(Activity activity, DateTime when)
        {
            this.EnsureRatingsAreAssociated();
            this.UpdateSuggestionValue(activity, when);
            return this.currentRecommendationsCache.suggestionValues[activity];
        }

        // update the estimate of how good it would be to suggest this activity now, unless the computation is already up-to-date
        private void UpdateSuggestionValue(Activity activity, DateTime when)
        {
            ActivityRecommendationsAnalysis recommendationsCache = this.cacheForDate(when);
            // Now we estimate how useful it would be to suggest this activity to the user

            Prediction suggestionPrediction = this.Get_OverallHappiness_SuggestionEstimate(activity, when);
            recommendationsCache.suggestionValues[activity] = suggestionPrediction;
        }

        // attempt to calculate the probability that the user would do this activity if we suggested it at this time
        public Prediction EstimateParticipationProbability(Activity activity, DateTime when)
        {
            ActivityRecommendationsAnalysis recommendationsCache = this.cacheForDate(when);
            List<Prediction> predictions = activity.GetParticipationProbabilityEstimates(when);
            Prediction prediction = this.CombineProbabilityPredictions(predictions);
            recommendationsCache.participationProbabilities[activity] = prediction;
            return prediction;
        }

        // returns a list of Distributions that are to be used to estimate the rating the user will assign to this Activity
        private List<Prediction> Get_ShortTerm_RatingEstimates(Activity activity, DateTime when)
        {
            // Now that the parents' ratings are up-to-date, the work begins
            // Make a list of predictions based on all the different factors
            List<Prediction> predictions = activity.Get_ShortTerm_RatingEstimates(when);
            return predictions;
        }

        // returns a Prediction of what the user's longterm happiness will be after having participated in the given activity at the given time
        public Prediction Get_OverallHappiness_ParticipationEstimate(Activity activity, DateTime when)
        {
            // The activity might use its estimated rating to predict the overall future value, so we must update the rating now
            this.EstimateRating(activity, when);

            List<Distribution> distributions = new List<Distribution>();

            // When there is little data, we focus on the fact that doing the activity will probably be as good as doing that activity (or its parent activities)
            // When there is a medium amount of data, we focus on the fact that doing the activity will probably make the user as happy as having done the activity in the past

            Prediction shortTerm_prediction = this.CombineRatingPredictions(activity.Get_ShortTerm_RatingEstimates(when));
            double shortWeight = Math.Pow(activity.NumParticipations + 1, 0.4);
            shortTerm_prediction.Distribution = shortTerm_prediction.Distribution.CopyAndReweightTo(shortWeight);
            distributions.Add(shortTerm_prediction.Distribution);

            double mediumWeight = activity.NumParticipations;
            Distribution ratingDistribution = activity.Predict_LongtermValue_If_Participated(when);
            Distribution mediumTerm_distribution = ratingDistribution.CopyAndReweightTo(mediumWeight);
            distributions.Add(mediumTerm_distribution);

            if (activity.NumParticipations < 40)
            {
                // before we have much data, we predict that the happiness after doing this activity resembles that after doing its parent activities
                foreach (Activity parent in activity.ParentsUsedForPrediction)
                {
                    Distribution parentDistribution = this.Get_OverallHappiness_ParticipationEstimate(parent, when).Distribution.CopyAndReweightTo(4);
                    distributions.Add(parentDistribution);
                }
                // if this activity is correlated with efficiency, and if efficiency is correlated with happiness,
                // then this activity is correlated with happiness too
                Distribution efficiency = this.Get_OverallEfficiency_ParticipationEstimate(activity, when).Distribution;
                Distribution happinessFromEfficiency_prediction = this.Predict_LongtermHappiness_From_LongtermEfficiency(efficiency).CopyAndReweightTo(4);
                distributions.Add(happinessFromEfficiency_prediction);
            }

            Distribution distribution = this.CombineRatingDistributions(distributions);
            Prediction prediction = shortTerm_prediction;
            prediction.Distribution = distribution;

            return prediction;
        }

        public Distribution GetAverageLongtermValueWhenParticipated(Activity activity)
        {
            return activity.GetAverageLongtermValueWhenParticipated();
        }

        public Distribution compute_longtermValue_increase_in_days(Distribution averageHappinessValue)
        {
            Distribution chosenEstimatedDistribution = averageHappinessValue;
            if (chosenEstimatedDistribution.Weight <= 0)
                return new Distribution();
            Distribution baseValue = this.GetAverageLongtermValueWhenParticipated(this.activityDatabase.RootActivity);
            Distribution chosenValue = chosenEstimatedDistribution.CopyAndReweightTo(1);

            Distribution bonusInDays = new Distribution();
            // relWeight(x) = 2^(-x/halflife)
            // integral(relWeight) = -(log(e)/log(2))*halfLife*2^(-x/halflife)
            // totalWeight = (log(e)/log(2))*halflife
            // absWeight(x) = relWeight(x) / totalWeight
            // absWeight(x) = 2^(-x/halflife) / ((log(e)/log(2))*halflife)
            // absWeight(0) = log(2)/log(e)/halflife = log(2)/halflife
            double weightOfThisMoment = Math.Log(2) / UserPreferences.DefaultPreferences.HalfLife.TotalDays;
            if (baseValue.Mean > 0)
            {
                Distribution combined = baseValue.Plus(chosenValue);
                double overallAverage = combined.Mean;

                double relativeImprovement = (chosenValue.Mean - baseValue.Mean) / overallAverage;
                double relativeVariance = chosenValue.Variance / (overallAverage * overallAverage);
                Distribution difference = Distribution.MakeDistribution(relativeImprovement, Math.Sqrt(relativeVariance), 1);

                bonusInDays = difference.CopyAndStretchBy(1.0 / weightOfThisMoment);
            }
            return bonusInDays;
        }


        // returns a Prediction of the value of suggesting the given activity at the given time
        private List<Prediction> Get_OverallHappiness_SuggestionEstimates(Activity activity, DateTime when)
        {
            List<Prediction> distributions = new List<Prediction>();

            // The activity might use its estimated rating to predict the overall future value, so we must update the rating now
            this.EstimateRating(activity, when);
            Prediction probabilityPrediction = this.EstimateParticipationProbability(activity, when);

            // When there is little data, we focus on the fact that doing the activity will probably be as good as doing that activity (or its parent activities)
            // When there is a medium amount of data, we focus on the fact that doing the activity will probably make the user as happy as having done the activity in the past
            // When there is a huge amount of data, we focus on the fact that suggesting the activity will probably make the user as happy as suggesting the activity in the past

            double participationProbability = probabilityPrediction.Distribution.Mean;

            Prediction shortTerm_prediction = this.CombineRatingPredictions(activity.Get_ShortTerm_RatingEstimates(when));
            shortTerm_prediction.Justification.Label = "How much you'll enjoy doing this";
            double shortWeight = 1;
            shortTerm_prediction.Distribution = this.RatingAndProbability_Into_Value(shortTerm_prediction.Distribution, participationProbability, activity.MeanParticipationDuration).CopyAndReweightTo(shortWeight);
            SuggestionJustification shortTermJustification = new Composite_SuggestionJustification(shortTerm_prediction.Distribution, shortTerm_prediction.Justification, probabilityPrediction.Justification);
            shortTermJustification.Label = "Short-term happiness estimate";
            shortTerm_prediction.Justification = shortTermJustification;
            distributions.Add(shortTerm_prediction);

            // When predicting longterm happiness based on the DateTimes of suggestions of (or participations in) this Activity, there are two likely sources of error.
            // One likely source of error is that suggesting this Activity isn't actually what's changing the user's net present happiness. To check the plausibility that
            // suggesting this Activity is what's changing the net present happiness, we need to have lots of suggestions, and we increase the weight based on the number of suggestions.
            //
            // Another likely source of error is that the true net present happiness can't be perfectly computed until after we know how happy the user will be in the future
            // (because net present happiness is defined as the (exponentially) weighted sum of all future happinesses (with larger weights given to sooner ratings).
            // As more time elapses, we get an increasingly accurate estimate of the user's net present happiness at a given time. To account for this, we decrease the weight of
            // the prediction for activities we haven't known about for long
            TimeSpan existenceDuration = when.Subtract(activity.DiscoveryDate);
            double numCompletedHalfLives = existenceDuration.TotalSeconds / UserPreferences.DefaultPreferences.HalfLife.TotalSeconds;
            double activityExistenceWeightMultiplier = 1.0 - Math.Pow(0.5, numCompletedHalfLives);


            double mediumWeight = activity.NumConsiderations * participationProbability * activityExistenceWeightMultiplier * 160;
            Distribution ratingDistribution = activity.Predict_LongtermValue_If_Participated(when);
            Distribution mediumTerm_distribution = ratingDistribution.CopyAndReweightTo(mediumWeight);


            double longWeight = activity.NumConsiderations * (1 - participationProbability) * activityExistenceWeightMultiplier * 6;
            Distribution longTerm_distribution = activity.Predict_LongtermValue_If_Suggested(when).CopyAndReweightTo(longWeight);

            InterpolatorSuggestion_Justification mediumJustification = new InterpolatorSuggestion_Justification(
                activity, mediumTerm_distribution, activity.GetAverageLongtermValueWhenParticipated(), null);
            mediumJustification.Label = "Happiness after participated";
            distributions.Add(new Prediction(activity, mediumTerm_distribution, when, mediumJustification));

            InterpolatorSuggestion_Justification longtermJustification = new InterpolatorSuggestion_Justification(
                activity, longTerm_distribution, activity.GetAverageLongtermValueWhenSuggested(), null);
            longtermJustification.Label = "Happiness after suggested";
            distributions.Add(new Prediction(activity, longTerm_distribution, when, longtermJustification));

            // also include parent activities in the prediction if this activity is one that the user hasn't done often
            if (activity.NumConsiderations < 40)
            {
                foreach (Activity parent in activity.Parents)
                {
                    Distribution parentDistribution = parent.Predict_LongtermValue_If_Participated(when);
                    double parentWeight = Math.Min(parent.NumParticipations + 1, 40) * participationProbability * 3;
                    parentDistribution = parentDistribution.CopyAndReweightTo(parentWeight);
                    distributions.Add(new Prediction(activity, parentDistribution, when, "How happy you have been after doing " + parent.Name));
                }
            }

            return distributions;
        }

        private Prediction Get_OverallHappiness_SuggestionEstimate(Activity activity, DateTime when)
        {
            List<Prediction> predictions = this.Get_OverallHappiness_SuggestionEstimates(activity, when);
            Prediction prediction = this.CombineRatingPredictions(predictions);
            return prediction;
        }

        // returns a Prediction of what the user's efficiency will be after having participated in the given activity at the given time
        public Prediction Get_OverallEfficiency_ParticipationEstimate(Activity activity, DateTime when)
        {
            Distribution distribution = activity.Predict_LongtermEfficiency_If_Participated(when);
            return new Prediction(activity, distribution, when, "How efficient you tend to be after doing " + activity.Name);
        }

        public Distribution Get_AverageEfficiency_WhenParticipated(Activity activity)
        {
            return activity.GetAverageEfficiencyWhenParticipated();
        }

        // returns the average efficiency after having participated in <activity>
        // The purpose is that this will be used as a reference to compare the result of Get_Efficiency_ParticipationEstimate to
        public Distribution Get_AverageEfficiency_ParticipationEstimate(Activity activity)
        {
            return activity.GetAverageEfficiencyWhenParticipated();
        }

        public Distribution Predict_LongtermHappiness_From_LongtermEfficiency(Distribution efficiency)
        {
            AdaptiveLinearInterpolation.Distribution prediction = this.happinessFromEfficiency_predictor.Interpolate(new double[] { efficiency.Mean });
            return new Distribution(prediction);
        }
        // returns a bunch of thoughts telling why <activity> was last rated as it was
        public ActivitySuggestion_Justification JustifySuggestion(ActivitySuggestion activitySuggestion)
        {
            // first identify the most important reason for convenience
            Activity activity = this.ActivityDatabase.ResolveDescriptor(activitySuggestion.ActivityDescriptor);
            DateTime when = this.currentRecommendationsCache.ApplicableDate;
            List<Prediction> predictions = this.Get_OverallHappiness_SuggestionEstimates(activity, when);
            double lowestScore = 1;
            SuggestionJustification bestReason = null;
            foreach (Prediction candidate in predictions)
            {
                // make a list of all predictions except this one
                List<Prediction> predictionsMinusOne = new List<Prediction>(predictions);
                predictionsMinusOne.Remove(candidate);
                Prediction prediction = this.CombineRatingPredictions(predictionsMinusOne);
                Distribution scoreDistribution = prediction.Distribution;
                if ((scoreDistribution.Mean < lowestScore) || (bestReason == null))
                {
                    lowestScore = scoreDistribution.Mean;
                    bestReason = candidate.Justification;
                }
            }
            ActivitySuggestion_Justification justification = new ActivitySuggestion_Justification();
            justification.Suggestion = activitySuggestion;
            justification.PrimaryReason = bestReason;
            // also list all of the reasons for clarity
            List<SuggestionJustification> clauses = new List<SuggestionJustification>(predictions.Count);
            foreach (Prediction contributor in predictions)
            {
                clauses.Add(contributor.Justification);
            }
            justification.Reasons = clauses;
            return justification;
        }
        public Prediction CombineRatingPredictions(IEnumerable<Prediction> predictions)
        {
            List<Distribution> distributions = new List<Distribution>();
            DateTime date = new DateTime(0);
            Activity activity = null;
            List<SuggestionJustification> justifications = new List<SuggestionJustification>();
            foreach (Prediction prediction in predictions)
            {
                distributions.Add(prediction.Distribution);
                if (prediction.ApplicableDate.CompareTo(date) > 0)
                    date = prediction.ApplicableDate;
                activity = prediction.Activity;
                justifications.Add(prediction.Justification);
            }
            Distribution distribution = this.CombineRatingDistributions(distributions);

            Prediction result = new Prediction(activity, distribution, date, new Composite_SuggestionJustification(distribution, justifications));

            return result;
        }
        public Distribution CombineRatingDistributions(IEnumerable<Distribution> distributions)
        {
            // first add up all distributions that have standard deviation equal to zero
            Distribution sumOfZeroStdDevs = Distribution.Zero;
            bool stdDevIsZero = false;
            foreach (Distribution distribution in distributions)
            {
                if (distribution.StdDev == 0 && distribution.Weight != 0)
                {
                    stdDevIsZero = true;
                    sumOfZeroStdDevs = sumOfZeroStdDevs.Plus(distribution);
                }
            }
            // check whether there were any distributions with zero standard deviation
            if (stdDevIsZero)
            {
                // If there were any distributions with zero standard deviation, then return simply the sum of those distributions
                return sumOfZeroStdDevs;
            }
            // If we get here, then there are no distributions with zero standard deviation
            // So we can divide by any standard deviation without getting division by zero
            Distribution sum = Distribution.Zero;
            double totalWeight = 0;
            foreach (Distribution currentDistribution in distributions)
            {
                if (currentDistribution.Weight != 0)
                {
                    double weightFactor = (double)1 / currentDistribution.StdDev;
                    Distribution weightedDistribution = currentDistribution.CopyAndReweightBy(weightFactor);
                    sum = sum.Plus(weightedDistribution);
                    totalWeight += currentDistribution.Weight;
                }
            }
            sum = sum.CopyAndReweightTo(totalWeight);
            return sum;
        }
        public Prediction CombineProbabilityPredictions(List<Prediction> predictions)
        {
            if (predictions.Count == 1)
                return predictions[0];
            List<Distribution> distributions = new List<Distribution>();
            DateTime date = new DateTime(0);
            Activity activity = null;
            List<SuggestionJustification> justifications = new List<SuggestionJustification>();
            foreach (Prediction prediction in predictions)
            {
                distributions.Add(prediction.Distribution);
                if (prediction.ApplicableDate.CompareTo(date) > 0)
                    date = prediction.ApplicableDate;
                activity = prediction.Activity;
                justifications.Add(prediction.Justification);
            }
            Distribution distribution = this.CombineProbabilityDistributions(distributions);
            SuggestionJustification justification = new Composite_SuggestionJustification(distribution, justifications);
            justification.Label = "Overall participation probability";
            Prediction result = new Prediction(activity, distribution, date, justification);
            return result;
        }
        public Distribution CombineProbabilityDistributions(IEnumerable<Distribution> distributions)
        {
            return this.CombineRatingDistributions(distributions);
        }
        // checks the type of the rating and proceeds accordinly
        public void PutRatingInMemory(Rating newRating)
        {
            AbsoluteRating absolute = newRating as AbsoluteRating;
            if (absolute != null)
            {
                this.PutRatingInMemory(absolute);
                return;
            }
            RelativeRating relative = newRating as RelativeRating;
            if (relative != null)
            {
                this.PutRatingInMemory(relative);
                return;
            }
            // if we get here, don't know the type of the rating, so there's nothing we can do
        }
        // tells the Engine about a rating that wasn't already in memory (but may have been stored on disk)
        public void PutRatingInMemory(RelativeRating newRating)
        {
            // add the ratings in chronological order
            this.PutRatingInMemory(newRating.FirstRating);
            this.PutRatingInMemory(newRating.SecondRating);
        }
        // tells the Engine about a rating that wasn't already in memory (but may have been stored on disk)
        public void PutRatingInMemory(AbsoluteRating newRating)
        {
            // keep track of any unapplied ratings
            if (newRating.FromUser)
            {
                // keep track of the first and last date at which anything happened
                this.DiscoveredRating(newRating);
                this.unappliedRatings.Add(newRating);
                this.PutActivityDescriptorInMemory(newRating.ActivityDescriptor);
            }
            // keep track of how well the user has been spending time
            if (newRating.Source != null)
            {
                Participation participation = newRating.Source.ConvertedAsParticipation;
                if (participation != null)
                {
                    // exponential moving average with all necessary history
                    this.weightedRatingSummarizer.AddRating(participation.StartDate, participation.EndDate, newRating.Score);
                    if (newRating.FromUser)
                    {
                        // usual average of the activities that were not suggested
                        if (!participation.Suggested)
                            this.ratingsOfUnpromptedActivities = this.ratingsOfUnpromptedActivities.Plus(newRating.Score);
                    }
                }
            }
        }
        // tells each activity to spend a little bit of time updating its knowledge of the ratings that came after one of its participations
        public void Update_RatingSummaries(int numRatingsToUpdate)
        {
            if (numRatingsToUpdate > 0)
            {
                foreach (Activity activity in this.ActivityDatabase.AllActivities)
                {
                    activity.UpdateNext_RatingSummaries(numRatingsToUpdate);
                }
                this.happinessFromEfficiency_predictor.UpdateMany(numRatingsToUpdate);
            }
        }
        // gets called whenever any outside source provides a rating
        public void DiscoveredRating(AbsoluteRating newRating)
        {
            this.DiscoveredActionDate(newRating.Date);
        }
        public void DiscoveredSuggestion(ActivitySuggestion suggestion)
        {
            this.DiscoveredActionDate(suggestion.CreatedDate);
        }
        // Tells that we know something happened at the given date (so update the min/max dates accordingly
        public void DiscoveredActionDate(DateTime? when)
        {
            if (when != null)
            {
                this.DiscoveredActionDate(when.Value);
            }
        }
        public void DiscoveredActionDate(DateTime when)
        {
            if (when.CompareTo(this.firstInteractionDate) < 0)
            {
                this.firstInteractionDate = when;
            }
            if (when.CompareTo(this.latestInteractionDate) > 0)
            {
                this.latestInteractionDate = when;
            }
        }
        // provides a previously unknown participation to the Engine
        /*public void AddParticipation(Participation newParticipation)
        {
            // write it to the hard drive
            this.WriteParticipation(newParticipation);
            // adjust any global dates for having found it
            this.DiscoveredParticipation(newParticipation);
            // give it to any relevant Activities
            this.CascadeParticipation(newParticipation);
        }*/
        // tells the Engine about a participation that wasn't already in memory (but may have been stored on disk)
        public void PutParticipationInMemory(Participation newParticipation)
        {
            // keep track of an overall summary of the participations that have been entered
            if (newParticipation.Suggested)
                this.numPromptedParticipations++;
            else
                this.numUnpromptedParticipations++;
            // keep track of the first and last date at which anything happened
            this.DiscoveredParticipation(newParticipation);
            
            this.unappliedParticipations.Add(newParticipation);
            this.PutActivityDescriptorInMemory(newParticipation.ActivityDescriptor);

            this.weightedRatingSummarizer.AddParticipationIntensity(newParticipation.StartDate, newParticipation.EndDate, 1);

            
            Rating rating = newParticipation.GetCompleteRating();
            if (rating != null)
            {
                this.PutRatingInMemory(rating);
            }

            if (!newParticipation.Hypothetical)
            {
                this.updateExperimentsWithNewParticipation(newParticipation);
            }
        }

        // updates any existing experiments with information from newParticipation
        private void updateExperimentsWithNewParticipation(Participation newParticipation)
        {
            Activity activity = this.ActivityDatabase.ResolveDescriptor(newParticipation.ActivityDescriptor);
            if (this.experimentToUpdate.ContainsKey(activity))
            {
                string metricName = newParticipation.EffectivenessMeasurement.Metric.Name;
                Dictionary<string, PlannedExperiment> experimentsForThisActivity = this.experimentToUpdate[activity];
                if (experimentsForThisActivity.ContainsKey(metricName))
                {
                    PlannedExperiment experiment = experimentsForThisActivity[metricName];
                    if (experiment.InProgress)
                    {
                        // found the second participation in the experiment, so mark the experiment as complete
                        this.numCompletedExperiments++;
                    }
                    else
                    {
                        // found the first participation in the experiment; no longer have to update this experiment if this Activity gets completed
                        // (and we can add this Activity+Metric into a new Experiment)
                        experiment.FirstParticipation = newParticipation;
                        this.numStartedExperiments++;
                    }
                    this.experimentToUpdate[activity].Remove(metricName);
                }

            }
        }

        private void ActivityDatabase_InheritanceAdded(Inheritance inheritance)
        {
            // If we're already planning on recalculating everything then we don't need to do extra recalculations now
            if (!this.requiresFullUpdate)
            {
                Activity child = this.activityDatabase.ResolveDescriptor(inheritance.ChildDescriptor);
                if (child.Parents.Count > 1)
                {
                    // this was an existing activity that was given another parent, so we have to cascade ratings, participations, etc
                    this.requiresFullUpdate = true;
                }
                else
                {
                    // If this activity was recently added then we can compute a rating for it without recomputing everything else
                    this.EstimateRating(child, DateTime.Now);
                }
            }
        }


        // gets called whenever an outside source adds a participation
        public void DiscoveredParticipation(Participation newParticipation)
        {
            if (!newParticipation.Hypothetical)
            {
                DateTime when = newParticipation.StartDate;
                if (when.CompareTo(this.firstInteractionDate) < 0)
                {
                    this.firstInteractionDate = when;
                }
                when = newParticipation.EndDate;
                if (when.CompareTo(this.latestInteractionDate) > 0)
                {
                    this.latestInteractionDate = when;
                }
            }
        }
        public void PutInheritanceInMemory(Inheritance newInheritance)
        {
            this.ActivityDatabase.AddInheritance(newInheritance);
            this.requiresFullUpdate = true;
        }
        public void PutSkipInMemory(ActivitySkip newSkip)
        {
            this.unappliedSkips.Add(newSkip);
            this.numSkips++;

            this.DiscoveredActionDate(newSkip.CreationDate);

            if (newSkip.ConsideredSinceDate != null)
            {
                TimeSpan duration = newSkip.ThinkingTime;
                if (duration.TotalDays > 1)
                    System.Diagnostics.Debug.WriteLine("skip duration > 1 day, this is probably a mistake");
                // update our estimate of how longer the user spends thinking about what to do
                this.thinkingTime = this.thinkingTime.Plus(Distribution.MakeDistribution(duration.TotalSeconds, 0, 1));
                // record the fact that the user wasn't doing anything directly productive at this time
                this.weightedRatingSummarizer.AddParticipationIntensity(newSkip.ConsideredSinceDate, newSkip.ThinkingTime, 0);
            }
        }
        public void PutActivityRequestInMemory(ActivityRequest newRequest)
        {
            Rating newRating = newRequest.GetCompleteRating();
            AbsoluteRating convertedRating = newRating as AbsoluteRating;
            if (convertedRating != null)
                this.PutRatingInMemory(convertedRating);
        }
        // tells the Engine about an ActivitySuggestion that wasn't already in memory (but may have been stored on disk)
        public void PutSuggestionInMemory(ActivitySuggestion suggestion)
        {
            this.DiscoveredSuggestion(suggestion);
            this.unappliedSuggestions.Add(suggestion);
        }
        public void PutExperimentInMemory(PlannedExperiment experiment)
        {
            // save the experiment in a way where we can associate a participation in either activity back to it
            this.PutExperimentInMemory(experiment.Earlier, experiment);
            this.PutExperimentInMemory(experiment.Later, experiment);
        }
        private void PutExperimentInMemory(PlannedMetric metric, PlannedExperiment experiment)
        {
            Activity activity = this.ActivityDatabase.ResolveDescriptor(metric.ActivityDescriptor);
            if (!this.experimentToUpdate.ContainsKey(activity))
                this.experimentToUpdate.Add(activity, new Dictionary<string, PlannedExperiment>());
            this.experimentToUpdate[activity].Add(metric.MetricName, experiment);
        }
        // tells the Engine about an Activity that it may choose from
        public void PutActivityDescriptorInMemory(ActivityDescriptor newDescriptor)
        {
            this.ActivityDatabase.GetActivityOrCreateCategory(newDescriptor);
        }
        public void RemoveParticipation(Participation participationToRemove)
        {
            // apply any pending participations so that they will be removed too
            this.ApplyParticipationsAndRatings();

            // remove the participation from any relevant Activity
            ActivityDescriptor descriptor = participationToRemove.ActivityDescriptor;
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            if (activity != null)
            {
                List<Activity> superCategories = this.FindAllSupercategoriesOf(activity);
                foreach (Activity parent in superCategories)
                {
                    parent.RemoveParticipation(participationToRemove);
                }
            }
            this.weightedRatingSummarizer.RemoveParticipation(participationToRemove.StartDate);
        }
        public RelativeRating MakeRelativeRating(Participation mainParticipation, double scale, Participation otherParticipation)
        {
            // Create activities if missing
            if (this.requiresFullUpdate)
                this.FullUpdate();
            // Compute updated rating estimates for these activities
            Activity thisActivity = this.activityDatabase.ResolveDescriptor(mainParticipation.ActivityDescriptor);
            Activity otherActivity = this.activityDatabase.ResolveDescriptor(otherParticipation.ActivityDescriptor);
            DateTime when = mainParticipation.StartDate;
            this.EstimateSuggestionValue(thisActivity, when);
            this.EstimateSuggestionValue(otherActivity, when);
            this.MakeRecommendation(mainParticipation.StartDate);

            // make an AbsoluteRating for the Activity (leave out date + activity because it's implied)
            AbsoluteRating thisRating = new AbsoluteRating();
            Distribution thisPrediction = this.EstimateRating(thisActivity, mainParticipation.StartDate).Distribution;

            // make an AbsoluteRating for the other Activity (include date + activity because they're not implied)
            AbsoluteRating otherRating = new AbsoluteRating();
            otherRating.Date = otherParticipation.StartDate;
            otherRating.ActivityDescriptor = otherParticipation.ActivityDescriptor;
            Distribution otherPrediction = this.EstimateRating(otherActivity, otherParticipation.StartDate).Distribution;

            // now we compute updated scores for the new activities
            thisPrediction = thisPrediction.CopyAndReweightTo(1);
            otherPrediction = otherPrediction.CopyAndReweightTo(1);
            Distribution combinedDistribution = thisPrediction.Plus(otherPrediction);
            // Solve:
            // (thisActual - thisPredicted) * otherStddev = (otherPredicted - otherActual) * thisStddev
            // thisActual = otherActual * scale
            // So:
            // (otherActual * scale - thisPredicted) * otherStddev = (otherPredicted - otherActual) * thisStddev
            // otherActual * (scale * otherStddev + thisStddev) - thisPredicted  * otherStddev = otherPredicted * thisStddev
            // otherActual = (otherPredicted * thisStddev + thisPredicted * otherStddev) / (scale * otherStdddev + thisStddev)
            double numerator = (otherPrediction.Mean * thisPrediction.StdDev + thisPrediction.Mean * otherPrediction.StdDev);
            double otherDenominator = scale * otherPrediction.StdDev + thisPrediction.StdDev;
            // compute the better and worse scores
            double otherScore = numerator / otherDenominator;
            double thisScore = otherScore * scale;


            // clamp to no more than 1
            if (thisScore > 1)
            {
                thisScore = 1;
                otherScore = thisScore / scale;
            }
            if (otherScore > 1)
            {
                otherScore = 1;
                thisScore = otherScore * scale;
            }
            if (thisScore > 1 || thisScore < 0 || otherScore > 1 || otherScore < 0)
            {
                throw new Exception("Invalid scores: " + thisScore + " and " + otherScore);
            }

            thisRating.Score = thisScore;
            otherRating.Score = otherScore;

            RelativeRating rating = new RelativeRating();
            

            if (scale >= 1)
            {
                rating.BetterRating = thisRating;
                rating.WorseRating = otherRating;
            }
            else
            {
                rating.BetterRating = otherRating;
                rating.WorseRating = thisRating;
            }
            if (rating.BetterRating.Score < rating.WorseRating.Score)
            {
                throw new Exception("Internal error creating relative rating - attempted to assign lower score to WorseRating");
            }
            rating.RawScoreScale = scale;
            return rating;
        }

        public Rating MakeEstimatedRating(Participation sourceParticipation)
        {
            Activity activity = this.activityDatabase.ResolveDescriptor(sourceParticipation.ActivityDescriptor);
            if (activity.NumRatings < 1)
            {
                // skip activities for which we don't have much data
                return null;
            }
            Prediction prediction = this.EstimateRating(activity, sourceParticipation.StartDate);
            AbsoluteRating rating = new AbsoluteRating(prediction.Distribution.Mean, sourceParticipation.StartDate, sourceParticipation.ActivityDescriptor, RatingSource.FromParticipation(sourceParticipation));
            rating.FromUser = false;
            return rating;
        }

        public Distribution PredictEfficiency(Activity activity, DateTime when)
        {
            Distribution result = activity.PredictEfficiency(when);
            foreach (Activity parent in activity.ParentsUsedForPrediction)
            {
                Distribution parentPrediction = parent.PredictEfficiency(when);
                result = result.Plus(parentPrediction);
            }
            return result;
        }

        public RelativeEfficiencyMeasurement Make_CompletionEfficiencyMeasurement(Participation p)
        {
            Activity a = this.ActivityDatabase.ResolveDescriptor(p.ActivityDescriptor);
            PlannedExperiment experiment = this.findExperimentToUpdate(a, p.EffectivenessMeasurement.Metric.Name);
            if (experiment == null)
            {
                // can't estimate effectiveness without an experiment
                return null;
            }
            if (!experiment.InProgress)
            {
                // can't estimate effectiveness if the experiment isn't in progress
                return null;
            }
            if (!experiment.Later.ActivityDescriptor.Matches(a))
            {
                // If this is the second participation to happen but it wasn't the one that we asked to be second,
                // then the user did the participations out of order and we're not going to compute an efficiency measurement
                // Alternatively, should we return a score of 0 instead because that's how much progress the user made on the designated activities at
                // the designated times?
                return null;
            }

            // go get a lot of input variables for this effectiveness calculation
            Participation participation1 = experiment.FirstParticipation;
            Participation participation2 = p;
            double predictedDifficulty1 = 1.0 / experiment.Earlier.DifficultyEstimate.EstimatedSuccessesPerSecond;
            double predictedDifficulty2 = 1.0 / experiment.Later.DifficultyEstimate.EstimatedSuccessesPerSecond;
            double duration1 = participation1.Duration.TotalSeconds;
            double duration2 = participation2.Duration.TotalSeconds;
            double numSuccesses1 = participation1.CompletionFraction;
            double numSuccesses2 = participation2.CompletionFraction;
            if (numSuccesses1 == 0 && numSuccesses2 == 0)
            {
                // if neither participation was successful, then we don't have enough information to update our efficiency estimates
                // (updatedWeight1 below would be 0)
                return null;
            }
            Activity activity1 = this.ActivityDatabase.ResolveDescriptor(participation1.ActivityDescriptor);
            Activity activity2 = this.ActivityDatabase.ResolveDescriptor(participation2.ActivityDescriptor);
            this.EstimateRating(activity1, p.StartDate);
            this.EstimateRating(activity2, p.StartDate);
            double predictedEfficiency1 = this.PredictEfficiency(activity1, participation1.StartDate).Mean;
            System.Diagnostics.Debug.WriteLine("Predictioned efficiency = " + predictedEfficiency1 + " for " + activity1.Name + " at " + participation1.StartDate);
            // TODO: when predicting the efficiency of participation2, don't use lots of extra prediction factors, just use the timestamp
            // because although the first participation was chosen randomly, the second wasn't chosen from the same pool
            // (For example, the user might tend to prefer to work on easier tasks in the evening, and so if participation2.StartDate is in the evening,
            // then the user may have only consented to activity2 because the user thought activity2 sounded sufficiently easy.
            // If we use time of day (or any other predictor, really) as a prediction factor, then it might cause us to draw the (incorrect) conclusion
            // that that time of day results in more eficiency)
            double predictedEfficiency2 = this.PredictEfficiency(activity2, participation2.StartDate).Mean;
            System.Diagnostics.Debug.WriteLine("Predictioned efficiency = " + predictedEfficiency2 + " for " + activity2.Name + " at " + participation2.StartDate);
            System.Diagnostics.Debug.WriteLine("Making completion efficiency measurement for " + activity1.Name + " and " + activity2.Name);

            // now calculate efficiency
            System.Diagnostics.Debug.WriteLine("Duration1 = " + duration1);
            double weight1 = duration1 / predictedDifficulty1;
            System.Diagnostics.Debug.WriteLine("Duration2 = " + duration2);
            double weight2 = duration2 / predictedDifficulty2;
            double totalEffectiveness = weight1 * predictedEfficiency1 + weight2 * predictedEfficiency2;
            System.Diagnostics.Debug.WriteLine("NumSuccess1 = " + numSuccesses1);
            double successFraction1 = numSuccesses1 / (numSuccesses1 + numSuccesses2);
            System.Diagnostics.Debug.WriteLine("NumSuccess2 = " + numSuccesses2);
            double successFraction2 = numSuccesses2 / (numSuccesses1 + numSuccesses2);
            double updatedEfficiency1 = totalEffectiveness * successFraction1 / weight1;
            double updatedEfficiency2 = totalEffectiveness * successFraction2 / weight2;
            System.Diagnostics.Debug.WriteLine("updatedEfficiency1 = " + updatedEfficiency1);
            double updatedWeight1 = numSuccesses1 + numSuccesses2;
            System.Diagnostics.Debug.WriteLine("updatedEfficiency2 = " + updatedEfficiency2);
            double updatedWeight2 = updatedWeight1;
            // Why the efficiencies are computed this way:
            // Note that weight1*updatedEfficiency1+weight2*updatedEfficiency2=totalProductivity=weight1*predictedEfficiency1+weight2*predictedEfficiency2, so (weighted) average estimated efficiency is conserved
            //  Note that numSuccesses1 is 0 or 1, and numSucesses2 is 0 or 1
            // Note that doubling predictedDifficulty[[1,2]] doesn't change updatedEfficiency[[1,2]]
            //  In other words, having poor difficulty estimates doesn't bias the efficiency measurements (although using poor difficulty estimates does add variance to the efficiency measurements)
            // Note that spending longer on duration1 than on duration2 makes weight1 larger than weight2, which makes updatedEfficiency2 larger than updatedEfficiency1
            // Note that doubling duration[i] and predictedDifficulty[i] leaves the efficiency estimates unchanged
            // Note that if numSuccesses1 == 0, then updatedEfficiency2 = totalProductivity/weight2 = predictedEfficiency1*weight1/weight2 + predictedEfficiency2, which is more than predictedEfficiency2
            // Note that if this process is run twice, once with two successes and once with numSuccesses1=0,numSuccesses2=1, then assuming everything else is identical:
            //  The first time, updatedEfficiency1=updatedEfficiency2=1, updatedWeight1=updatedWeight2=2
            //  The second time, updatedEfficiency1=0, updatedEfficiency2=2, updatedWeight1=updatedWeight2=1
            //  average(updatedEfficiency1)=2/3, average(updatedEfficiency2)=4/3 = 2*average(updatedEfficiency1) as desired

            // lastly, assemble the results
            RelativeEfficiencyMeasurement measurement1 = new RelativeEfficiencyMeasurement(participation1, Distribution.MakeDistribution(updatedEfficiency1, 0, updatedWeight1));
            RelativeEfficiencyMeasurement measurement2 = new RelativeEfficiencyMeasurement(participation2, Distribution.MakeDistribution(updatedEfficiency2, 0, updatedWeight2));
            measurement1.Later = measurement2;
            measurement2.Earlier = measurement1;

            return measurement2;
        }
        private double computeSuccessesPerSecond(Participation participation)
        {
            if (participation.CompletedMetric)
            {
                double totalSeconds = participation.Duration.TotalSeconds;
                if (totalSeconds > 0)
                    return 1.0 / totalSeconds;
            }
            return 0;
        }


        // Tells whether ChooseExperimentOption will return an error
        // If ChooseExperimentOption is not available right now, returns a string explaining why. Otherwise returns ""
        public SuggestedMetric_Metadata Test_ChooseExperimentOption()
        {
            return this.ChooseExperimentOption(new ActivityRequest(), new List<SuggestedMetric>(0), null, new DateTime(), true);
        }

        private double convert_numCompletedExperimentParticipations_to_minNumPendingPostTasks(int numCompletedExperimentalParticipations)
        {
            // Motivation #1 (calculating the minimum required number of post-tasks in the pending-task pool)
            //  We'd like it to be possible to graph the log of the user's efficiency over time, and for any errors in the graph introduced by randomness to converge to zero,
            //   or at least definitely not diverge
            //  The way that efficiency will be computed will be by comparing the completion duration of both tasks in an experiment
            //  The source of randomness is that the two tasks are ordered randomly but might have significantly different difficulties
            //  So, we need to have the pool of tasks grow increasingly large so we can compare increasingly distant activities and hopefully cause error to converge to zero
            // The situation:
            //  Suppose that the user has completed T tasks.
            //  Suppose that the number of tasks in the pool equals P(T)
            // Let us slightly modify this problem for the purpose of more easily modelling it:
            //  Let us pretend that tasks are separated into bins, each of size P(T)
            //  Let us pretend that all pairs of tasks are between two adjacent bins, and that they are evently distributed.
            //   That is, let us pretend that there are (P(T)/2) pairs of tasks that have one task in bin i and one task in bin i+1
            // The analysis:
            //  We want to compute the standard deviation (stddev) of the log of the ratio of the efficiency at the beginning of time and the efficiency at the end of time
            //   This is equivalent to computing the sqrt of the variance of the log of the ratio of the computed efficiency at the beginning and the computed efficiency at the end
            //    This is equivalent to computing the sqrt of the sum of the variances of the logs of the ratios between adjacent bins
            //   The stddev of the ratios between adjacent bins (when there are T tasks, and when the stddev of measurement error equals m) is:
            //    m / sqrt(P(T)/2)
            //   This gives a variance of the ratios between adjacent bins of:
            //    2m*m / P(T)
            //   because log(X) approximately equals X - 1 for X near 1, the variance of the log of the ratios is approximately the same
            //   Then the sum of the logs of the variances equals:
            //    sum((probability that T is at the boundary between two bins) * (log of the variance of the ratio P(T+1)/P(T))
            //    = sum((1/P(T))*(2m*m/P(T)))
            //    = 2m * m * sum(1/(P(T)^2))
            //     This diverges for P(T) = T^(1/2) but converges for P(T) = T^(d) for d > 1/2
            //      What does it converge to? sum(2m*m*sum(T^(-2d))) = (-2m*m/(-2d+1))*(T^(-2d+1))[1,infinity) = 2m*m/(2d-1)
            //  (Also, recall that we need d <= 1 to collect any data at all (if the current pool contains all tasks ever, then we'll never complete an experiment))
            //  We need d >= 0.5 for convergence, and the larger it is, the lower the sum we converge to.
            //  Suppose d = 2/3.
            //   Then the total variance = 2m*m/(2d-1) = 6m*m, so the total stddev = sqrt(6) * m = about 2.5m
            //   Then P(8) = 8^(2/3) = 4, so half of tasks would be in the pool after 8 tasks were completed
            //   Then P(27) = 27^(2/3) = 9, so one third of tasks would be in the pool after 27 tasks were completed
            //   Then P(365) = 365^(2/3) = roughly 49, so about one seventh of tasks would be in the pool after doing one task per day for a year
            return Math.Pow(numCompletedExperimentalParticipations, 0.667);
        }

        private int convert_numCompletedExperimentParticipations_to_minNumPendingTasks(int numCompletedExperimentalParticipations)
        {
            double min_pendingPostTasks_pool_size = this.convert_numCompletedExperimentParticipations_to_minNumPendingPostTasks(numCompletedExperimentalParticipations);
            // Motivation #2 (calculating the minimum number of tasks in the pending-task pool)
            //  Note that:
            //   If we want to keep the size of the pending-task pool above some number (as described above), then:
            //    When the size of the pending-task pool reaches its minimum target size,
            //     We must be always choose to increase it
            //      Increasing the pending-task pool size is equivalent to using only unpaired tasks in the next experiment
            //  We don't want the user to reach a state where the number of possible tasks that can be suggested for an experiment becomes small
            //   because the user won't realize how many tasks are available for suggesting
            //    and the user might try for a while to get some more experiment suggestions without knowing that the Engine will just repeatedly make the same suggestions
            //     caused by the small number of unpaired tasks
            //  So, we want the number of available tasks to be larger than the minimum target size of the pending-post-task pool
            //   For now, let's require that the number of pending tasks is at least double the minimum target number of pending post-tasks
            int minTotalPoolSize = (int)Math.Max(min_pendingPostTasks_pool_size * 2, 3); // need at least 3 tasks available to even start an experiment
            return minTotalPoolSize;
        }

        // Does a binary search to figure out how many more experiment participations can happen before we need more tasks
        public int ComputeNumRemainingExperimentParticipations(int numCompletedExperimentParticipations, int numPendingTasks)
        {
            int guess = 0;
            int step = 1;
            bool increasePrev = true;
            int bestResult = -1;
            bool everFlipped = false;
            while (true)
            {
                int requiredNumPendingTasks = this.convert_numCompletedExperimentParticipations_to_minNumPendingTasks(numCompletedExperimentParticipations + guess);
                int expectedNumPendingTasks = numPendingTasks - guess;
                bool increaseNow = (requiredNumPendingTasks <= expectedNumPendingTasks);
                if (increaseNow)
                {
                    bestResult = guess;
                    guess += step;
                }
                else
                {
                    guess -= step;
                }
                if (step < 1)
                    break;
                if (increaseNow == increasePrev)
                {
                    if (!everFlipped)
                        step *= 2;
                }
                else
                {
                    step /= 2;
                    everFlipped = true;
                }
                increasePrev = increaseNow;
            }
            
            // we add 1 because experiments are permitted when exactly on the threshold (at which point the number of participations will be 1 more)
            return bestResult + 1;
        }

        // given a list of options of activities that the user is considering doing; returns a new option (different than the others) that can be added to the list
        public SuggestedMetric_Metadata ChooseExperimentOption(ActivityRequest activityRequest, List<SuggestedMetric> existingOptions, TimeSpan? requestedProcessingTime, DateTime when, bool dryRun = false)
        {
            // activities that can ever be put in an experiment
            List<Activity> activitiesHavingMetrics = this.SuggestibleActivitiesHavingIntrinsicMetrics;

            // activities that are already listed as options can't be re-added as new options
            HashSet<Activity> excludedActivities = new HashSet<Activity>();
            foreach (SuggestedMetric existingOption in existingOptions)
            {
                excludedActivities.Add(this.ActivityDatabase.ResolveDescriptor(existingOption.ActivityDescriptor));
            }
            List<Activity> availablePreActivities = new List<Activity>();
            List<Activity> availablePostActivities = new List<Activity>();

            // the Activity that the user has requested this suggestion come from
            Activity fromCategory = this.activityDatabase.ResolveDescriptor(activityRequest.FromCategory);

            // find available activities and classify each as a pre-Activity (not yet part of a planned Experiment) or post-Activity (already is the second task in a planned Experiment)
            foreach (Activity activity in activitiesHavingMetrics)
            {
                if (!excludedActivities.Contains(activity))
                {
                    if (fromCategory == null || this.activityDatabase.HasAncestor(activity, fromCategory))
                    {
                        // Now we determine whether to plan for this activity to be a pre activity or a post activity.
                        // We don't want to require the user to measure their participation in any specific way because if we did require them to measure it in a certain way then
                        // they would know that this activity was being treated as a post activity.
                        // So, (assuming that the user created multiple metrics), because it is always possible for the user to change the metric, if any of the metrics are paired, then
                        // we prepare for the possibility that the user will change the metric to one that has already been paired.
                        // Consequently, we treat these activities as post-tasks.
                        // However, if the user already has lots of pending post-tasks and chooses another unchosen metric, then this could result in the user having to wait longer than expected before
                        // being able to measure their efficiency. However, it should be unlikely and mostly harmless for a user to repeatedly change metrics and get too many pre-tasks, so we ignore
                        // it for now.
                        bool canBePostMetric = false;
                        foreach (Metric m in activity.AllMetrics)
                        {
                            // If we get into this ChooseExperimentOption function, then there exists no experiment that has been planned but unstarted.
                            // So, we can check whether this is a post-task by checking whether it is participating in any experiments
                            if (this.wouldCompletionAffectExperiment(activity, m.Name))
                            {
                                canBePostMetric = true;
                                break;
                            }
                        }
                        if (canBePostMetric)
                            availablePostActivities.Add(activity);
                        else
                            availablePreActivities.Add(activity);
                    }
                }
            }

            // check ActivityToBeat if specified
            Activity activityToBeat = this.activityDatabase.ResolveDescriptor(activityRequest.ActivityToBeat);
            if (activityToBeat != null)
            {
                if (!availablePreActivities.Contains(activityToBeat) && !availablePostActivities.Contains(activityToBeat))
                    return new SuggestedMetric_Metadata(activityToBeat.Name + " cannot be added to this experiment");
            }


            int numActivitiesToChooseFromNow = availablePreActivities.Count + availablePostActivities.Count;

            // We also want the pre/post status of each item in the pool to be as unpredictable to the user as possible,
            // to prevent them from subconsciously working harder on tasks that are known to be post tasks.
            // So, we randomly decide whether to add suggest a pre-task or a post-task, and we weight the suggestion based on how are already in the pool
            bool considerPreActivities;
            bool considerPostActivities;
            if (activityRequest.FromCategory != null || activityRequest.ActivityToBeat != null)
            {
                // If the caller passed in an ActivityRequest, then they've specified a specific preference for some activities
                // We don't wan't to make it easy for the user to accidentially figure out which tasks are unpaired and which tasks are post tasks
                // (because that could motivate the user to work less hard on pre-tasks)
                // We also don't want to confuse the user by inexplicably excluding certain tasks
                // So, if the user passed in an ActivityRequest, then we allow both unpaired and post activities
                // (However, we only allow the user to specify an ActivityRequest occasionally, to prevent the size of the post tasks from getting too small)
                considerPreActivities = considerPostActivities = true;

                if (numActivitiesToChooseFromNow < 1)
                    return new SuggestedMetric_Metadata("No matching activities!");
            }
            else
            {
                // check that we have enough pending tasks to choose from
                int min_pendingPostTasks_pool_size = (int)this.convert_numCompletedExperimentParticipations_to_minNumPendingPostTasks(this.NumCompletedExperimentParticipations);
                int minTotalPoolSize = this.convert_numCompletedExperimentParticipations_to_minNumPendingTasks(this.NumCompletedExperimentParticipations);
                int numActivitiesToChooseFromTotal = numActivitiesToChooseFromNow + existingOptions.Count;

                if (numActivitiesToChooseFromTotal < minTotalPoolSize)
                {
                    int numExtraRequiredActivities = minTotalPoolSize - numActivitiesToChooseFromTotal;
                    string message = "Don't have enough activities having metrics to run another experiment. Go create " + numExtraRequiredActivities + " more ";
                    if (numExtraRequiredActivities == 1)
                        message += "activity of type ToDo and/or add " + numExtraRequiredActivities + " Metric to another Activity!";
                    else
                        message += "activities of type ToDo and/or add Metrics to " + numExtraRequiredActivities + " more Activities!";
                    // no enough activities for a meaningful experiment
                    return new SuggestedMetric_Metadata(message);
                }

                if (availablePostActivities.Count < min_pendingPostTasks_pool_size && availablePreActivities.Count > 0)
                {
                    // not enough tasks in the pool; we're at risk of having too few overlapping comparisons
                    considerPostActivities = false;
                }
                else
                {
                    if (availablePostActivities.Count > minTotalPoolSize)
                    {
                        // pool is more than big enough; we should start collecting data (completing experiments)
                        considerPostActivities = true;
                    }
                    else
                    {
                        int postWeight = availablePostActivities.Count - min_pendingPostTasks_pool_size;
                        int preWeight = minTotalPoolSize - availablePostActivities.Count;
                        considerPostActivities = (this.randomGenerator.Next(preWeight + postWeight) >= preWeight);
                    }
                }
                considerPreActivities = !considerPostActivities;

                if (dryRun)
                {
                    // The caller was only looking for some stats; return stats
                    int maxNumRemainingParticipations = this.ComputeNumRemainingExperimentParticipations(this.NumCompletedExperimentParticipations, numActivitiesToChooseFromTotal);
                    return new SuggestedMetric_Metadata(null, maxNumRemainingParticipations);
                }

            }


            List<Activity> recommendableActivities;
            if (considerPreActivities)
            {
                if (considerPostActivities)
                {
                    List<Activity> choices = availablePreActivities;
                    foreach (Activity activity in availablePostActivities)
                    {
                        choices.Add(activity);
                    }
                    recommendableActivities = choices;
                }
                else
                {
                    recommendableActivities = availablePreActivities;
                }
            }
            else
            {
                recommendableActivities = availablePostActivities;
            }


            // Now that we've identified some activities that are reasonable to add to the experiment, choose the one that we think will provide the most longterm value to the user
            ActivityRequest request = new ActivityRequest();
            request.LeafActivitiesToConsider = recommendableActivities;
            if (activityToBeat != null)
                request.ActivityToBeat = activityToBeat.MakeDescriptor();
            request.RequestedProcessingTime = requestedProcessingTime;

            ActivitySuggestion activitySuggestion = this.MakeRecommendation(request);
            Activity bestActivity = this.activityDatabase.ResolveDescriptor(activitySuggestion.ActivityDescriptor);

            // chose a random metric for this activity
            List<Metric> metrics = new List<Metric>();
            foreach (Metric metricOption in bestActivity.AllMetrics)
            {
                bool isPostMetric = this.wouldCompletionAffectExperiment(bestActivity, metricOption.Name);
                if (isPostMetric)
                {
                    if (considerPostActivities)
                        metrics.Add(metricOption);
                }
                else
                {
                    if (considerPreActivities)
                        metrics.Add(metricOption);
                }
            }
            if (metrics.Count < 1)
                throw new Exception("Internal error while planning experiment: no valid metric found for " + bestActivity);
            Metric metric = metrics[this.randomGenerator.Next(metrics.Count)];

            PlannedMetric plannedMetric = new PlannedMetric();
            plannedMetric.ActivityDescriptor = bestActivity.MakeDescriptor();
            plannedMetric.MetricName = metric.Name;

            bool userGuidedThisSuggestion = fromCategory != null;
            return new SuggestedMetric_Metadata(new SuggestedMetric(plannedMetric, this.SuggestActivity(bestActivity, when), userGuidedThisSuggestion), -1);
        }

        // Generates a set of experiment options that together form a possible experiment
        // This is convenient for unit tests
        // The user interface will generally call ChooseExperimentOption instead to choose them one at a time
        public List<SuggestedMetric> ChooseExperimentOptions(DateTime when)
        {
            List<SuggestedMetric> choices = new List<SuggestedMetric>();
            ActivityRequest request = new ActivityRequest(this.activityDatabase.RootActivity.MakeDescriptor(), null, when);
            for (int i = 0; i < 3; i++)
            {
                SuggestedMetric_Metadata metadata = this.ChooseExperimentOption(request, choices, null, when);
                choices.Add(metadata.Content);
            }
            return choices;
        }

        // returns number of successes per second
        public double EstimateDifficulty_WithoutUser(Activity activity)
        {
            double typicalParticipationDuration = 3600; // default to 1 hour
            Activity activityToComputeMeanParticipationDuration = activity;
            if (activity.NumParticipations < 4 && activity.Parents.Count > 0)
                activityToComputeMeanParticipationDuration = activity.Parents[0];
            typicalParticipationDuration = activityToComputeMeanParticipationDuration.MeanParticipationDuration;
            DifficultyEstimate difficultyEstimate = new DifficultyEstimate();
            return 1.0 / typicalParticipationDuration;
        }

        // assigns estimated number of successes per second without using the user's estimates
        public void ignoreUserDifficultyEstimates(PlannedMetric a, PlannedMetric b)
        {
            a.DifficultyEstimate.EstimatedSuccessesPerSecond = a.DifficultyEstimate.EstimatedSuccessesPerSecond_WithoutUser;
            b.DifficultyEstimate.EstimatedSuccessesPerSecond = b.DifficultyEstimate.EstimatedSuccessesPerSecond_WithoutUser;
        }
        // computes estimated number of successes per second based on both past history and user's estimates
        public void incorporateUserDifficultyEstimates(PlannedMetric a, PlannedMetric b)
        {
            // estimate overall expected difficulty
            double totalEstimatedNumSuccessesPerSecond = a.DifficultyEstimate.EstimatedSuccessesPerSecond_WithoutUser +
                b.DifficultyEstimate.EstimatedSuccessesPerSecond_WithoutUser;
            int difficultyDifference = b.DifficultyEstimate.NumHarders - a.DifficultyEstimate.NumHarders;
            int absDifficultyDifference = Math.Abs(difficultyDifference);
            double maxDiffIndices = 3;
            double easierWeight, harderWeight;
            if (absDifficultyDifference == 0)
            {
                easierWeight = harderWeight = 0.5;
            }
            else
            {
                if (absDifficultyDifference > maxDiffIndices)
                {
                    throw new InvalidOperationException("Unsupported difference (" + difficultyDifference + ") , must be from -" + maxDiffIndices + " to " + maxDiffIndices + " (inclusive)");
                }
                double maxMultiplier = 4;
                double minEasierWeight = Math.Pow(maxMultiplier, (double)(absDifficultyDifference - 1) / maxDiffIndices);
                double maxEasierWeight = Math.Pow(maxMultiplier, (double)absDifficultyDifference / maxDiffIndices);
                easierWeight = (minEasierWeight + maxEasierWeight) / 2;
                harderWeight = 1;

                // renormalize
                easierWeight = easierWeight / (easierWeight + harderWeight);
                harderWeight = 1 - easierWeight;
            }

            // swap if backwards
            if (a.DifficultyEstimate.NumHarders < b.DifficultyEstimate.NumHarders)
            {
                PlannedMetric temp = a;
                a = b;
                b = temp;
            }
            // compute updated difficulty 
            a.DifficultyEstimate.EstimatedSuccessesPerSecond = totalEstimatedNumSuccessesPerSecond * easierWeight;
            b.DifficultyEstimate.EstimatedSuccessesPerSecond = totalEstimatedNumSuccessesPerSecond * harderWeight;
        }
        public ExperimentSuggestion Experiment(List<SuggestedMetric> choices, DateTime when)
        {
            // separate into pre-activities and post-activities
            List<SuggestedMetric> unpairedActivityMetrics = new List<SuggestedMetric>();
            List<SuggestedMetric> pairedActivityMetrics = new List<SuggestedMetric>();

            foreach (SuggestedMetric suggestion in choices)
            {
                // If we get into this Experiment function, then there exists no experiment that has been planned but unstarted.
                // So, we can check whether this is a post-task by checking whether it is participating in any experiments
                if (this.wouldCompletionAffectExperiment(this.ActivityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor), suggestion.PlannedMetric.MetricName))
                    pairedActivityMetrics.Add(suggestion);
                else
                    unpairedActivityMetrics.Add(suggestion);
            }
            // to start a new experiment we need at least 2 available activities
            if (unpairedActivityMetrics.Count < 2)
                unpairedActivityMetrics.Clear();

            // Choose whether to start a new experiment or finish an existing experiment.
            // We've gone to great lengths to hide from the user the information about whether we're finishing an existing experiment or starting a new experiment because
            // if the user discovers which one this is, it might subconsciously motivate them to work harder on the post tasks and less hard on the pre tasks, artificially skewing the results
            int suggestionIndex = this.randomGenerator.Next(unpairedActivityMetrics.Count + pairedActivityMetrics.Count);

            bool choosePreActivity = (suggestionIndex < unpairedActivityMetrics.Count);
            if (choosePreActivity)
            {
                // We're choosing a pre-activity. Pair it with another unpaired activity
                int suggestion2Index = this.randomGenerator.Next(unpairedActivityMetrics.Count - 1);
                if (suggestion2Index == suggestionIndex)
                    suggestion2Index = unpairedActivityMetrics.Count - 1;

                PlannedExperiment experiment = this.PlanExperiment(unpairedActivityMetrics[suggestionIndex], unpairedActivityMetrics[suggestion2Index]);

                ActivitySuggestion activitySuggestion = unpairedActivityMetrics[suggestionIndex].ActivitySuggestion;
                activitySuggestion.Skippable = false;
                return new ExperimentSuggestion(experiment, activitySuggestion);
            }
            else
            {
                // We're choosing a post-activity. Find the experiment we previously created for it
                SuggestedMetric experimentSuggestion = pairedActivityMetrics[suggestionIndex - unpairedActivityMetrics.Count];
                Activity laterActivity = this.ActivityDatabase.ResolveDescriptor(experimentSuggestion.ActivityDescriptor);
                PlannedExperiment experiment = this.findExperimentToUpdate(laterActivity, experimentSuggestion.PlannedMetric.MetricName);
                ActivitySuggestion activitySuggestion = experimentSuggestion.ActivitySuggestion;
                activitySuggestion.Skippable = false;
                return new ExperimentSuggestion(experiment, activitySuggestion);
            }
        }

        // Given two SuggestedMetric objects which suggest doing certain activities, returns a PlannedExperiment that estimates the difficulty of each
        public PlannedExperiment PlanExperiment(SuggestedMetric a, SuggestedMetric b)
        {
            PlannedExperiment experiment = new PlannedExperiment();
            experiment.Earlier = a.PlannedMetric;
            experiment.Later = b.PlannedMetric;

            this.ReplanExperiment(experiment);

            return experiment;
        }

        // Recomputes estimated difficulties for the given experiment
        public void ReplanExperiment(PlannedExperiment experiment)
        {
            experiment.Earlier.DifficultyEstimate.EstimatedSuccessesPerSecond_WithoutUser = this.EstimateDifficulty_WithoutUser(this.ActivityDatabase.ResolveDescriptor(experiment.Earlier.ActivityDescriptor));
            experiment.Later.DifficultyEstimate.EstimatedSuccessesPerSecond_WithoutUser = this.EstimateDifficulty_WithoutUser(this.ActivityDatabase.ResolveDescriptor(experiment.Later.ActivityDescriptor));

            if (this.activityDatabase.ResolveDescriptor(experiment.Earlier.ActivityDescriptor).NumParticipations <= 2 &&
                this.activityDatabase.ResolveDescriptor(experiment.Later.ActivityDescriptor).NumParticipations <= 2 &&
                (experiment.Earlier.DifficultyEstimate.NumEasiers > 0 || experiment.Earlier.DifficultyEstimate.NumHarders > 0))
            {
                // If neither activity has been done many times before, then make the difficulty estimates be close to each other but slightly offset based on what the user guesses
                this.incorporateUserDifficultyEstimates(experiment.Earlier, experiment.Later);
            }
            else
            {
                // If either activity has been done often, then just estimate the difficulty directly based on what has historically been observed
                this.ignoreUserDifficultyEstimates(experiment.Earlier, experiment.Later);
            }
        }

        // for a given Activity, returns the Experiment that will need updating if the user participates in Activity
        private PlannedExperiment findExperimentToUpdate(Activity activity, string metricName)
        {
            if (this.experimentToUpdate.ContainsKey(activity))
            {
                Dictionary<string, PlannedExperiment> experimentsByMetricName = this.experimentToUpdate[activity];
                if (experimentsByMetricName.ContainsKey(metricName))
                    return experimentsByMetricName[metricName];
            }
            return null;
        }
        // tells whether completing this Activity would affect an experiment
        private bool wouldCompletionAffectExperiment(Activity activity, string metricName)
        {
            return (this.findExperimentToUpdate(activity, metricName) != null);
        }

        private int NumCompletedExperiments
        {
            get
            {
                return this.numCompletedExperiments;
            }
        }

        private int NumCompletedExperimentParticipations
        {
            get
            {
                return this.NumCompletedExperiments + this.numStartedExperiments;
            }
        }
        // returns a list of Activities that are available to be added into a new Experiment
        private List<Activity> SuggestibleActivitiesHavingIntrinsicMetrics
        {
            get
            {
                // find each Activity that has a metric
                List<Activity> results = new List<Activity>();
                foreach (Activity activity in this.activityDatabase.AllActivities)
                {
                    if (activity.Suggestible && activity.IntrinsicMetrics.Count > 0)
                        results.Add(activity);
                }
                return results;
            }
        }
        
        public List<Activity> ActivitiesSortedByAverageRating
        {
            get
            {
                List<Activity> activities = new List<Activity>(this.ActivityDatabase.AllActivities);
                // it takes more time to also analyze the ratings for the root activity, but it's probably not very helpful to do
                activities.Remove(this.ActivityDatabase.RootActivity);
                activities.Sort(new ActivityAverageScoreComparer());
                activities.Reverse();
                return activities;
            }
        }

        public DateTime LatestInteractionDate
        {
            get
            {
                return this.latestInteractionDate;
            }
        }
        public ActivityDatabase ActivityDatabase
        {
            get
            {
                return this.activityDatabase;
            }
        }
        public ScoreSummarizer RatingSummarizer
        {
            get
            {
                return this.weightedRatingSummarizer;
            }
        }
        public ScoreSummarizer EfficiencySummarizer
        {
            get
            {
                return this.efficiencySummarizer;
            }
        }
        private UserPreferences Get_UserPreferences()
        {
            return UserPreferences.DefaultPreferences;
        }

        public Random Randomness
        {
            set
            {
                this.randomGenerator = value;
            }
        }
        private ActivityRecommendationsAnalysis cacheForDate(DateTime when)
        {
            if (this.currentRecommendationsCache == null || !when.Equals(this.currentRecommendationsCache.ApplicableDate))
                this.currentRecommendationsCache = new ActivityRecommendationsAnalysis(when);
            return this.currentRecommendationsCache;
        }

        private ActivityDatabase activityDatabase;                  // stores all Activities
        private List<AbsoluteRating> unappliedRatings;              // lists all Ratings that the RatingProgressions don't know about yet
        private List<Participation> unappliedParticipations;        // lists all Participations that the ParticipationProgressions don't know about yet
        private List<ActivitySkip> unappliedSkips;                  // lists all skips that the progressions don't know about yet
        private List<ActivitySuggestion> unappliedSuggestions;            // lists all ActivitySuggestions that the Activities don't know about yet
        DateTime firstInteractionDate;
        DateTime latestInteractionDate;
        bool requiresFullUpdate = true;
        Distribution thinkingTime;      // how long the user spends before skipping a suggestion
        ScoreSummarizer weightedRatingSummarizer;
        ScoreSummarizer efficiencySummarizer;
        
        
        Distribution ratingsOfUnpromptedActivities;
        int numSkips;
        int numPromptedParticipations;
        int numUnpromptedParticipations;
        private Random randomGenerator = new Random();

        // for each of several activities and metrics, tells which PlannedExperiment that needs updating if the user participates in that activity
        Dictionary<Activity, Dictionary<string, PlannedExperiment>> experimentToUpdate = new Dictionary<Activity, Dictionary<string, PlannedExperiment>>();
        int numCompletedExperiments;
        int numStartedExperiments;
        ActivityRecommendationsAnalysis currentRecommendationsCache;

        LongtermValuePredictor happinessFromEfficiency_predictor;

    }
}
