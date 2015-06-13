using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ActivityRecommendation
{
    class Engine
    {
        public Engine()
        {
            this.ratingSummarizer = new RatingSummarizer(UserPreferences.DefaultPreferences.HalfLife);
            this.activityDatabase = new ActivityDatabase(this.ratingSummarizer);
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
        // creates an Activity object for each ActivityDescriptor that needs one
        public void CreateNewActivities()
        {
            // first, create the necessary Activities
            foreach (ActivityDescriptor descriptor in this.allActivityDescriptors)
            {
                Activity newActivity = this.activityDatabase.CreateActivityIfMissing(descriptor);
                if (newActivity != null)
                {
                    this.CreatingActivity(newActivity);
                }
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
                Activity parent = this.activityDatabase.ResolveDescriptor(inheritance.ParentDescriptor);
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
            return parent.GetAllSubactivities();
        }
        public Activity MakeRecommendation()
        {
            DateTime when = DateTime.Now;
            return this.MakeRecommendation(when);
        }
        public Activity MakeRecommendation(DateTime when)
        {
            return this.MakeRecommendation(when, null);
        }

        public Activity MakeRecommendation(DateTime when, TimeSpan? requestedProcessingTime)
        {
            return this.MakeRecommendation(this.activityDatabase.AllActivities, when, 0, requestedProcessingTime);
        }

        public Activity MakeRecommendation(Activity categoryToConsider, DateTime when, TimeSpan? requestedProcessingTime)
        {
            List<Activity> availableCandidates = this.FindAllSubCategoriesOf(categoryToConsider);
            return this.MakeRecommendation(availableCandidates, when, 0, requestedProcessingTime);
        }

        public Activity MakeRecommendation(IEnumerable<Activity> candidateActivities, DateTime when, double greediness, TimeSpan? requestedProcessingTime)
        {
            List<Activity> candidates = new List<Activity>(candidateActivities);
            List<Activity> consideredCandidates = new List<Activity>();
            DateTime processingStartTime = DateTime.Now;
            // First, go estimate the value for each activity
            if (this.requiresFullUpdate)
            {
                this.FullUpdate();
            }
            else
            {
                this.ApplyParticipationsAndRatings();
            }
            foreach (Activity activity in this.activityDatabase.AllActivities)
            {
                activity.PredictionsNeedRecalculation = true;
            }
            // Now we determine which activity is most important to suggest
            // That requires first finding the one with the highest mean
            Activity bestActivity = null;
            Distribution bestRating = null;
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
                    consideredCandidates.Add(candidate);
                    System.Diagnostics.Debug.WriteLine("Considering " + candidate);
                    // estimate how good it is for us to suggest this particular activity
                    this.EstimateSuggestionValue(candidate, when);
                    Distribution currentRating = candidate.SuggestionValue.Distribution.CopyAndReweightBy(1 - greediness).Plus(candidate.PredictedScore.Distribution.CopyAndReweightBy(greediness));
                    if (bestRating == null || bestRating.Mean < currentRating.Mean)
                    {
                        bestActivity = candidate;
                        bestRating = currentRating;
                    }
                    if (consideredCandidates.Count >= 2 && requestedProcessingTime != null)
                    {
                        // check whether we've used up all of our processing time, but always consider at least two activities
                        DateTime now = DateTime.Now;
                        TimeSpan spentDuration = now.Subtract(processingStartTime);
                        if (spentDuration.CompareTo(requestedProcessingTime) >= 0)
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
            double bestCombinedScore = GetCombinedValue(bestActivity, bestActivityToPairWith, greediness);
            foreach (Activity candidate in consideredCandidates)
            {
                double currentScore = this.GetCombinedValue(bestActivity, candidate, greediness);
                if (currentScore > bestCombinedScore)
                {
                    bestActivityToPairWith = candidate;
                    bestCombinedScore = currentScore;
                }
            }
            // If there was a pair of activities that could do strictly better than two of the best activity, then we must actually choose the second-best
            // If there was no such pair, then we just want to choose the best activity because no others could help
            // Remember that the reason the activity with second-highest rating might be a better choice is that it might have a higher variance
            return bestActivityToPairWith;
        }
        // This function essentially addresses the well-known multi-armed bandit problem
        // Given two distributions, we estimate the expected total value from choosing values from them
        private double GetCombinedValue(Activity activityA, Activity activityB, double greediness)
        {
            Distribution a = activityA.SuggestionValue.Distribution.CopyAndReweightBy(1 - greediness).Plus(activityA.PredictedScore.Distribution.CopyAndReweightBy(greediness));
            Distribution b = activityB.SuggestionValue.Distribution.CopyAndReweightBy(1 - greediness).Plus(activityB.PredictedScore.Distribution.CopyAndReweightBy(greediness));
            TimeSpan interval1 = activityA.AverageTimeBetweenConsiderations;
            TimeSpan interval2 = activityB.AverageTimeBetweenConsiderations;
            BinomialDistribution distribution1 = new BinomialDistribution((1 - a.Mean) * a.Weight, a.Mean * a.Weight);
            BinomialDistribution distribution2 = new BinomialDistribution((1 - b.Mean) * b.Weight, b.Mean * b.Weight);

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
            activity.SetupRatingPredictorsIfNeeded();
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

#if false   // Would need to bring back something like this if we need to directly account for the possibility that the user won't take our suggestion
            // For each action the user might take (do it, skip it, or do something else), compute its probability
            double suggestedParticipation_probability = probabilityPrediction.Distribution.Mean;
            double skipProbability = (1 - suggestedParticipation_probability) * (double)(this.numSkips + 1) / ((double)this.numSkips + (double)this.numUnpromptedParticipations + 2);
            double nonsuggestedParticipation_probability = 1 - (suggestedParticipation_probability + skipProbability);

            // Now compute the probability that the eventual selection will be a suggested activity (assuming that all activities are like this one)
            double probabilitySuggested = suggestedParticipation_probability / (suggestedParticipation_probability + nonsuggestedParticipation_probability);

            // Now compute the expected amount of wasted time
            double averageWastedSeconds = this.thinkingTime.Mean * (-1 + 1 / (1 - skipProbability));

            // Let p be the probability of skipping this activity. The expected number of skips then is p + p^2 ... = -1 + 1 + p + p^2... = -1 + 1 / (1 - p)
            // = -1 + 1 / (1 - (the probability that the user will skip the activity))
            // So the amount of waste is (the average length of a skip) * (-1 + 1 / (1 - (the probability that the user will skip the activity)))

            

            
            // Finally, when calculating the suggestionValue, we can update all the scores
            foreach (Prediction prediction in ratingPredictions)
            {
                // Now compute what the expected value would be, assuming that we choose something
                double valueWhenChosen = probabilitySuggested * prediction.Distribution.Mean + (1 - probabilitySuggested) * this.ratingsOfUnpromptedActivities.Mean;


                //double usefulFraction = activity.MeanParticipationDuration / (averageWastedSeconds + activity.MeanParticipationDuration);
                
                // TODO: for activities other than this one, use a better estimate of the participation duration than just the usual duration of the current activity
                double overallValue = valueWhenChosen * activity.MeanParticipationDuration / (averageWastedSeconds + activity.MeanParticipationDuration);

                double scale = overallValue / prediction.Distribution.Mean;
                prediction.Distribution = prediction.Distribution.CopyAndStretchBy(scale);
                //prediction.Distribution = Distribution.MakeDistribution(overallValue, prediction.Distribution.StdDev, prediction.Distribution.Weight);
            }
#endif

        }

        // update the estimate of how good it would be to suggest this activity now
        public void EstimateSuggestionValue(Activity activity, DateTime when)
        {
            DateTime startDate = DateTime.Now;
            // Now we estimate how useful it would be to suggest this activity to the user
            //List<Prediction> extraPredictions = this.GetSuggestionEstimates(activity, when);

            //IEnumerable<Prediction> suggestionPredictions = ratingPredictions.Concat(extraPredictions);
            IEnumerable<Prediction> suggestionPredictions = this.GetSuggestionEstimates(activity, when);
            activity.SuggestionValue = this.CombineRatingPredictions(suggestionPredictions);
            DateTime endDate = DateTime.Now;
            TimeSpan predictionDuration = endDate.Subtract(startDate);
            //System.Diagnostics.Debug.WriteLine(predictionDuration.ToString() + " to evaluate " + activity.Name);
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
            List<Prediction> predictions = activity.Get_ShortTerm_RatingEstimate(when);
            return predictions;
        }
        // returns a list of all Predictions that are used to predict the value of suggesting the given activity at the given time
        private List<Prediction> GetSuggestionEstimates(Activity activity, DateTime when)
        {
            // The activity might use its estimated rating to predict the overall future value, so we must update the rating now
            this.EstimateRating(activity, when);
            // Get the activity's estimates of what the overall value will be after having done this activity
            List<Prediction> predictions = activity.Get_LongTerm_ValueEstimates(when);
#if false
            // Now add a correction that helps avoid suggesting the same activity multiple times in a row
            int numActivities = this.activityDatabase.NumActivities;
            DateTime latestParticipationDate = activity.LatestParticipationDate;
            TimeSpan unplayedDuration = when.Subtract(latestParticipationDate);
            double numUnplayedHours = unplayedDuration.TotalHours;
            double weightFraction = (double)1 - numUnplayedHours / (double)24;
            if (weightFraction > 0)
            {
                Prediction spacer = new Prediction();
                spacer.Distribution = new Distribution(0, 0.5, weightFraction * 8);
                spacer.Justification = "you did this recently";
                predictions.Add(spacer);
            }
#endif
            /* 
            // We no longer need to incorporate the following distribution, which was solely to ensure that no activity was ever forgotten
            // The right way to do this is to create a more-accurate model of the rating of an activity when it hasn't been suggested in a while
            // Finally, take into account the fact that we gain more information by suggesting activities that haven't been remembered in a while
            DateTime latestActivityInteractionDate = activity.LatestInteractionDate;
            TimeSpan idleDuration = when.Subtract(latestActivityInteractionDate);
            double numIdleHours = idleDuration.TotalHours;
            if (numIdleHours > 0)
            {
                weightFraction = Math.Pow(numIdleHours, 0.7);
                double stdDev = 1;
                //guess = new Distribution(
                Distribution scores = Distribution.MakeDistribution(1, stdDev, weightFraction);
                Prediction guess = new Prediction();
                guess.Distribution = scores;
                guess.Justification = "how long it's been since you thought about this activity";
                predictions.Add(guess);
            }
            */
            return predictions;
        }
        // returns a list of all predictions that were used to predict the value of suggesting the given activity at the given time
        private IEnumerable<Prediction> GetAllSuggestionEstimates(Activity activity, DateTime when)
        {
            List<Prediction> ratingPredictions = this.Get_ShortTerm_RatingEstimates(activity, when);
            List<Prediction> suggestionPredictions = this.GetSuggestionEstimates(activity, when);
            return ratingPredictions.Concat(suggestionPredictions);
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

        // returns a string telling the most important reason that 'activity' was last rated as it was
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
        }
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

            // keep track of the first and last date at which anything happened
            this.DiscoveredRating(newRating);
            // keep track of any unapplied ratings
            this.unappliedRatings.Add(newRating);
            this.PutActivityDescriptorInMemory(newRating.ActivityDescriptor);
            // keep track of how well the user has been spending time
            if (newRating.Source != null)
            {
                Participation participation = newRating.Source.ConvertedAsParticipation;
                if (participation != null)
                {
                    // exponential moving average with all necessary history
                    this.ratingSummarizer.AddRating(participation.StartDate, participation.EndDate, newRating.Score);
                    // usual average of the activities that were not suggested
                    if (participation.Suggested != null && participation.Suggested.Value == false)
                        this.ratingsOfUnpromptedActivities = this.ratingsOfUnpromptedActivities.Plus(newRating.Score);
                }
            }
        }
        // tells each activity to spend a little bit of time updating its knowledge of the ratings that came after one of its participations
        public void Update_RatingSummaries(int numRatingsToUpdate)
        {
            foreach (Activity activity in this.ActivityDatabase.AllActivities)
            {
                activity.UpdateNext_RatingSummaries(this.ratingSummarizer, numRatingsToUpdate);
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

            this.ratingSummarizer.AddParticipationIntensity(newParticipation.StartDate, newParticipation.EndDate, 1);

            
            Rating rating = newParticipation.GetCompleteRating();
            if (rating != null)
            {
                this.PutRatingInMemory(rating);
            }
        }
        // provides a previously unknown Inheritance to the Engine
        public void ApplyInheritance(Inheritance newInheritance)
        {
            // find the activity being described
            ActivityDescriptor childDescriptor = newInheritance.ChildDescriptor;
            ActivityDescriptor parentDescriptor = newInheritance.ParentDescriptor;
            Activity child = this.activityDatabase.ResolveDescriptor(childDescriptor);
            if (child == null)
            {
                // if the activity doesn't exist, then create it
                child = this.activityDatabase.CreateActivityIfMissing(childDescriptor);
                // calculate an appropriate DiscoveryDate
                this.CreatingActivity(child);
                if (newInheritance.DiscoveryDate != null)
                    child.ApplyInheritanceDate((DateTime)newInheritance.DiscoveryDate);
            }
            else
            {
                // if we're not creating a new, empty child, then lots of ratings and participations may have to cascade
                // and so we will update everything before making another recommendation
                this.requiresFullUpdate = true;
            }
            // locate or create the correct parent
            Activity parent = this.activityDatabase.ResolveDescriptor(parentDescriptor);
            if (parent == null)
            {
                parent = this.activityDatabase.CreateActivityIfMissing(parentDescriptor);
                this.CreatingActivity(parent);
            }
            child.AddParent(parent);
            // Important! if (this.requiresFullUpdate) then the value calculated in EstimateRating will be wrong
            // However, when we need the correct value, we'll go calculate it, so it's okay
            // It's only when we're doing autocomplete that we don't bother with the full update
            // if we just created an empty child, then we can estimate its rating based on the parent's rating
            this.EstimateRating(child, DateTime.Now);
            /*if (!this.requiresFullUpdate)
            {
                // if we just created an empty child, then we can estimate its rating based on the parent's rating
                this.EstimateRating(child, DateTime.Now);
                //this.MakeRecommendation(child, DateTime.Now);
            }*/
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

            if (newSkip.SuggestionCreationDate != null)
            {
                TimeSpan duration = newSkip.CreationDate.Subtract((DateTime)newSkip.SuggestionCreationDate);
                if (duration.TotalDays > 1)
                    System.Diagnostics.Debug.WriteLine("skip duration > 1 day, this is probably a mistake");
                // update our estimate of how longer the user spends thinking about what to do
                this.thinkingTime = this.thinkingTime.Plus(Distribution.MakeDistribution(duration.TotalSeconds, 0, 1));
                // record the fact that the user wasn't doing anything directly productive at this time
                this.ratingSummarizer.AddParticipationIntensity(newSkip.SuggestionCreationDate.Value, newSkip.CreationDate, 0);
            }

#if false
            Rating newRating = newSkip.GetCompleteRating();
            AbsoluteRating convertedRating = newRating as AbsoluteRating;
            if (convertedRating != null)
                this.PutRatingInMemory(convertedRating);
#endif
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
            this.ratingSummarizer.RemoveParticipation(participationToRemove.StartDate);
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
                return this.ratingSummarizer;
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
        RatingSummarizer ratingSummarizer;
        
        Distribution ratingsOfUnpromptedActivities;
        int numSkips;
        int numPromptedParticipations;
        int numUnpromptedParticipations;
        private Random randomGenerator = new Random();


    }
}
