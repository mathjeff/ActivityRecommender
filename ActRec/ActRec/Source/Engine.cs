using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;

namespace ActivityRecommendation
{
    // The Engine class analyzes information in various Activity objects and can, for example, make recommendations
    // The Engine class doesn't have any knowledge of the user interface or about persisting any data across application restarts; that's handled by ActivityRecommender
    class Engine
    {
        public Engine()
        {
            this.weightedRatingSummarizer = new ExponentialRatingSummarizer(UserPreferences.DefaultPreferences.HalfLife);
            this.activityDatabase = new ActivityDatabase(this.weightedRatingSummarizer);
            this.activityDatabase.ActivityAdded += ActivityDatabase_ActivityAdded;
            this.activityDatabase.InheritanceAdded += ActivityDatabase_InheritanceAdded;
            this.unappliedRatings = new List<AbsoluteRating>();
            this.unappliedParticipations = new List<Participation>();
            this.unappliedSkips = new List<ActivitySkip>();
            this.unappliedSuggestions = new List<ActivitySuggestion>();
            this.allActivityDescriptors = new List<ActivityDescriptor>();
            this.inheritances = new List<Inheritance>();
            this.firstInteractionDate = DateTime.Now;
            this.latestInteractionDate = new DateTime(0);
            this.thinkingTime = Distribution.MakeDistribution(60, 0, 1);      // default amount of time thinking about a suggestion is 1 minute
            this.ratingsOfUnpromptedActivities = new Distribution();

        }

        // gives to the necessary objects the data that we've read. Optimized for when there are large quantities of data to give to the different objects
        public void FullUpdate()
        {
            this.CreateNewActivities();

            this.ApplyInheritances();


            this.ApplyParticipationsAndRatings();
            this.requiresFullUpdate = false;
        }
        public void EnsureRatingsAreAssociated()
        {
            if (this.requiresFullUpdate)
                this.FullUpdate();
            else
                this.ApplyParticipationsAndRatings();
        }
        // creates an Activity object for each ActivityDescriptor that needs one
        public void CreateNewActivities()
        {
            // first, create the necessary Activities
            foreach (ActivityDescriptor descriptor in this.allActivityDescriptors)
            {
                this.activityDatabase.GetActivityOrCreateCategory(descriptor);
            }
            this.allActivityDescriptors.Clear();
        }
        // connects up all of the parent and child pointers
        public void ApplyInheritances()
        {
            // next, add the necessary parent pointers
            foreach (Inheritance inheritance in this.inheritances)
            {
                Activity child = this.activityDatabase.ResolveDescriptor(inheritance.ChildDescriptor);
                Category parent = this.activityDatabase.ResolveToCategory(inheritance.ParentDescriptor);
                if (inheritance.DiscoveryDate != null)
                    child.ApplyInheritanceDate((DateTime)inheritance.DiscoveryDate);
                if (child == null)
                {
                    foreach (Activity activity in this.activityDatabase.AllActivities)
                    {
                        System.Diagnostics.Debug.WriteLine("Activity '" + activity.Name + "'");
                    }
                    System.Diagnostics.Debug.WriteLine("end of activities");
                }
                child.AddParent(parent);
            }
            this.inheritances.Clear();
        }
        // moves the ratings and participations from the pending queues into the Activities
        // Note that there is a bug at the moment: when an inheritance is added to an activity, all of its ratings need to cascade appropriately
        // currently they don't. However, as long as the inheritances are always added before this function is called, then it's fine
        public void ApplyParticipationsAndRatings()
        {
            DateTime nextDate;
            int numRatingsNewlyApplied = this.unappliedRatings.Count;
            while (true)
            {
                // find the next date at which something happened
                List<DateTime> dates = new List<DateTime>();
                if (this.unappliedRatings.Count > 0)
                    dates.Add((DateTime)this.unappliedRatings[0].Date);
                if (this.unappliedParticipations.Count > 0)
                    dates.Add((DateTime)this.unappliedParticipations[0].StartDate);
                if (this.unappliedSkips.Count > 0)
                    dates.Add((DateTime)this.unappliedSkips[0].CreationDate);
                if (this.unappliedSuggestions.Count > 0)
                    dates.Add(this.unappliedSuggestions[0].GuessCreationDate());
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
                while (this.unappliedRatings.Count > 0 && ((DateTime)this.unappliedRatings[0].Date).CompareTo(nextDate) == 0)
                {
                    this.CascadeRating(this.unappliedRatings[0]);
                    this.unappliedRatings.RemoveAt(0);
                }
                while (this.unappliedParticipations.Count > 0 && ((DateTime)this.unappliedParticipations[0].StartDate).CompareTo(nextDate) == 0)
                {
                    this.CascadeParticipation(this.unappliedParticipations[0]);
                    this.unappliedParticipations.RemoveAt(0);
                }
                while (this.unappliedSkips.Count > 0 && ((DateTime)this.unappliedSkips[0].CreationDate).CompareTo(nextDate) == 0)
                {
                    this.CascadeSkip(this.unappliedSkips[0]);
                    this.unappliedSkips.RemoveAt(0);
                }
                while (this.unappliedSuggestions.Count > 0 && ((DateTime)this.unappliedSuggestions[0].GuessCreationDate()).CompareTo(nextDate) == 0)
                {
                    this.CascadeSuggestion(this.unappliedSuggestions[0]);
                    this.unappliedSuggestions.RemoveAt(0);
                }
            }
            this.Update_RatingSummaries(numRatingsNewlyApplied);
        }



        private void ActivityDatabase_ActivityAdded(object sender, Activity activity)
        {
            this.CreatingActivity(activity);
        }

        // this function gets called when a new activity gets created
        public void CreatingActivity(Activity activity)
        {
            activity.SetDefaultDiscoveryDate(this.firstInteractionDate);
            //activity.ApplyKnownInteractionDate(firstInteractionDate);
            //activity.DiscoveryDate = this.firstInteractionDate;
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
            return child.GetAllSuperactivities();
        }
        // performs Depth First Search to find all subCategories of the given Activity
        public List<Activity> FindAllSubCategoriesOf(Activity parent)
        {
            return parent.GetChildrenRecursive();
        }
        public ActivitySuggestion MakeRecommendation()
        {
            DateTime when = DateTime.Now;
            return this.MakeRecommendation(when);
        }
        public ActivitySuggestion MakeRecommendation(DateTime when)
        {
            return this.MakeRecommendation(when, null);
        }

        public ActivitySuggestion MakeRecommendation(DateTime when, TimeSpan? requestedProcessingTime)
        {
            return this.MakeRecommendation((Activity)null, null, when, requestedProcessingTime);
        }


        public ActivitySuggestion MakeRecommendation(Activity requestCategory, Activity activityToBeat, DateTime when, TimeSpan? requestedProcessingTime)
        {
            List<Activity> candidates;
            // determine which activities to consider
            if (requestCategory != null)
            {
                candidates = requestCategory.GetChildrenRecursive();
            }
            else
            {
                candidates = new List<Activity>(this.activityDatabase.AllActivities);
            }
            return this.MakeRecommendation(candidates, activityToBeat, when, requestedProcessingTime);
        }

        public ActivitySuggestion MakeRecommendation(List<Activity> candidates, Activity activityToBeat, DateTime when, TimeSpan? requestedProcessingTime)
        {
            DateTime processingStartTime = DateTime.Now;
            foreach (Activity activity in this.activityDatabase.AllActivities)
            {
                activity.PredictionsNeedRecalculation = true;
            }

            // First, go update the stats for existing activities
            this.EnsureRatingsAreAssociated();

            List<Activity> consideredCandidates = new List<Activity>();

            // Now we determine which activity is most important to suggest
            // That requires first finding the one with the highest mean
            Activity bestActivity = null;
            if (activityToBeat != null)
            {
                // If the user has given another activity that they're tempted to try instead, then evaluate that activity
                // Use its short-term value as a minimum when considering other activities
                this.UpdateSuggestionValue(activityToBeat, when);
                activityToBeat.Utility = activityToBeat.Ratings.Mean; // if they're asking for us to beat this activity then it means they want to do it
                if (candidates.Contains(activityToBeat))
                {
                    candidates.Remove(activityToBeat);
                }
                // Here we add the activityToBeat as a candidate so that if activityToBeat is the best activity, then we still have give a suggestion
                bestActivity = activityToBeat;
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
                if (candidate.Choosable)
                {
                    // estimate how good it is for us to suggest this particular activity
                    this.UpdateSuggestionValue(candidate, when);
                    Distribution currentRating = candidate.SuggestionValue.Distribution;
                    bool better = false;
                    if (activityToBeat != null && candidate.Utility < activityToBeat.Utility)
                        better = false; // user doesn't have enough will power to do this activity
                    else
                    {
                        consideredCandidates.Add(candidate);
                        //if (bestActivity == null || candidate.SuggestionValue.Distribution.Mean >= bestActivity.SuggestionValue.Distribution.Mean)
                        if (bestActivity == null || candidate.SuggestionValue.Distribution.Mean >= bestActivity.SuggestionValue.Distribution.Mean)
                            better = true; // found a better activity
                    }
                    if (better)
                    {
                        /*if (bestActivity != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Candidate " + candidate + " with suggestion value " + candidate.SuggestionValue.Distribution.Mean + " replaced " + bestActivity + " with suggestion value " + bestActivity.SuggestionValue.Distribution.Mean + " as highest-value suggestion");
                        }*/
                        bestActivity = candidate;
                    }
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
            // After finding the activity with the highest expected rating, we need to check for other activities having high variance but almost-as-good values
            Activity bestActivityToPairWith = bestActivity;
            if (bestActivity == null)
                return null;
            double bestCombinedScore = GetCombinedValue(bestActivity, bestActivityToPairWith);
            foreach (Activity candidate in consideredCandidates)
            {
                double currentScore = this.GetCombinedValue(bestActivity, candidate);
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
            ActivitySuggestion suggestion = new ActivitySuggestion(activity.MakeDescriptor());
            ParticipationsSummary participationSummary = activity.SummarizeParticipationsBetween(new DateTime(), DateTime.Now);
            double typicalNumSeconds = Math.Exp(participationSummary.LogActiveTime.Mean);
            suggestion.CreatedDate = DateTime.Now;
            suggestion.StartDate = when;
            suggestion.EndDate = suggestion.StartDate.Add(TimeSpan.FromSeconds(typicalNumSeconds));
            suggestion.ParticipationProbability = activity.PredictedParticipationProbability.Distribution.Mean;
            double average = this.ActivityDatabase.RootActivity.Ratings.Mean;
            if (average == 0)
                average = 1;
            suggestion.PredictedScoreDividedByAverage = activity.PredictedScore.Distribution.Mean / average;
            return suggestion;
        }
        // This function essentially addresses the well-known multi-armed bandit problem
        // Given two distributions, we estimate the expected total value from choosing values from them
        private double GetCombinedValue(Activity activityA, Activity activityB)
        {
            Distribution a = activityA.SuggestionValue.Distribution;
            Distribution b = activityB.SuggestionValue.Distribution;
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
        public void EstimateRating(Activity activity, DateTime when)
        {
            // If we've already estimated the rating at this date, then just return what we calculated
            //DateTime latestUpdateDate = activity.PredictedScore.ApplicableDate;
            if (!activity.PredictionsNeedRecalculation)
            {
                return;
            }
            activity.PredictionsNeedRecalculation = false;
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
            activity.PredictedScore = ratingPrediction;

            // Estimate the probability that the user would do this activity
            List<Prediction> probabilityPredictions = activity.GetParticipationProbabilityEstimates(when);
            Prediction probabilityPrediction = this.CombineProbabilityPredictions(probabilityPredictions);
            activity.PredictedParticipationProbability = probabilityPrediction;



            activity.Utility = this.RatingAndProbability_Into_Value(activity.PredictedScore.Distribution, probabilityPrediction.Distribution.Mean, activity.MeanParticipationDuration).Mean;
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
        public void EstimateSuggestionValue(Activity activity, DateTime when)
        {
            foreach (Activity other in this.activityDatabase.AllActivities)
                other.PredictionsNeedRecalculation = true;
            this.EnsureRatingsAreAssociated();
            this.UpdateSuggestionValue(activity, when);
        }

        // update the estimate of how good it would be to suggest this activity now, unless the computation is already up-to-date
        private void UpdateSuggestionValue(Activity activity, DateTime when)
        {
            DateTime startDate = DateTime.Now;
            // Now we estimate how useful it would be to suggest this activity to the user

            Prediction suggestionPrediction = this.Get_Overall_SuggestionEstimate(activity, when);
            activity.SuggestionValue = suggestionPrediction;
            DateTime endDate = DateTime.Now;
            /*TimeSpan predictionDuration = endDate.Subtract(startDate);
            System.Diagnostics.Debug.WriteLine(predictionDuration.ToString() + " to evaluate " + activity.Name);*/
        }

        // attempt to calculate the probability that the user would do this activity if we suggested it at this time
        public void EstimateParticipationProbability(Activity activity, DateTime when)
        {
            List<Prediction> predictions = activity.GetParticipationProbabilityEstimates(when);
            Prediction prediction = this.CombineProbabilityPredictions(predictions);
            activity.PredictedParticipationProbability = prediction;            
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
        public Prediction Get_Overall_ParticipationEstimate(Activity activity, DateTime when)
        {
            // The activity might use its estimated rating to predict the overall future value, so we must update the rating now
            this.EstimateRating(activity, when);

            // When there is little data, we focus on the fact that doing the activity will probably be as good as doing that activity (or its parent activities)
            // When there is a medium amount of data, we focus on the fact that doing the activity will probably make the user as happy as having done the activity in the past

            Prediction shortTerm_prediction = this.CombineRatingPredictions(activity.Get_ShortTerm_RatingEstimates(when));
            double shortWeight = Math.Pow(activity.NumParticipations + 1, 0.5) * 30;
            shortTerm_prediction.Distribution = shortTerm_prediction.Distribution.CopyAndReweightTo(shortWeight);

            double mediumWeight = Math.Pow(activity.NumParticipations, 0.8333) * 40;
            Distribution ratingDistribution = activity.Predict_LongtermValue_If_Participated(when);
            Distribution mediumTerm_distribution = ratingDistribution.CopyAndReweightTo(mediumWeight);

            List<Distribution> distributions = new List<Distribution>();
            distributions.Add(shortTerm_prediction.Distribution);
            distributions.Add(mediumTerm_distribution);

            Distribution distribution = this.CombineRatingDistributions(distributions);
            Prediction prediction = shortTerm_prediction;
            prediction.Distribution = distribution;

            return prediction;
        }


        // returns a Prediction of the value of suggesting the given activity at the given time
        private Prediction Get_Overall_SuggestionEstimate(Activity activity, DateTime when)
        {
            // The activity might use its estimated rating to predict the overall future value, so we must update the rating now
            this.EstimateRating(activity, when);

            // When there is little data, we focus on the fact that doing the activity will probably be as good as doing that activity (or its parent activities)
            // When there is a medium amount of data, we focus on the fact that doing the activity will probably make the user as happy as having done the activity in the past
            // When there is a huge amount of data, we focus on the fact that suggesting the activity will probably make the user as happy as suggesting the activity in the past

            double participationProbability = activity.PredictedParticipationProbability.Distribution.Mean;

            Prediction shortTerm_prediction = this.CombineRatingPredictions(activity.Get_ShortTerm_RatingEstimates(when));
            double shortWeight = Math.Pow(activity.NumConsiderations + 1, 0.5) * 30 * participationProbability;
            shortTerm_prediction.Distribution = this.RatingAndProbability_Into_Value(shortTerm_prediction.Distribution, participationProbability, activity.MeanParticipationDuration).CopyAndReweightTo(shortWeight);

            double mediumWeight = Math.Pow(activity.NumConsiderations, 0.8333) * 70 * participationProbability;
            Distribution ratingDistribution = activity.Predict_LongtermValue_If_Participated(when);
            Distribution mediumTerm_distribution = ratingDistribution.CopyAndReweightTo(mediumWeight);

            double longWeight = activity.NumConsiderations;
            Distribution longTerm_distribution = activity.Predict_LongtermValue_If_Suggested(when).CopyAndReweightTo(longWeight);

            List<Distribution> distributions = new List<Distribution>();
            distributions.Add(shortTerm_prediction.Distribution);
            distributions.Add(mediumTerm_distribution);
            distributions.Add(longTerm_distribution);

            // also include parent activities in the prediction if this activity is one that the user hasn't done often
            if (activity.NumConsiderations < 40)
            {
                foreach (Activity parent in activity.Parents)
                {
                    Distribution parentDistribution = parent.Predict_LongtermValue_If_Participated(when);
                    double parentWeight = Math.Min(parent.NumParticipations + 1, 40);
                    distributions.Add(parentDistribution.CopyAndReweightTo(parentWeight));
                }
            }

            Distribution distribution = this.CombineRatingDistributions(distributions);
            Prediction prediction = shortTerm_prediction;
            prediction.Distribution = distribution;

            return prediction;
        }
        // suppose there only Activities are A,B,C,D
        // suppose A and B are parents of C
        // suppose C is the parent of D
        // The rating of D should exactly equal the rating of C
        // Currently, the rating of D is a combination of:
        // -The current expected rating of C
        // -The predicted rating of D based on the current expected rating of C
        // -The recent participation average of D
        // -The recent ratings of D
        // Here, only the first of these four should make any difference
        // Therefore, the other three should get weighted by (1-X), where X = (the number of child ratings divided by the number of parent ratings)

        // Suppose that an activity has n parents
        // If it is the only child of each, then its rating should be the average of each parent's rating
        // If one parent has more children, that parent's prediction weight becomes relevant, and its characteristic weight decreases

        /*public IActivitySuggestionJustification JustifySuggestion(ActivitySuggestion suggestion)
        {
            Activity activity = this.activityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor);
            IActivitySuggestionJustification justification = activity.JustifyInterpolation(suggestion);
            return justification;
        }*/
        /* // returns a string telling the most important reason that 'activity' was last rated as it was
        public string JustifyRating(Activity activity)
        {
            DateTime when = activity.SuggestionValue.ApplicableDate;
            IEnumerable<Prediction> predictions = this.GetAllSuggestionEstimates(activity, when);
            double lowestScore = 1;
            string bestReason = null;
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
            return bestReason;
        }*/
        public Prediction CombineRatingPredictions(IEnumerable<Prediction> predictions)
        {
            List<Distribution> distributions = new List<Distribution>();
            DateTime date = new DateTime(0);
            foreach (Prediction prediction in predictions)
            {
                distributions.Add(prediction.Distribution);
                if (prediction.ApplicableDate.CompareTo(date) > 0)
                    date = prediction.ApplicableDate;
            }
            Distribution distribution = this.CombineRatingDistributions(distributions);
            Prediction result = new Prediction();
            result.Distribution = distribution;
            result.ApplicableDate = date;
            return result;
        }
        public Distribution CombineRatingDistributions(IEnumerable<Distribution> distributions)
        {
            // first add up all distributions that have standard deviation equal to zero
            Distribution sumOfZeroStdDevs = new Distribution(0, 0, 0);
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
            Distribution sum = new Distribution(0, 0, 0);
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
        public Prediction CombineProbabilityPredictions(IEnumerable<Prediction> predictions)
        {
            List<Distribution> distributions = new List<Distribution>();
            DateTime date = new DateTime(0);
            foreach (Prediction prediction in predictions)
            {
                distributions.Add(prediction.Distribution);
                if (prediction.ApplicableDate.CompareTo(date) > 0)
                    date = prediction.ApplicableDate;
            }
            Distribution distribution = this.CombineProbabilityDistributions(distributions);
            Prediction result = new Prediction();
            result.Distribution = distribution;
            result.ApplicableDate = date;
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
                        if (participation.Suggested != null && participation.Suggested.Value == false)
                            this.ratingsOfUnpromptedActivities = this.ratingsOfUnpromptedActivities.Plus(newRating.Score);
                    }
                }
            }
        }
        // tells each activity to spend a little bit of time updating its knowledge of the ratings that came after one of its participations
        public void Update_RatingSummaries(int numRatingsToUpdate)
        {
            foreach (Activity activity in this.ActivityDatabase.AllActivities)
            {
                activity.UpdateNext_RatingSummaries(numRatingsToUpdate);
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
            if (newParticipation.Suggested != null)
            {
                if (newParticipation.Suggested.Value)
                    this.numPromptedParticipations++;
                else
                    this.numUnpromptedParticipations++;
            }
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
            if (this.currentExperiments.ContainsKey(activity))
            {
                PlannedExperiment experiment = this.currentExperiments[activity];
                if (experiment.InProgress)
                {
                    // found the second participation in the experiment, so mark the experiment as complete
                    this.currentExperiments.Remove(this.ActivityDatabase.ResolveDescriptor(experiment.Earlier.ActivityDescriptor));
                    this.currentExperiments.Remove(this.ActivityDatabase.ResolveDescriptor(experiment.Later.ActivityDescriptor));
                    this.numCompletedExperiments++;
                }
                else
                {
                    // found the first participation in the experiment
                    experiment.FirstParticipation = newParticipation;
                }
            }
        }

        private void ActivityDatabase_InheritanceAdded(object sender, Inheritance inheritance)
        {
            Activity child = this.activityDatabase.ResolveDescriptor(inheritance.ChildDescriptor);
            if (child.Parents.Count > 1)
            {
                // this was an existing activity that was given another parent, so we have to cascade ratings, participations, etc
                this.requiresFullUpdate = true;
            }
            this.EstimateRating(child, DateTime.Now);
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
            this.inheritances.Add(newInheritance);
            this.PutActivityDescriptorInMemory(newInheritance.ParentDescriptor);
            this.PutActivityDescriptorInMemory(newInheritance.ChildDescriptor);
            this.requiresFullUpdate = true;
        }
        public void PutSkipInMemory(ActivitySkip newSkip)
        {
            this.unappliedSkips.Add(newSkip);
            this.numSkips++;

            this.DiscoveredActionDate(newSkip.CreationDate);

            if (newSkip.ConsideredSinceDate != null)
            {
                TimeSpan duration = newSkip.CreationDate.Subtract(newSkip.ConsideredSinceDate);
                if (duration.TotalDays > 1)
                    System.Diagnostics.Debug.WriteLine("skip duration > 1 day, this is probably a mistake");
                // update our estimate of how longer the user spends thinking about what to do
                this.thinkingTime = this.thinkingTime.Plus(Distribution.MakeDistribution(duration.TotalSeconds, 0, 1));
                // record the fact that the user wasn't doing anything directly productive at this time
                this.weightedRatingSummarizer.AddParticipationIntensity(newSkip.ConsideredSinceDate, newSkip.CreationDate, 0);
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
            this.currentExperiments.Add(this.ActivityDatabase.ResolveDescriptor(experiment.Earlier.ActivityDescriptor), experiment);
            this.currentExperiments.Add(this.ActivityDatabase.ResolveDescriptor(experiment.Later.ActivityDescriptor), experiment);
        }
        // tells the Engine about an Activity that it may choose from
        public void PutActivityDescriptorInMemory(ActivityDescriptor newDescriptor)
        {
            this.allActivityDescriptors.Add(newDescriptor);
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
            this.EstimateRating(thisActivity, mainParticipation.StartDate);
            Distribution thisPrediction = thisActivity.PredictedScore.Distribution;

            // make an AbsoluteRating for the other Activity (include date + activity because they're not implied)
            AbsoluteRating otherRating = new AbsoluteRating();
            otherRating.Date = otherParticipation.StartDate;
            otherRating.ActivityDescriptor = otherParticipation.ActivityDescriptor;
            this.EstimateRating(otherActivity, otherParticipation.StartDate);
            Distribution otherPrediction = otherActivity.PredictedScore.Distribution;

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
            this.EstimateRating(activity, sourceParticipation.StartDate);
            AbsoluteRating rating = new AbsoluteRating(activity.PredictedScore.Distribution.Mean, sourceParticipation.StartDate, sourceParticipation.ActivityDescriptor, RatingSource.FromParticipation(sourceParticipation));
            rating.FromUser = false;
            return rating;
        }


        // Tells whether ChooseExperimentOption will return an error
        // If ChooseExperimentOption is not available right now, returns a string explaining why. Otherwise returns ""
        public String Test_ChooseExperimentOption()
        {
            SuggestedMetricOrError result = this.ChooseExperimentOption(new List<SuggestedMetric>(0), null, new DateTime(), true);
            if (result != null)
                return result.Error;
            return "";
        }

        // given a list of options of activities that the user is considering doing; returns a new option (different than the others) that can be added to the list
        public SuggestedMetricOrError ChooseExperimentOption(List<SuggestedMetric> existingOptions, TimeSpan? requestedProcessingTime, DateTime when, bool dryRun = false)
        {
            // activities that can ever be put in an experiment
            List<Activity> activitiesHavingMetrics = this.ChooseableActivitiesHavingMetrics;

            // activities that are already listed as options can't be re-added as new options
            HashSet<Activity> excludedActivities = new HashSet<Activity>();
            foreach (SuggestedMetric existingOption in existingOptions)
            {
                excludedActivities.Add(this.ActivityDatabase.ResolveDescriptor(existingOption.ActivityDescriptor));
            }
            // determine which activities are already planned as part of an experiment
            HashSet<Activity> plannedExperimentActivities = new HashSet<Activity>(this.PlannedExperimentActivities);

            List<Activity> availablePreActivities = new List<Activity>();
            List<Activity> availablePostActivities = new List<Activity>();
            foreach (Activity activity in activitiesHavingMetrics)
            {
                if (!excludedActivities.Contains(activity))
                {
                    // found an available Activity; classify it as a pre-Activity (not yet part of a planned Experiment) or post-Activity (already is the second task in a planned Experiment)
                    if (plannedExperimentActivities.Contains(activity))
                        availablePostActivities.Add(activity);
                    else
                        availablePreActivities.Add(activity);
                }
            }

            // Motivation:
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
            int numCompletedExperimentalParticipations = this.NumCompletedExperimentalParticipations;
            int min_pendingPool_size = Math.Max((int)Math.Pow(numCompletedExperimentalParticipations, 0.667), 3); // need at least 3 tasks available to even start an experiment
            int numActivitiesToChooseFrom = availablePreActivities.Count + availablePostActivities.Count + existingOptions.Count;
            if (numActivitiesToChooseFrom < min_pendingPool_size)
            {
                int numExtraRequiredActivities = min_pendingPool_size - numActivitiesToChooseFrom;
                string message = "Don't have enough activities having metrics to run another experiment. Go create " + numExtraRequiredActivities + " more ";
                if (numExtraRequiredActivities == 1)
                    message += "activity of type ToDo!";
                else
                    message += "activities of type ToDo!";
                // no enough activities for a meaningful experiment
                return new SuggestedMetricOrError(message);
            }

            if (dryRun)
            {
                // validation completed; tell the caller that there was no error
                return null;
            }

            // We also want the pre/post status of each item in the pool to be as unpredictable to the user as possible,
            // to prevent them from subconsciously working harder on tasks that are known to be post tasks.
            // So, we randomly decide whether to add suggest a pre-task or a post-task, and we weight the suggestion based on how are already in the pool
            int max_pendingPool_size = min_pendingPool_size * 2;
            bool choosePostActivity;
            if (availablePostActivities.Count < min_pendingPool_size)
            {
                // not enough tasks in the pool; we're at risk of having too few overlapping comparisons
                choosePostActivity = false;
            }
            else
            {
                if (availablePostActivities.Count > max_pendingPool_size)
                {
                    // pool is more than big enough; we should start collecting data (completing experiments)
                    choosePostActivity = true;
                }
                else
                {
                    int preWeight = availablePostActivities.Count - min_pendingPool_size;
                    int postWeight = max_pendingPool_size - availablePostActivities.Count;
                    choosePostActivity = (this.randomGenerator.Next(preWeight + postWeight) >= preWeight);
                }
            }

            List<Activity> recommendableActivities;
            if (choosePostActivity)
                recommendableActivities = availablePostActivities;
            else
                recommendableActivities = availablePreActivities;


            // Now that we've identified some activities that are reasonable to add to the experiment, choose the one that we think will provide the most longterm value to the user
            Activity bestActivity = this.activityDatabase.ResolveDescriptor(this.MakeRecommendation(recommendableActivities, null, DateTime.Now, requestedProcessingTime).ActivityDescriptor);

            // chose a random metric for this activity
            List<Metric> metrics = bestActivity.Metrics;
            if (metrics.Count < 1)
                throw new Exception("Internal error while planning experiment: activity " + bestActivity + " has no metrics");
            Metric metric = metrics[this.randomGenerator.Next(metrics.Count)];

            PlannedMetric plannedMetric = new PlannedMetric();
            plannedMetric.ActivityDescriptor = bestActivity.MakeDescriptor();
            plannedMetric.MetricName = metric.Name;
            plannedMetric.EstimatedSuccessesPerSecond = 1; // TODO: predict the success rate for bestActivity, or at least take the average
            return new SuggestedMetricOrError(new SuggestedMetric(plannedMetric, this.SuggestActivity(bestActivity, when)));
        }

        public ExperimentSuggestion Experiment(List<SuggestedMetric> choices, DateTime when)
        {
            // separate into pre-activities and post-activities
            List<SuggestedMetric> unpairedActivities = new List<SuggestedMetric>();
            List<SuggestedMetric> pairedActivities = new List<SuggestedMetric>();

            foreach (SuggestedMetric suggestion in choices)
            {
                if (this.isPartOfPendingExperiment(this.ActivityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor)))
                    pairedActivities.Add(suggestion);
                else
                    unpairedActivities.Add(suggestion);
            }
            // to start a new experiment we need at least 2 available activities
            if (unpairedActivities.Count < 2)
                unpairedActivities.Clear();

            // Choose whether to start a new experiment or finish an existing experiment.
            // We've gone to great lengths to hide from the user the information about whether we're finishing an existing experiment or starting a new experiment because
            // if the user discovers which one this is, it might subconsciously motivate them to work harder on the post tasks and less hard on the pre tasks, artificially skewing the results
            int suggestionIndex = this.randomGenerator.Next(unpairedActivities.Count + pairedActivities.Count);

            bool choosePreActivity = (suggestionIndex < unpairedActivities.Count);
            if (choosePreActivity)
            {
                // We're choosing a pre-activity. Pair it with another unpaired activity
                int suggestion2Index = this.randomGenerator.Next(unpairedActivities.Count - 1);
                if (suggestion2Index == suggestionIndex)
                    suggestion2Index = unpairedActivities.Count - 1;

                PlannedExperiment experiment = new PlannedExperiment();
                experiment.Earlier = unpairedActivities[suggestionIndex].PlannedMetric;
                experiment.Later = unpairedActivities[suggestion2Index].PlannedMetric;

                Activity earlierActivity = this.ActivityDatabase.ResolveDescriptor(experiment.Earlier.ActivityDescriptor);
                return new ExperimentSuggestion(experiment, unpairedActivities[suggestionIndex].ActivitySuggestion);
            }
            else
            {
                // We're choosing a post-activity. Find the experiment we previously created for it
                SuggestedMetric experimentSuggestion = pairedActivities[suggestionIndex];
                Activity laterActivity = this.ActivityDatabase.ResolveDescriptor(experimentSuggestion.ActivityDescriptor);
                PlannedExperiment experiment = this.findPendingExperiment(laterActivity);
                return new ExperimentSuggestion(experiment, experimentSuggestion.ActivitySuggestion);
            }
        }

        private PlannedExperiment findPendingExperiment(Activity activity)
        {
            if (this.currentExperiments.ContainsKey(activity))
            {
                return this.currentExperiments[activity];
            }
            return null;
        }
        // tells whether there's already a pending experiment containing this activity
        private bool isPartOfPendingExperiment(Activity activity)
        {
            return (this.findPendingExperiment(activity) != null);
        }

        private int NumCompletedExperiments
        {
            get
            {
                return this.numCompletedExperiments;
            }
        }

        private int NumCompletedExperimentalParticipations
        {
            get
            {
                // NumCompletedExperiments counts the individual experiments that completed, but we're interested in the number of completed participations (so double it) .
                // this.currentExperiments has 2 entries per experiment (so it can be found via either Activity), but only 1 of them will have been completed (so halve it) .
                return this.NumCompletedExperiments * 2 + this.currentExperiments.Count / 2;
            }
        }
        // returns a list of Activities that are available to be added into a new Experiment
        private List<Activity> ChooseableActivitiesHavingMetrics
        {
            get
            {
                // find each Activity that has a metric
                List<Activity> results = new List<Activity>();
                foreach (Activity activity in this.activityDatabase.AllActivities)
                {
                    if (activity.Choosable)
                    {
                        List<Metric> metrics = activity.Metrics;
                        if (metrics.Count > 0)
                        {
                            results.Add(activity);
                        }
                    }
                }
                return results;
            }
        }
        // returns a list of activities for which there is a Participation planned as part of a planned but not completed Experiment
        private HashSet<Activity> PlannedExperimentActivities
        {
            get
            {
                HashSet<Activity> plannedExperiments = new HashSet<Activity>();
                foreach (PlannedExperiment experiment in this.currentExperiments.Values)
                {
                    plannedExperiments.Add(this.ActivityDatabase.ResolveDescriptor(experiment.Later.ActivityDescriptor));
                    if (experiment.FirstParticipation != null)
                        plannedExperiments.Add(this.ActivityDatabase.ResolveDescriptor(experiment.Earlier.ActivityDescriptor));
                }
                return plannedExperiments;
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
        public RatingSummarizer RatingSummarizer
        {
            get
            {
                return this.weightedRatingSummarizer;
            }
        }
        private UserPreferences Get_UserPreferences()
        {
            return UserPreferences.DefaultPreferences;
        }
        private ActivityDatabase activityDatabase;                  // stores all Activities
        private List<AbsoluteRating> unappliedRatings;              // lists all Ratings that the RatingProgressions don't know about yet
        private List<Participation> unappliedParticipations;        // lists all Participations that the ParticipationProgressions don't know about yet
        private List<ActivitySkip> unappliedSkips;                  // lists all skips that the progressions don't know about yet
        private List<ActivitySuggestion> unappliedSuggestions;            // lists all ActivitySuggestions that the Activities don't know about yet
        private List<ActivityDescriptor> allActivityDescriptors;    // lists all Activities that we need to create
        private List<Inheritance> inheritances; // tells which Activities are descendents of which others
        // the PredictionLinks can predict the rating of an activity based on the intensity of the parent participations and based on parent ratings
        // 1. Let us create one PredictionLink per parent, which predictions the child's current rating from the parent's current rating
        // This PredictionLink must be trained using only ratings that are actually provided, but as inputs to guess from it may take the current calculated rating of the parent
        // 2. Let us create one PredictionLink per Activity, which predicts its rating from its recent ratings
        // 3. Let us create one PredictionLink per Activity, which predicts its rating from its recent participations
        // private Dictionary<string, Dictionary<string, PredictionLink>> PredictionLinks;
        //TextConverter textConverter;    // converts objects to and from text
        //string ratingsFileName;         // the name of the file that stores ratings
        //string inheritancesFileName;    // the name of the file that stores inheritances
        DateTime firstInteractionDate;
        DateTime latestInteractionDate;
        bool requiresFullUpdate;
        Distribution thinkingTime;      // how long the user spends before skipping a suggestion
        //Distribution ratingsForSuggestedActivities;     // the ratings that the user gives to activities that were suggested by the engine
        //Distribution ratingsForUnsuggestedActivities;   // the ratings that the user gives to activities that were not suggestd by the engine
        RatingSummarizer weightedRatingSummarizer;
        
        
        Distribution ratingsOfUnpromptedActivities;
        int numSkips;
        int numPromptedParticipations;
        int numUnpromptedParticipations;
        private Random randomGenerator = new Random();

        // a list of experiments that have been planned but not yet completed
        Dictionary<Activity, PlannedExperiment> currentExperiments = new Dictionary<Activity, PlannedExperiment>();
        int numCompletedExperiments;

    }
}
