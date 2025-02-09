﻿using ActivityRecommendation.Effectiveness;
using AdaptiveInterpolation;
using StatLists;
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
            this.weightedRatingSummarizer = new ExponentialRatingSummarizer(CommonPreferences.Instance.HalfLife);
            this.efficiencySummarizer = new ExponentialRatingSummarizer(CommonPreferences.Instance.EfficiencyHalflife);
            this.efficiencyCorrelator = new EfficiencyCorrelator();
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

            this.longTerm_skipValue_interpolator = new LongtermValuePredictor(this.weightedRatingSummarizer);
            this.longTerm_participationValue_interpolator = new LongtermValuePredictor(this.weightedRatingSummarizer);
            this.longTerm_efficiency_interpolator = new LongtermValuePredictor(this.efficiencySummarizer);
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
            this.Update_RatingSummaries(ratingIndex + participationIndex + skipIndex);
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
                // give participation to ancestor activities too
                List<Activity> superCategories = this.FindAllSupercategoriesOf(activity);
                foreach (Activity parent in superCategories)
                {
                    if (parent != activity)
                    {
                        parent.AddParticipation(newParticipation);
                    }
                }
            }
            RelativeEfficiencyMeasurement efficiencyMeasurement = newParticipation.RelativeEfficiencyMeasurement;
            this.addEfficiencyMeasurement(efficiencyMeasurement);

            this.longTerm_participationValue_interpolator.AddDatapoint(newParticipation.StartDate, this.getCoordinatesForParticipation(newParticipation.StartDate, activity));
        }
        private void addEfficiencyMeasurement(RelativeEfficiencyMeasurement measurement)
        {
            if (measurement == null)
                return;
            this.addEfficiencyMeasurement(measurement.Earlier);
            this.efficiencySummarizer.AddRating(measurement.StartDate, measurement.EndDate, measurement.RecomputedEfficiency.Mean);
            this.efficiencyCorrelator.Add(measurement.StartDate, measurement.EndDate, measurement.RecomputedEfficiency.Mean);
            Activity activity = this.ActivityDatabase.ResolveDescriptor(measurement.ActivityDescriptor);
            foreach (Activity parent in this.FindAllSupercategoriesOf(activity))
            {
                parent.AddEfficiencyMeasurement(measurement);
            }
            this.longTerm_efficiency_interpolator.AddDatapoint(measurement.StartDate, this.getCoordinatesForParticipation(measurement.StartDate, activity));
        }
        // gives the Skip to all Activities to which it applies
        public void CascadeSkip(ActivitySkip newSkip)
        {
            foreach (ActivityDescriptor descriptor in newSkip.ActivityDescriptors)
            {
                Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
                if (activity != null)
                {
                    List<Activity> superCategories = this.FindAllSupercategoriesOf(activity);
                    foreach (Activity parent in superCategories)
                    {
                        parent.AddSkip(newSkip);
                    }
                }
                this.longTerm_skipValue_interpolator.AddDatapoint(newSkip.SuggestionCreationDate, this.getCoordinatesForSkip(newSkip.SuggestionCreationDate, activity));
            }
        }
        // assigns this ActivitySuggestion to each relevant activity
        private void CascadeSuggestion(ActivitySuggestion suggestion)
        {
            Activity activity = this.activityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor);
            if (activity != null)
            {
                List<Activity> superCategories = this.FindAllSupercategoriesOf(activity);
                foreach (Activity parent in superCategories)
                {
                    parent.AddSuggestion(suggestion);
                }
            }
        }
        // performs Depth First Search to find all superCategories of the given Activity
        public List<Activity> FindAllSupercategoriesOf(Activity child)
        {
            return this.activityDatabase.GetAllSuperactivitiesOf(child);
        }
        // performs Depth First Search to find all subCategories of the given Activity
        public IEnumerable<Activity> FindAllSubCategoriesOf(Activity parent)
        {
            if (parent == this.activityDatabase.RootActivity)
                return this.activityDatabase.AllActivities;
            else
                return parent.GetChildrenRecursive();
        }
        public ActivitiesSuggestion MakeRecommendation()
        {
            ActivityRequest request = new ActivityRequest();
            return this.MakeRecommendation(request);
        }
        public ActivitiesSuggestion MakeRecommendation(DateTime when)
        {
            ActivityRequest request = new ActivityRequest(when);
            return this.MakeRecommendation(request);
        }

        public List<Activity> GetActivitiesToConsider(ActivityRequest request)
        {
            List<Activity> candidates;
            if (request.LeafActivitiesToConsider != null)
            {
                // caller specified which activities to consider
                candidates = request.LeafActivitiesToConsider;
            }
            else
            {
                // caller requested a certain category
                Activity category;
                if (request.FromCategory != null)
                    category = this.activityDatabase.ResolveDescriptor(request.FromCategory);
                else
                    category = this.activityDatabase.RootActivity;
                // Now we identify the relevant subcategories from within the given category
                candidates = this.getActivitiesToConsider(category);
            }
            return candidates;
        }
        // which activities to consider suggesting given that the user asked for a suggestion from <fromCategory>
        private List<Activity> getActivitiesToConsider(Activity fromCategory)
        {
            // Note that the question of whether a category is suggestible depends on some interesting interactions among inheritances
            // Suppose an inheritance hierarchy like this:
            //   A
            //  / \
            // B   C   D
            //      \ / \
            //       E   F
            // If the user asks for a suggestion from A, then we consider B and C. We don't consider E, however, because we believe that suggesting C is already clear enough.
            // It's possible that C could have more children in reality that the user hasn't considered it important enough to enter, and that
            // if we suggest C then that's more clear and more generalizable than if we suggest E.
            //
            // However, if the user asks for a suggestion from D, then we consider E and F. The reason we consider E now is because D has multiple children that the user
            // has considered it worthwhile to distinguish.
            //
            // The main reason we might have an inheritance hierarchy like this (a parent having exactly one child) is if activities are classified by multiple overlapping
            // criteria. For example, suppose that each of these activities above represents:
            //
            // A = The set of activities that can improve communication with other people
            // B = Waiting to communicate until a later time
            // C = Communicating face-to-face
            // D = Talking to some specific people
            // E = Communicating face-to-face with some specific people
            // F = Some specific activity with these people
            //
            // Note that activity A and activity D are different types of classifications: Activity A describes how to communicate better with people in general,
            // whereas Activity D describes communicating with a specific set of people. It's possible to do one, the other, both, or neither at once.
            // So, if the user hasn't yet decided that it's interesting to break C up into smaller sub-categories, then we limit our suggestions to the parent activities
            //
            // If, in the future, the user does add more ways to communicate face-to-face, then the user will have to add a few more categories, splitting it up into
            // one category of generic communication methods, and another category for communicating with each relevant group of people.
            // These categories could be named things like:
            // Communication // includes both generic and specific suggestions
            // Generic Communication Suggestions // only includes suggestions that don't include specific groups of people
            // Communicating with Group X
            // Talking Face-to-Face to Group X // is a child of Communicating with Group X
            //
            // Then, in the future, if the user just wants to communicate with anybody, the user can ask for a Communication suggestion
            // If the user wants to communicate with someone specific but not yet known to ActivityRecommender, the user can ask for a Generic Communication Suggestions
            // If the user wants to communicate with Group X, the user can ask for a suggestion from Communicating with Group X
            //
            // The user can still enter these activities lazily over time, though, as they become relevant, because if a certain category is too specific to enter at first,
            // ActivityRecommender will use the parent activity (because the user will have created the parent activity). The user will also be able to start to create
            // overlapping hierarchies, and if only one child has been entered, the parent will still be used because it's more generic and sufficiently clear

            // Start by finding all subcategories
            List<Activity> availableCandidateList = new List<Activity>();
            foreach (Activity candidate in this.FindAllSubCategoriesOf(fromCategory))
            {
                // filter out candidates that will never want to be suggested, like Problems, completed ToDos, and the root activity
                if (candidate.Suggestible)
                {
                    availableCandidateList.Add(candidate);
                }
            }
            // Set of candidates that could be acceptable to suggest (might include some candidates that are strictly less interesting than others)
            HashSet<Activity> availableCandidateFilter = null;
            if ((fromCategory != this.activityDatabase.RootActivity))
                availableCandidateFilter = new HashSet<Activity>(availableCandidateList);
            // Set of candidates that we will allow suggesting (will only include candidates that are not strictly less interesting than others)
            HashSet<Activity> interestingCandidates = new HashSet<Activity>(availableCandidateList);
            // Now filter out categories where their children are more interesting
            foreach (Activity candidate in availableCandidateList)
            {
                // Some categories might be strictly less interesting than other categories. Usually, child categories are more interesting than their parents
                if (candidate is Category)
                {
                    // Each activity is considered to be a subset of each of its parents.
                    // If an activity is a subset of a parent, then it must either equal the parent or be a strict subset of the parent. In either case, we should be
                    // able to identify which of the two is more interesting to suggest.
                    foreach (Activity parent in candidate.Parents)
                    {
                        if (availableCandidateFilter == null || availableCandidateFilter.Contains(parent))
                        {
                            Activity uniqueLeaf = parent.GetUniqueLeafDescendant();
                            if (uniqueLeaf != null && uniqueLeaf is Category)
                            {
                                // If this parent has exactly one leaf descendant category, and the user is considering entering new children for this parent, then:
                                // If we suggest the only child, then that could be weird if the user would be better off entering a new child activity and choosing that.
                                // If we suggest the parent, then that is still clear even if the user ends up choosing the only child anyway.
                                interestingCandidates.Remove(candidate);
                            }
                            else
                            {
                                // If the parent has multiple descendents, then the child is more specific than the parent in some meaningful
                                // way, so it would be more interesting to suggest the child
                                interestingCandidates.Remove(parent);
                            }
                        }
                    }
                }
            }
            // This returns activities in a potentially random order but that's ok
            // We intentionally process them in a random order later anyway
            return new List<Activity>(interestingCandidates);
        }
        public ActivitiesSuggestion MakeRecommendation(ActivityRequest request)
        {
            DateTime when = request.Date;
            Activity activityToBeat = this.activityDatabase.ResolveDescriptor(request.ActivityToBeat);

            List<Activity> candidates = this.GetActivitiesToConsider(request);
            TimeSpan? requestedProcessingTime = request.RequestedProcessingTime;

            DateTime processingStartTime = DateTime.Now;
            RatingsAnalysis ratingsCache = this.ratingsCacheFor(request.Date);
            UtilitiesAnalysis utilitiesCache = this.utilitiesCacheFor(request);

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
                this.EnsureCalculatedSuggestionValue(activityToBeat, request);
                ratingsCache.futureEfficiencies[activityToBeat] = this.Get_OverallEfficiency_ParticipationEstimate(activityToBeat, when);
                Prediction prediction = new Prediction(activityToBeat, Distribution.MakeDistribution(0, 0, double.MaxValue), when, "you asked for an activity at least as fun as this one");
                utilitiesCache.futureHappinessIfSuggested[activityToBeat] = prediction;
                utilitiesCache.suggestionValues[activityToBeat] = prediction;
                if (candidates.Contains(activityToBeat))
                {
                    candidates.Remove(activityToBeat);
                }
                // Here we add the activityToBeat as a candidate so that if activityToBeat is the best activity, then we still have give a suggestion
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

                // estimate how good it is for us to suggest this particular activity
                this.EnsureCalculatedSuggestionValue(candidate, request);
                bool better = false;
                if (request.Optimize == ActivityRequestOptimizationProperty.LONGTERM_EFFICIENCY)
                {
                    ratingsCache.futureEfficiencies[candidate] = this.Get_OverallEfficiency_ParticipationEstimate(candidate, when);
                }
                if (activityToBeat != null && utilitiesCache.utilities[candidate] < utilitiesCache.utilities[activityToBeat])
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
                                better = utilitiesCache.suggestionValues[candidate].Distribution.Mean >= utilitiesCache.suggestionValues[bestActivity].Distribution.Mean;
                                break;
                            case ActivityRequestOptimizationProperty.PARTICIPATION_PROBABILITY:
                                better = ratingsCache.participationProbabilities[candidate].Distribution.Mean >= ratingsCache.participationProbabilities[bestActivity].Distribution.Mean;
                                break;
                            case ActivityRequestOptimizationProperty.LONGTERM_EFFICIENCY:
                                better = ratingsCache.futureEfficiencies[candidate].Distribution.Mean >= ratingsCache.futureEfficiencies[candidate].Distribution.Mean;
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
                        System.Diagnostics.Debug.WriteLine("Candidate " + candidate + " with suggestion value " + utilitiesCache.suggestionValues[candidate].Distribution.Mean + " replaced " + bestActivity + " with suggestion value " + utilitiesCache.suggestionValues[bestActivity].Distribution.Mean + " as highest-value suggestion");
                    }
                    */
                    bestActivity = candidate;
                }
                if (activityToBeat != null)
                {
                    if (mostFunActivity == null || utilitiesCache.utilities[candidate] >= utilitiesCache.utilities[mostFunActivity])
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
                if (consideredCandidates.Count >= 3 && requestedProcessingTime != null)
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
            // some fallbacks in case no activity matched
            if (bestActivity == null)
            {
                bestActivity = mostFunActivity;
                if (bestActivity == null)
                    bestActivity = activityToBeat;
            }
            // After finding the activity with the highest expected rating, we need to check for other activities having high variance but almost-as-good values

            if (bestActivity == null)
                return null;
            // System.Diagnostics.Debug.WriteLine("bestActivity = " + bestActivity + ", suggesting " + bestActivityToPairWith);
            // If there was a pair of activities that could do strictly better than two of the best activity, then we must actually choose the second-best
            // If there was no such pair, then we just want to choose the best activity because no others could help
            // Remember that the reason the activity with second-highest rating might be a better choice is that it might have a higher variance
            return this.SuggestMultipleActivityOptions(new List<Activity>() { bestActivity }, request, consideredCandidates.Count, true);
        }
        private ActivitiesSuggestion SuggestMultipleActivityOptions(List<Activity> bestActivities, ActivityRequest request, int numActivitiesConsidered, bool tryComputeExpectedFeedback)
        {
            List<ActivitySuggestion> children = new List<ActivitySuggestion>();
            foreach (Activity activity in bestActivities)
            {
                children.Add(this.SuggestOneActivity(activity, request, numActivitiesConsidered, tryComputeExpectedFeedback));
            }
            return new ActivitiesSuggestion(children);
        }
        private ActivitySuggestion SuggestOneActivity(Activity activity, ActivityRequest request, int numActivitiesConsidered, bool tryComputeExpectedFeedback)
        {
            DateTime when = request.Date;
            DateTime now = DateTime.Now;
            ActivitySuggestion suggestion = new ActivitySuggestion(activity.MakeDescriptor());
            suggestion.CreatedDate = now;
            suggestion.StartDate = when;
            suggestion.NumActivitiesConsidered = numActivitiesConsidered;
            suggestion.EndDate = this.GuessParticipationEndDate(activity, when);
            suggestion.ParticipationProbability = this.EstimateParticipationProbability(activity, request.Date).Distribution.Mean;
            if (tryComputeExpectedFeedback)
            {
                ParticipationFeedback feedback = this.computeStandardParticipationFeedback(activity, when, when, false);
                if (feedback != null)
                    suggestion.ExpectedReaction = feedback.Summary;
            }
            double average = this.ActivityDatabase.RootActivity.Ratings.Mean;
            if (average == 0)
                average = 0.5;
            suggestion.PredictedScoreDividedByAverage = this.EstimateRating(activity, when).Distribution.Mean / average;
            Distribution thisHappiness = this.Get_OverallHappiness_ParticipationEstimate(activity, request).Distribution;
            Distribution otherHappiness = this.Get_OverallHappiness_ParticipationEstimate(this.activityDatabase.RootActivity, request).Distribution;
            if (thisHappiness.Mean < otherHappiness.Mean)
            {
                // The reason that we calculate whether we think the user's future happiness after doing this will be less than after simply doing something is:
                // We want to be able to encourage the user to search for and discover new things when appropriate.

                // If all of our specific suggestions are below average, then the user would probably be better off doing something random, because there's a decent chance that
                // doing something random could be just as good.
                // If we expect that doing something that is already known will be below average, then the user should choose something not already known.
                // Choosing something not already known also has the advantage that if it ends up being good, then that could be very informative.

                // What are the cases where it should be possible for us to expect this suggested activity to indicate lower future happiness than the root activity?

                // Causes that we like:
                // 1. Maybe we made a suggestion because making the suggestion is helpful even though doing the suggested activity is not
                //    Recall that we model the future-happiness-if-suggested and future-happiness-if-done separately
                //    If we make a suggestion and don't really intend for the user to do it, it's because we expect the user to interpret it as a suggestion to do something else
                //    For example, maybe when the user is investigating doing activity X they notice something that reminds them about activity Y and they do that instead
                // 2. Maybe the predictions for this activity are more sensitive to noise and/or to new data due to this activity being newer than the root activity
                //    If a bunch of undesirable things happened very recently (for example, the user skipped a few suggestions), that might have more of an effect on predictions for newer activities
                // 3. If this (suggesting trying something new) works well, perhaps child activities will be sensitive to skips and the root activity won't
                //    Perhaps, if the user often tries new things, then it will often be the case that skipping a child activity doesn't have much impact on overall happiness,
                //    because the user will just try something new later.
                //    However, if skipping a child activity is a strong signal that the user is unwilling to do that activity, then it could mean that if the user were to actually do that
                //    activity, that there would be some sort of problem and that the user might be less happy

                // Causes that we don't really like:
                // 3. Maybe we didn't consider all activities
                // 3.1. Maybe we're still warming up the cache and didn't have time to consider all activities
                // 3.2. Maybe we're doing an experiment. In that case it shouldn't matter, though
                // 4. The root activity has a higher density of datapoints per unit time than any other activity, so each interpolator box will be a smaller fraction of the total
                //    So, when predicting the future happiness based on the root activity, a smaller time window will be used

                // It would be nice for us to simply compare the predicted-future-happiness-after-doing-this to the average happiness (because that would be a lagging indicator of happiness
                // and could be used as a more stable baseline), but recent happiness scores are probably more relevant than very old happiness scores from years ago.
                // It could be nice for us to use a slightly laggy happiness indicator as a baseline
                //  Maybe we could make another interpolator for the root activity and ask that interpolator to use a larger window duration.
                //  Maybe we could just compute the average happiness from the last ~1 month and use that as a baseline
                suggestion.WorseThanRootActivity = true;
            }
            return suggestion;
        }
        public DateTime GuessParticipationEndDate(Activity activity, DateTime start)
        {
            ParticipationsSummary participationSummary = activity.SummarizeParticipationsBetween(new DateTime(), start);
            TimeSpan estimatedDuration;
            if (participationSummary.CumulativeIntensity.TotalSeconds > 0)
            {
                double typicalNumSeconds = Math.Exp(participationSummary.LogActiveTime.Mean);
                estimatedDuration = TimeSpan.FromSeconds(typicalNumSeconds);
            }
            else
            {
                // No data, so we guess that the user will spend 1 hour doing this activity
                estimatedDuration = TimeSpan.FromHours(1);
            }

            return start.Add(estimatedDuration);
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

            DateTime latestParticipationEnd = activity.LatestParticipationDate;
            double currentIdleSeconds = actualStart.Subtract(latestParticipationEnd).TotalSeconds;
            double randomSeconds = (currentIdleSeconds + averageIdleSeconds) * this.randomGenerator.NextDouble();
            TimeSpan randomIdleTime = TimeSpan.FromSeconds(randomSeconds);
            return latestParticipationEnd.Add(randomIdleTime);
        }

        // update the estimate of what rating the user would give to this activity now
        public Prediction EstimateRating(Activity activity, DateTime when)
        {
            // If we've already estimated the rating at this date, then just return what we calculated
            RatingsAnalysis recommendationsCache = this.ratingsCacheFor(when);
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

            return ratingPrediction;
        }

        public void EstimateUtility(Activity activity, ActivityRequest request)
        {
            this.EstimateRating(activity, request.Date);
            
            RatingsAnalysis ratingsCache = this.ratingsCacheFor(request.Date);
            Prediction ratingPrediction = ratingsCache.ratings[activity];
            Prediction probabilityPrediction = ratingsCache.participationProbabilities[activity];

            UtilitiesAnalysis utilitiesCache = this.utilitiesCacheFor(request);
            utilitiesCache.utilities[activity] = this.RatingAndProbability_Into_Value(ratingPrediction.Distribution, probabilityPrediction.Distribution.Mean, activity.MeanParticipationDuration, request.NumAcceptancesPerParticipation).Mean;
        }

        private Distribution RatingAndProbability_Into_Value(Distribution rating, double suggestedParticipation_probability, double meanParticipationDuration, int numAcceptancesPerParticipation)
        {
            // It's probably more accurate to assume that the user will make their own selection of an activity to do, but the user doesn't want us to model it like that
            // Because if we do model the possibility that the user will choose their own activity, then that can incentivize purposely bad suggestions to prompt the user to have to think of something
            double skipProbability = 1 - suggestedParticipation_probability;

            // Now compute the expected amount of wasted time
            double estimatedNumFutureSkips;
            if (skipProbability < 1)
                estimatedNumFutureSkips = (-1 + 1 / (1 - skipProbability));
            else
                estimatedNumFutureSkips = 1000;
            double averageWastedSecondsPerAcceptance = this.thinkingTime.Mean * estimatedNumFutureSkips;
            double averageWastedSecondsPerParticipation = averageWastedSecondsPerAcceptance * numAcceptancesPerParticipation;

            // Let p be the probability of skipping this activity. The expected number of skips then is p + p^2 ... = -1 + 1 + p + p^2... = -1 + 1 / (1 - p)
            // = -1 + 1 / (1 - (the probability that the user will skip the activity))
            // So the amount of waste is (the average length of a skip) * (-1 + 1 / (1 - (the probability that the user will skip the activity)))

            // reweight such that more rating data increases certainty, and more confidence in more skips will increases certainty too
            double weight = rating.Weight + estimatedNumFutureSkips;
            Distribution overallValue = rating.CopyAndReweightTo(meanParticipationDuration).Plus(Distribution.MakeDistribution(0, 0, averageWastedSecondsPerParticipation)).CopyAndReweightTo(weight);

            return overallValue;
        }

        // recompute the estime of how good it would be to suggest this activity now
        public Prediction EstimateFutureHappinessIfSuggested(Activity activity, ActivityRequest request)
        {
            this.EnsureRatingsAreAssociated();
            this.EnsureCalculatedSuggestionValue(activity, request);
            return this.currentUtilitiesCache.futureHappinessIfSuggested[activity];
        }

        // update the estimate of how good it would be to suggest this activity now, unless the computation is already up-to-date
        private void EnsureCalculatedSuggestionValue(Activity activity, ActivityRequest request)
        {
            UtilitiesAnalysis recommendationsCache = this.utilitiesCacheFor(request);
            // Now we estimate how useful it would be to suggest this activity to the user

            if (!recommendationsCache.suggestionValues.ContainsKey(activity))
            {
                // First we calculate how happy we expect the user to be after we suggest this activity
                DateTime when = request.Date;
                Prediction happinessPrediction = this.Get_OverallHappiness_SuggestionEstimate(activity, request);
                recommendationsCache.futureHappinessIfSuggested[activity] = happinessPrediction;
                Distribution happinessDistribution = happinessPrediction.Distribution;

                // Next we incorporate the possibility of learning something about the user and calculate a score about how good it would be to suggest this activity

                // The longer it has been since the user thought about this activity, the more that we can learn by suggesting this activity
                DateTime lastInteracted = activity.LatestInteractionDate;
                double idleSeconds = when.Subtract(lastInteracted).TotalSeconds;
                double idlenessWeight = this.weightedRatingSummarizer.getWeightInDuration(idleSeconds);

                double predictionMean = happinessDistribution.Mean;
                double predictionUncertainty = happinessDistribution.StdDev;
                Distribution suggestionValue = Distribution.MakeDistribution(predictionMean + predictionUncertainty * idlenessWeight / Math.Sqrt(activity.NumConsiderations + 1), happinessDistribution.StdDev, happinessDistribution.Weight);
                LabeledDistributionJustification justification = new LabeledDistributionJustification(suggestionValue, "Increased the score to discover whether this could be better than expected");


                Composite_SuggestionJustification compositeJustification = new Composite_SuggestionJustification(suggestionValue,
                    new List<Justification>() {
                        happinessPrediction.Justification,
                        justification,
                        new LabeledDistributionJustification(Distribution.MakeDistribution(idleSeconds, 0, 1), "You last considered this on " + lastInteracted),
                        new LabeledDistributionJustification(Distribution.MakeDistribution(activity.NumConsiderations, 0, 1), "How many times you have considered this")
                    });
                compositeJustification.Label = "How good I think it is to suggest this";

                recommendationsCache.suggestionValues[activity] = new Prediction(activity, suggestionValue, when, compositeJustification);
            }
        }

        // attempt to calculate the probability that the user would do this activity if we suggested it at this time
        public Prediction EstimateParticipationProbability(Activity activity, DateTime when)
        {
            RatingsAnalysis recommendationsCache = this.ratingsCacheFor(when);
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

        // gets the coordinates for predicting what the user's longterm happiness should be if the user does <participated> at <when>
        private LazyInputs getCoordinatesForParticipation(DateTime when, Activity participated)
        {
            return this.getCoordinatesForLongtermPrediction(when, participated, null);
        }

        // gets the coordinates for predicting what the user's longterm happiness should be if we suggest to the user to do <activity> at <when> and they dismiss our suggestion
        private LazyInputs getCoordinatesForSkip(DateTime when, Activity skipped)
        {
            // get all activities
            List<Activity> allActivities = this.getActivitiesForLongtermInterpolation();

            // Collect all of the coordinates
            List<LazyInputs> inputs = new List<LazyInputs>();
            // Note that even enumerating all of the coordinates takes too much memory, so we provide a few lazy getters that each know how to get lots of different coordinates
            List<LazyCoordinate> coordinates = new List<LazyCoordinate>();

            // How long it has been since a reference date
            coordinates.Add(new LazyProgressionValue(when, TimeProgression.AbsoluteTime));
            // How long it has been since the day started
            coordinates.Add(new LazyProgressionValue(when, TimeProgression.DayCycle));
            // How long it has been since the user considered this activity
            coordinates.Add(new LazyProgressionValue(when, skipped.IdlenessProgression));
            inputs.Add(new LazyInputList(coordinates));

            // The identities of the activity the user participated in
            inputs.Add(new ActivityInList_Inputs(skipped, allActivities));

            // How much the user has been doing each particular activity recently
            inputs.Add(new ParticipationInputs(when, null, allActivities));

            // The relative frequencies of Participations and Skips in each particular activity
            inputs.Add(new ConsiderationInputs(when, null, null, allActivities));

            // all of these coordinates, combined
            return new ConcatInputs(inputs);
        }

        // helper function for getCoordinatesForParticipation and getCoordinatesForSuggestion
        private LazyInputs getCoordinatesForLongtermPrediction(DateTime when, Activity participated, Activity suggested)
        {
            Activity considered = participated;
            if (considered == null)
                considered = suggested;
            // get all activities
            List<Activity> allActivities = this.getActivitiesForLongtermInterpolation();

            // Collect all of the coordinates
            List<LazyInputs> inputs = new List<LazyInputs>();
            // Note that even enumerating all of the coordinates takes too much memory, so we provide a few lazy getters that each know how to get lots of different coordinates
            List<LazyCoordinate> coordinates = new List<LazyCoordinate>();

            // How long it has been since a reference date
            coordinates.Add(new LazyProgressionValue(when, TimeProgression.AbsoluteTime));
            // How long it has been since the day started
            coordinates.Add(new LazyProgressionValue(when, TimeProgression.DayCycle));
            // How long it has been since the user considered this activity
            coordinates.Add(new LazyProgressionValue(when, considered.IdlenessProgression));
            inputs.Add(new LazyInputList(coordinates));

            // The identities of the activity the user participated in
            inputs.Add(new ActivityInList_Inputs(considered, allActivities));

            // How much the user has been doing each particular activity recently
            inputs.Add(new ParticipationInputs(when, participated, allActivities));

            // The relative frequencies of Participations and Skips in each particular activity
            inputs.Add(new ConsiderationInputs(when, participated, null, allActivities));

            // all of these coordinates, combined
            return new ConcatInputs(inputs);
        }


        private List<Activity> getActivitiesForLongtermInterpolation()
        {
            if (activitiesForLongtermInterpolation == null)
                activitiesForLongtermInterpolation = new List<Activity>(this.activityDatabase.AllActivities);
            return activitiesForLongtermInterpolation;
        }

        private Distribution Interpolate_LongtermValue_If_Participated(Activity activity, DateTime when)
        {
            LazyInputs coordinates = this.getCoordinatesForParticipation(when, activity);
            Distribution defaultPrediction = Distribution.MakeDistribution(0.5, 0.125, 2);
            Distribution interpolation = new Distribution(this.longTerm_participationValue_interpolator.Interpolate(coordinates));
            Distribution result = interpolation.Plus(defaultPrediction);
            return result;
        }

        // returns a Prediction of what the user's longterm happiness will be after having participated in the given activity at the given time
        public Prediction Get_OverallHappiness_ParticipationEstimate(Activity activity, ActivityRequest request)
        {
            DateTime when = request.Date;

            // The activity might use its estimated rating to predict the overall future value, so we must update the rating now
            this.EstimateRating(activity, request.Date);

            List<Prediction> predictions = new List<Prediction>();

            // When there is little data, we focus on the fact that doing the activity will probably be as good as doing that activity (or its parent activities)
            // When there is a medium amount of data, we focus on the fact that doing the activity will probably make the user as happy as having done the activity in the past

            Prediction shortTerm_prediction = this.CombineRatingPredictions(activity.Get_ShortTerm_RatingEstimates(when));
            shortTerm_prediction.Justification.Label = "How happy I expect you to be while doing " + activity.Name;
            double shortWeight = Math.Pow(activity.NumParticipations + 1, 0.5);
            shortTerm_prediction.Distribution = shortTerm_prediction.Distribution.CopyAndReweightTo(shortWeight);
            predictions.Add(shortTerm_prediction);

            double mediumWeight = activity.NumParticipations + 1;
            Distribution ratingDistribution = this.Interpolate_LongtermValue_If_Participated(activity, when);
            Distribution mediumTerm_distribution = ratingDistribution.CopyAndReweightTo(mediumWeight);
            predictions.Add(new Prediction(activity, mediumTerm_distribution, when, new InterpolatorSuggestion_Justification(activity, mediumTerm_distribution, null)));

            if (activity.NumParticipations < 40)
            {
                // before we have much data, we predict that the happiness after doing this activity resembles that after doing its parent activities
                foreach (Activity parent in activity.ParentsUsedForPrediction)
                {
                    Prediction parentPrediction = this.Get_OverallHappiness_ParticipationEstimate(parent, request);
                    parentPrediction.Distribution = parentPrediction.Distribution.CopyAndReweightTo(8);
                    predictions.Add(parentPrediction);
                }
            }

            Prediction prediction = this.CombineRatingPredictions(predictions);
            prediction.Justification.Label = "How happy I expect you to be after doing " + activity.Name;

            return prediction;
        }

        public Distribution compute_longtermValue_increase_in_days(Distribution chosenValue, DateTime when, DateTime baselineEnd)
        {
            if (chosenValue.Weight <= 0)
                return new Distribution();

            Distribution baseValue = this.longTerm_participationValue_interpolator.AverageUntil(baselineEnd);

            Distribution bonusInDays = new Distribution();
            // relWeight(x) = 2^(-x/halflife)
            // integral(relWeight) = -(log(e)/log(2))*halfLife*2^(-x/halflife)
            // totalWeight = (log(e)/log(2))*halflife
            // absWeight(x) = relWeight(x) / totalWeight
            // absWeight(x) = 2^(-x/halflife) / ((log(e)/log(2))*halflife)
            // absWeight(0) = log(2)/log(e)/halflife = log(2)/halflife
            double weightOfThisMoment = Math.Log(2) / CommonPreferences.Instance.HalfLife.TotalDays;
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

        private Distribution interpolate_longtermValue_if_skipped(Activity activity, DateTime when)
        {
            LazyInputs inputs = this.getCoordinatesForSkip(when, activity);
            Distribution defaultPrediction = Distribution.MakeDistribution(0.5, 0.125, 2);
            Distribution interpolation = new Distribution(this.longTerm_skipValue_interpolator.Interpolate(inputs));
            Distribution result = interpolation.Plus(defaultPrediction);
            return result;
        }

        // returns a Prediction of the value of suggesting the given activity at the given time
        private Prediction Get_OverallHappiness_SuggestionEstimate(Activity activity, ActivityRequest request)
        {
            DateTime when = request.Date;
            List<Prediction> predictions = new List<Prediction>();
            // The activity might use its estimated rating to predict the overall future value, so we must update the rating now
            this.EstimateUtility(activity, request);

            // Estimate participation probability
            Prediction probabilityPrediction = this.EstimateParticipationProbability(activity, request.Date);
            double participationProbability = probabilityPrediction.Distribution.Mean;

            // Estimate how happy the user will be if they do this
            Prediction participationPrediction = this.Get_OverallHappiness_ParticipationEstimate(activity, request);
            // ask the interpolator how happy the user is after this is skipped
            Distribution skippedDistribution = this.interpolate_longtermValue_if_skipped(activity, when);
            // compute weights
            double totalWeight = participationPrediction.Distribution.Weight * participationProbability + skippedDistribution.Weight * (1 - participationProbability);
            double participationWeight = totalWeight * participationProbability;
            double skipWeight = totalWeight - participationWeight;

            // create prediction for how happy the user will be after doing this
            Distribution futureHappinessIfParticipated = participationPrediction.Distribution.CopyAndReweightTo(participationWeight);
            InterpolatorSuggestion_Justification participationFuture_justification = new InterpolatorSuggestion_Justification(
                activity, participationPrediction.Distribution, null
            );
            participationFuture_justification.Label = "How happy I expect you to be after doing " + activity.Name;
            predictions.Add(new Prediction(activity, futureHappinessIfParticipated, when, participationFuture_justification));

            // create prediction for how happy the user will be after skipping this
            Distribution futureHappinessIfSkipped = skippedDistribution.CopyAndReweightTo(skipWeight);
            InterpolatorSuggestion_Justification longtermJustification = new InterpolatorSuggestion_Justification(
                activity, futureHappinessIfSkipped, null);
            longtermJustification.Label = "How happy you have been after skipping " + activity.Name;
            predictions.Add(new Prediction(activity, futureHappinessIfSkipped, when, longtermJustification));

            // combine all the components and return final result
            Prediction prediction = this.CombineRatingPredictions(predictions);
            prediction.Distribution = prediction.Distribution.CopyAndReweightTo(prediction.Distribution.Weight);
            prediction.Justification.Label = "How happy I expect you to be after " + activity.Name + " is suggested";
            return prediction;
        }

        // returns a Prediction of what the user's efficiency will be after having participated in the given activity at the given time
        public Prediction Get_OverallEfficiency_ParticipationEstimate(Activity activity, DateTime when)
        {
            LazyInputs coordinates = this.getCoordinatesForParticipation(when, activity);
            Distribution distribution = new Distribution(this.longTerm_efficiency_interpolator.Interpolate(coordinates));
            return new Prediction(activity, distribution, when, "How efficient you tend to be after doing " + activity.Name);
        }


        // returns a bunch of thoughts telling why <activity> was last rated as it was
        public ActivitySuggestion_Explanation JustifySuggestion(ActivitySuggestion activitySuggestion)
        {
            // get the happiness values computed for the suggested activity
            // which activity we suggested
            Activity activity = this.ActivityDatabase.ResolveDescriptor(activitySuggestion.ActivityDescriptor);
            // when we made the suggestion
            DateTime when = this.currentUtilitiesCache.ApplicableDate;
            ActivityRequest request = new ActivityRequest(activity, null, when);
            request.NumAcceptancesPerParticipation = this.currentUtilitiesCache.NumAcceptancesPerParticipation;

            // how good we thought it was to suggest this activity
            this.EstimateFutureHappinessIfSuggested(activity, request);
            Prediction ourValue = this.currentUtilitiesCache.suggestionValues[activity];
            ActivitySuggestion_Explanation explanation = new ActivitySuggestion_Explanation();
            explanation.Score = this.currentRatingsCache.ratings[activity].Distribution.Mean;
            explanation.Suggestion = activitySuggestion;
            explanation.SuggestionValue = ourValue.Distribution.Mean;
            explanation.Reasons = new List<Justification>() { ourValue.Justification };
            return explanation;
        }
        public Prediction CombineRatingPredictions(IEnumerable<Prediction> predictions)
        {
            List<Distribution> distributions = new List<Distribution>();
            DateTime date = new DateTime(0);
            Activity activity = null;
            List<Justification> justifications = new List<Justification>();
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
            List<Justification> justifications = new List<Justification>();
            foreach (Prediction prediction in predictions)
            {
                distributions.Add(prediction.Distribution);
                if (prediction.ApplicableDate.CompareTo(date) > 0)
                    date = prediction.ApplicableDate;
                activity = prediction.Activity;
                justifications.Add(prediction.Justification);
            }
            Distribution distribution = this.CombineProbabilityDistributions(distributions);
            Justification justification = new Composite_SuggestionJustification(distribution, justifications);
            justification.Label = "Overall participation probability";
            Prediction result = new Prediction(activity, distribution, date, justification);
            return result;
        }
        public Distribution CombineProbabilityDistributions(IEnumerable<Distribution> distributions)
        {
            return this.CombineRatingDistributions(distributions);
        }
        public Activities_HappinessContributions GetMostSignificantRecentActivities(DateTime windowStart, int maxCount)
        {
            // For each activity, compute its total contribution in change of happiness since windowStart
            double averageRating = this.ActivityDatabase.RootActivity.Ratings.Mean;
            List<ActivityHappinessContribution> contributions = new List<ActivityHappinessContribution>();
            foreach (Activity activity in this.ActivityDatabase.AllActivities)
            {
                double contributionInSeconds = 0;
                List<Participation> participations = activity.getParticipationsSince(windowStart);
                foreach (Participation participation in participations)
                {
                    AbsoluteRating rating = participation.GetEstimatedRating();
                    if (rating != null)
                    {
                        double score = rating.Score;
                        double difference = score - averageRating;
                        double duration = participation.Duration.TotalSeconds;
                        contributionInSeconds += difference * duration;
                    }
                }
                ActivityHappinessContribution contribution = new ActivityHappinessContribution();
                contribution.Activity = activity;
                contribution.TotalHappinessIncreaseInSeconds = contributionInSeconds;
                contributions.Add(contribution);
            }
            // sort
            ActivityHappinessContributionComparer comparer = new ActivityHappinessContributionComparer();
            contributions.Sort(comparer);
            // select the highest and lowest few results
            List<ActivityHappinessContribution> bests = new List<ActivityHappinessContribution>();
            List<ActivityHappinessContribution> worsts = new List<ActivityHappinessContribution>();
            int lowIndex = 0;
            int highIndex = contributions.Count - 1;
            bool activityRemains = true;
            while (true)
            {
                // check whether we're done
                if (highIndex < lowIndex)
                {
                    // No activities left to add
                    activityRemains = false;
                    break;
                }
                if (bests.Count + worsts.Count >= maxCount)
                {
                    // We've found as many activities as was requested
                    break;
                }
                // add next most extreme activity
                if (bests.Count <= worsts.Count)
                {
                    this.addContribution(bests, contributions[highIndex]);
                    highIndex--;
                }
                else
                {
                    this.addContribution(worsts, contributions[lowIndex]);
                    lowIndex++;
                }
                // determine whether one listed activity is an ancestor of another listed activity, and if so, remove it
            }

            Activities_HappinessContributions results = new Activities_HappinessContributions();
            // put the best contributions first
            results.Best = bests;
            results.Worst = worsts;
            results.ActivitiesRemain = activityRemains;

            return results;
        }

        // adds newContribution to existingContribution and removes any duplicate ancestors
        private void addContribution(List<ActivityHappinessContribution> existingContributions, ActivityHappinessContribution newContribution)
        {
            // determine whether this activity is an ancestor of another existing activity
            Activity newActivity = newContribution.Activity;
            foreach (ActivityHappinessContribution other in existingContributions)
            {
                foreach (Activity ancestor in this.FindAllSupercategoriesOf(other.Activity))
                {
                    if (ancestor == newActivity)
                    {
                        // this activity is already included; no need to add it
                        return;
                    }
                }
            }
            // determine whether any existing activities are ancestors of this new activity
            List<Activity> ancestors = this.FindAllSupercategoriesOf(newActivity);
            for (int i = existingContributions.Count - 1; i >= 0; i--)
            {
                if (ancestors.Contains(existingContributions[i].Activity))
                {
                    existingContributions.RemoveAt(i);
                }
            }
            existingContributions.Add(newContribution);
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
        public void Update_RatingSummaries(int numGroups)
        {
            if (numGroups > 0)
            {
                this.longTerm_skipValue_interpolator.UpdateMany(numGroups);
                this.longTerm_participationValue_interpolator.UpdateMany(numGroups);
                this.longTerm_efficiency_interpolator.UpdateMany(numGroups);
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

            Activity activity = this.ActivityDatabase.ResolveDescriptor(newParticipation.ActivityDescriptor);
            activity.AddParticipation(newParticipation);

            this.weightedRatingSummarizer.AddParticipationIntensity(newParticipation.StartDate, newParticipation.EndDate, 1);

            Rating rating = newParticipation.Rating;
            if (rating != null)
            {
                this.PutRatingInMemory(rating);
            }

            if (!newParticipation.Hypothetical)
            {
                this.updateExperimentsWithNewParticipation(newParticipation);
            }
        }

        public void PutCommentInMemory(ParticipationComment comment)
        {
            Activity activity = this.ActivityDatabase.ResolveDescriptor(comment.ActivityDescriptor);
            activity.AddComment(comment);
        }
        // updates any existing experiments with information from newParticipation
        private void updateExperimentsWithNewParticipation(Participation newParticipation)
        {
            Activity activity = this.ActivityDatabase.ResolveDescriptor(newParticipation.ActivityDescriptor);
            if (this.experimentToUpdate.ContainsKey(activity) && newParticipation.EffectivenessMeasurement != null)
            {
                string metricName = newParticipation.EffectivenessMeasurement.Metric.Name;
                Dictionary<string, PlannedExperiment> experimentsForThisActivity = this.experimentToUpdate[activity];
                if (experimentsForThisActivity.ContainsKey(metricName))
                {
                    PlannedExperiment experiment = experimentsForThisActivity[metricName];
                    if (experiment.Started)
                    {
                        // Found the second participation in the experiment, so mark the experiment as complete
                        this.numCompletedExperiments++;
                        if (newParticipation.RelativeEfficiencyMeasurement != null)
                        {
                            // If we were able to compute an efficiency for this second participation, then we must have an efficiency for the matching first participation too
                            // (If this participation updated an experiment without computing an efficiency, it's possible that both participations failed to complete their metrics)

                            // Find the earlier participation and tell it about its measured efficiency too
                            Activity earlierActivity = this.activityDatabase.ResolveDescriptor(experiment.Earlier.ActivityDescriptor);
                            Metric earlierMetric = earlierActivity.MetricForName(experiment.Earlier.MetricName);
                            experiment.FirstParticipation.setRelativeEfficiencyMeasurement(newParticipation.RelativeEfficiencyMeasurement.Earlier, earlierMetric);

                            newParticipation.RelativeEfficiencyMeasurement.Experiment = experiment;
                            experiment.FirstParticipation.RelativeEfficiencyMeasurement.Experiment = experiment;
                            experiment.SecondParticipation = newParticipation;
                            experiment.SecondParticipation.RelativeEfficiencyMeasurement.Experiment = experiment;
                        }
                    }
                    else
                    {
                        // found the first participation in the experiment; no longer have to update this experiment if this Activity gets completed
                        // (and we can add this Activity+Metric into a new Experiment)
                        experiment.FirstParticipation = newParticipation;
                        this.numExperimentStartParticipations++;
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
        // Adds the given ActivitySkip and marks it for assignment to specific activities later
        public void PutSkipInMemory(ActivitySkip newSkip)
        {
            this.unappliedSkips.Add(newSkip);
            this.receivedSkip(newSkip);
        }

        // adds the given ActivitySkip and cascades it to any relevant Activity
        public void ApplySkip(ActivitySkip newSkip)
        {
            this.receivedSkip(newSkip);
            this.CascadeSkip(newSkip);
        }
        // updates some information when we receive a skip
        private void receivedSkip(ActivitySkip newSkip)
        {
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
            if (newRequest.FromCategory != null)
            {
                this.activityDatabase.RequestedActivityFromCategory = true;
                Activity activity = this.activityDatabase.ResolveDescriptor(newRequest.FromCategory);
                activity.EverRequestedFromDirectly = true;
            }
            if (newRequest.ActivityToBeat != null)
                this.activityDatabase.RequestedActivityAtLeastAsGoodAsOther = true;
        }
        // tells the Engine about an ActivitySuggestion that wasn't already in memory (but may have been stored on disk)
        public void PutSuggestionInMemory(ActivitiesSuggestion suggestion)
        {
            foreach (ActivitySuggestion child in suggestion.Children)
            {
                this.PutSuggestionInMemory(child);
            }
        }
        public void PutSuggestionInMemory(ActivitySuggestion suggestion)
        {
            this.DiscoveredSuggestion(suggestion);
            this.unappliedSuggestions.Add(suggestion);
        }
        // tells the Engine about a new ActivitySuggestion that was just created
        public void ApplySuggestion(ActivitiesSuggestion suggestion)
        {
            foreach (ActivitySuggestion child in suggestion.Children)
            {
                this.CascadeSuggestion(child);
            }
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
        public RelativeRating MakeRelativeRating(ActivityDescriptor activity, DateTime when, double scale, Participation otherParticipation)
        {
            // Create activities if missing
            if (this.requiresFullUpdate)
                this.FullUpdate();
            // Compute updated rating estimates for these activities
            Activity thisActivity = this.activityDatabase.ResolveDescriptor(activity);
            Activity otherActivity = this.activityDatabase.ResolveDescriptor(activity);
            ActivityRequest request = new ActivityRequest(when);
            this.EstimateFutureHappinessIfSuggested(thisActivity, request);
            this.EstimateFutureHappinessIfSuggested(otherActivity, request);
            this.MakeRecommendation(when);

            // make an AbsoluteRating for the Activity (leave out date + activity because it's implied)
            AbsoluteRating thisRating = new AbsoluteRating();
            thisRating.Date = when;
            thisRating.ActivityDescriptor = activity;
            Distribution thisPrediction = this.EstimateRating(thisActivity, when).Distribution;

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
            if (!experiment.Started)
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
            System.Diagnostics.Debug.WriteLine("Predicted efficiency = " + predictedEfficiency1 + " for " + activity1.Name + " at " + participation1.StartDate);
            // TODO: when predicting the efficiency of participation2, don't use lots of extra prediction factors, just use the timestamp
            // because although the first participation was chosen randomly, the second wasn't chosen from the same pool
            // (For example, the user might tend to prefer to work on easier tasks in the evening, and so if participation2.StartDate is in the evening,
            // then the user may have only consented to activity2 because the user thought activity2 sounded sufficiently easy.
            // If we use time of day (or any other predictor, really) as a prediction factor, then it might cause us to draw the (incorrect) conclusion
            // that that time of day results in more eficiency)
            double predictedEfficiency2 = this.PredictEfficiency(activity2, participation2.StartDate).Mean;
            System.Diagnostics.Debug.WriteLine("Predicted efficiency = " + predictedEfficiency2 + " for " + activity2.Name + " at " + participation2.StartDate);
            System.Diagnostics.Debug.WriteLine("Making completion efficiency measurement for " + activity1.Name + " and " + activity2.Name);

            // now calculate efficiency
            System.Diagnostics.Debug.WriteLine("Duration1 = " + duration1);
            System.Diagnostics.Debug.WriteLine("Duration2 = " + duration2);
            double totalEffectiveness = duration1 * predictedEfficiency1 + duration2 * predictedEfficiency2;
            System.Diagnostics.Debug.WriteLine("NumSuccess1 = " + numSuccesses1);
            double successFraction1 = numSuccesses1 / (numSuccesses1 + numSuccesses2);
            System.Diagnostics.Debug.WriteLine("NumSuccess2 = " + numSuccesses2);
            double successFraction2 = numSuccesses2 / (numSuccesses1 + numSuccesses2);

            double weight1 = successFraction1 * predictedDifficulty1;
            System.Diagnostics.Debug.WriteLine("weight1 = " + weight1);
            double weight2 = successFraction2 * predictedDifficulty2;
            System.Diagnostics.Debug.WriteLine("weight2 = " + weight2);
            double totalWeight = weight1 + weight2;

            double updatedEffectiveness1 = totalEffectiveness * weight1 / totalWeight;
            System.Diagnostics.Debug.WriteLine("updatedEffectiveness1 = " + updatedEffectiveness1);
            double updatedEffectiveness2 = totalEffectiveness * weight2 / totalWeight;
            System.Diagnostics.Debug.WriteLine("updatedEffectiveness2 = " + updatedEffectiveness2);

            double updatedEfficiency1 = updatedEffectiveness1 / duration1;
            System.Diagnostics.Debug.WriteLine("updatedEfficiency1 = " + updatedEfficiency1);
            double updatedEfficiency2 = updatedEffectiveness2 / duration2;
            System.Diagnostics.Debug.WriteLine("updatedEfficiency2 = " + updatedEfficiency2);
            // We want to be able to measure it accurately if the user becomes more efficient after one or more points in time.
            // If the second participation is after the user's efficiency improvement and the first is before it, then only the second will be improved.
            // This means that if the user took a different amount of time doing each of these participations, then we want to assign all of the difference to the second.
            // If we split the difference among the two participations, then it will make the pre tasks completed right before the improvement look a little bit worse, and the
            // post tasks completed right after the improvement look only a little bit better (about half as much as they should).
            // We can only do this if the first participation was successful, though, to avoid division by zero.
            if (updatedEfficiency1 > 0)
            {
                // both participations succeeded
                double ratio = updatedEfficiency2 / updatedEfficiency1;
                updatedEfficiency1 = predictedEfficiency1;
                updatedEfficiency2 = updatedEfficiency1 * ratio;
                System.Diagnostics.Debug.WriteLine("returned updatedEfficiency1 to " + updatedEfficiency1);
                System.Diagnostics.Debug.WriteLine("rescaled updatedEfficiency2 to " + updatedEfficiency2);
            }

            double updatedWeight1 = numSuccesses1 + numSuccesses2;
            double updatedWeight2 = updatedWeight1;
            // Why the efficiencies are computed this way:
            // Note that duration1*updatedEfficiency1+duration2*updatedEfficiency2=totalEffectiveness=duration1*predictedEfficiency1+duration2*predictedEfficiency2, so (weighted) average estimated efficiency is conserved
            //  Note that numSuccesses1 is 0 or 1, and numSucesses2 is 0 or 1
            // Note that doubling predictedDifficulty[[1,2]] doesn't change updatedEfficiency[[1,2]]
            //  In other words, having poor difficulty estimates doesn't bias the efficiency measurements (although using poor difficulty estimates does add variance to the efficiency measurements)
            // Note that spending longer on duration1 than on duration2 makes weight1 smaller than weight2, which makes updatedEfficiency2 larger than updatedEfficiency1
            // Note that doubling duration[i] and predictedDifficulty[i] leaves the weights (and the ratios of the updated efficiencies) unchanged
            // Note that if numSuccesses1 == 0, then updatedEfficiency2 = totalEffectiveness/duration2 = predictedEfficiency1*duration1/duration2 + predictedEfficiency2, which is more than predictedEfficiency2
            // Note that if this process is run twice, once with two successes and once with numSuccesses1=0,numSuccesses2=1, then assuming everything else is identical:
            //  The first time, updatedEfficiency1=updatedEfficiency2=1, updatedWeight1=updatedWeight2=2
            //  The second time, updatedEfficiency1=0, updatedEfficiency2=2, updatedWeight1=updatedWeight2=1
            //  average(updatedEfficiency1)=2/3, average(updatedEfficiency2)=4/3 = 2*average(updatedEfficiency1) as desired
            // Note that if predictedDifficulty1 = 1000, duration1 = predictedDifficulty1/2, predictedDifficulty2 = 1, duration2 = 1, then
            //  the total effectiveness is dominated by predictedDifficulty1 and duration1; predictedDifficulty2 and duration2 are so small as to be almost negligible
            //  Also note that in this case, updatedEfficency1 is almost predictedDifficulty1 / duration1, so almost 2

            // lastly, assemble the results
            RelativeEfficiencyMeasurement measurement1 = new RelativeEfficiencyMeasurement(participation1, Distribution.MakeDistribution(updatedEfficiency1, 0, updatedWeight1));
            RelativeEfficiencyMeasurement measurement2 = new RelativeEfficiencyMeasurement(participation2, Distribution.MakeDistribution(updatedEfficiency2, 0, updatedWeight2));
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
                    // not enough activities for a meaningful experiment
                    string message = "";
                    if (!this.HasInitiatedExperiment)
                    {
                        // The user never ran an experiment, so explain what an experiment is
                        message += "This screen is where you will be able to start an experiment for measuring your efficiency. The way it works is you find some specific, " +
                            "measurable tasks that you want to do, and then I help you choose one at random to do. If you repeat this enough times, you can observe " +
                            "how your efficiency changes over time.\n";
                    }
                    int numExtraRequiredActivities = minTotalPoolSize - numActivitiesToChooseFromTotal;
                    message += "Don't have enough activities having metrics to run another experiment. Go back and create " + numExtraRequiredActivities + " more ";
                    if (numExtraRequiredActivities == 1)
                        message += "activity of type ToDo and/or add " + numExtraRequiredActivities + " Metric to another Activity!";
                    else
                        message += "activities of type ToDo and/or add Metrics to " + numExtraRequiredActivities + " more Activities!";
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
            request.NumAcceptancesPerParticipation = 3;

            ActivitiesSuggestion activitySuggestion = this.MakeRecommendation(request);
            Activity bestActivity = this.activityDatabase.ResolveDescriptor(activitySuggestion.Children[0].ActivityDescriptor);

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
            return new SuggestedMetric_Metadata(new SuggestedMetric(plannedMetric, this.SuggestOneActivity(bestActivity, request, numActivitiesToChooseFromNow, false), userGuidedThisSuggestion), -1);
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
            // We don't rely on the user to estimate the absolute success rate of the two tasks, because it's expected that the user will tend to forget requirements or be unaware of requirements,
            // and therefore underestimate task difficulties.
            double ourTotalEstimatedSuccessRate = a.DifficultyEstimate.EstimatedSuccessesPerSecond_WithoutUser +
                b.DifficultyEstimate.EstimatedSuccessesPerSecond_WithoutUser;

            // However, we do rely on the user to estimate the relative success rate of two tasks, because we don't really have much information about the difficulty of the tasks
            double userTotalEstimatedSuccessRate = a.DifficultyEstimate.EstimatedRelativeSuccessRate_FromUser + b.DifficultyEstimate.EstimatedRelativeSuccessRate_FromUser;

            // Renormalize user estimates so they total 1
            double aWeight, bWeight;
            if (userTotalEstimatedSuccessRate > 0)
            {
                aWeight = a.DifficultyEstimate.EstimatedRelativeSuccessRate_FromUser / userTotalEstimatedSuccessRate;
                bWeight = b.DifficultyEstimate.EstimatedRelativeSuccessRate_FromUser / userTotalEstimatedSuccessRate;
            }
            else
            {
                aWeight = bWeight = 0.5;
            }

            // allocate our estimated success rate according to the user ratios
            a.DifficultyEstimate.EstimatedSuccessesPerSecond = ourTotalEstimatedSuccessRate * aWeight;
            b.DifficultyEstimate.EstimatedSuccessesPerSecond = ourTotalEstimatedSuccessRate * bWeight;
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
            if (experiment.Earlier.DifficultyEstimate.NumHarders > 0 || experiment.Earlier.DifficultyEstimate.NumEasiers > 0)
            {
                // old format; can't recompute difficulty
                return;
            }
            experiment.Earlier.DifficultyEstimate.EstimatedSuccessesPerSecond_WithoutUser = this.EstimateDifficulty_WithoutUser(this.ActivityDatabase.ResolveDescriptor(experiment.Earlier.ActivityDescriptor));
            experiment.Later.DifficultyEstimate.EstimatedSuccessesPerSecond_WithoutUser = this.EstimateDifficulty_WithoutUser(this.ActivityDatabase.ResolveDescriptor(experiment.Later.ActivityDescriptor));

            this.incorporateUserDifficultyEstimates(experiment.Earlier, experiment.Later);
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

        public bool HasInitiatedExperiment
        {
            get
            {
                return this.experimentToUpdate.Count > 0 || this.numCompletedExperiments > 0;
            }
        }
        public int NumCompletedExperiments
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
                return this.NumCompletedExperiments + this.numExperimentStartParticipations;
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
                List<Activity> activities = new List<Activity>();
                foreach (Activity candidate in this.ActivityDatabase.AllActivities)
                {
                    // ratings for the root activity aren't particularly interesting
                    if (candidate == this.ActivityDatabase.RootActivity)
                        continue;
                    // each ToDo only gets done once and isn't super exciting to share
                    if (candidate is ToDo)
                        continue;
                    activities.Add(candidate);
                }
                
                activities.Remove(this.ActivityDatabase.RootActivity);
                activities.Sort(new ActivityAverageScoreComparer());
                activities.Reverse();
                return activities;
            }
        }

        private Distribution compute_estimatedRating_ratio(Activity chosenActivity, DateTime startDate)
        {
            ActivityRequest request = new ActivityRequest(startDate);
            Activity rootActivity = this.ActivityDatabase.RootActivity;
            this.EstimateFutureHappinessIfSuggested(chosenActivity, request);
            Prediction prediction = this.EstimateRating(chosenActivity, startDate);
            return this.compute_estimatedRating_ratio(prediction.Distribution, rootActivity.Ratings);
        }
        private Distribution compute_estimatedRating_ratio(Activity chosenActivity)
        {
            Activity rootActivity = this.ActivityDatabase.RootActivity;
            return this.compute_estimatedRating_ratio(chosenActivity.Ratings, rootActivity.Ratings);
        }
        private Distribution compute_estimatedRating_ratio(Distribution value, Distribution rootValue)
        {
            Distribution expectedShortermRating = value;
            double overallAverageRating = rootValue.Mean;
            Distribution shorttermRatio = expectedShortermRating.CopyAndStretchBy(1.0 / overallAverageRating);

            return shorttermRatio;
        }

        // given an activity and a DateTime for its Participation to start, estimates the change in longterm happiness (measured in days) caused by doing it
        private Distribution compute_longtermValue_increase(Activity chosenActivity, DateTime startDate, DateTime baselineEnd)
        {
            ActivityRequest request = new ActivityRequest(startDate);

            Distribution thisValue = this.Get_OverallHappiness_ParticipationEstimate(chosenActivity, request).Distribution;

            Distribution increase = this.compute_longtermValue_increase_in_days(thisValue, startDate, baselineEnd);
            return increase;
        }

        // given an activity and a DateTime for its Participation to start, estimates the change in efficiency (measured in hours) in the near future caused by doing it
        private Distribution computeEfficiencyIncrease(Activity chosenActivity, DateTime startDate, DateTime baselineEnd)
        {
            Distribution previous = this.Get_OverallEfficiency_ParticipationEstimate(chosenActivity, startDate).Distribution;
            Distribution baseValue = this.longTerm_efficiency_interpolator.AverageUntil(baselineEnd);
            Distribution increase = this.computeEfficiencyIncrease(baseValue, previous);
            return increase;
        }
        private Distribution computeEfficiencyIncrease(Distribution baseValue, Distribution endValue)
        {
            if (endValue.Weight <= 0)
                return Distribution.Zero;
            Distribution chosenValue = endValue.CopyAndReweightTo(1);

            Distribution bonusInHours = Distribution.Zero;
            // relWeight(x) = 2^(-x/halflife)
            // integral(relWeight) = -(log(e)/log(2))*halfLife*2^(-x/halflife)
            // totalWeight = (log(e)/log(2))*halflife
            // absWeight(x) = relWeight(x) / totalWeight
            // absWeight(x) = 2^(-x/halflife) / ((log(e)/log(2))*halflife)
            // absWeight(0) = log(2)/log(e)/halflife = log(2)/halflife
            double weightOfThisMoment = Math.Log(2) / CommonPreferences.Instance.EfficiencyHalflife.TotalHours;
            if (baseValue.Mean > 0)
            {
                Distribution combined = baseValue.Plus(chosenValue);
                double overallAverage = combined.Mean;

                double relativeImprovement = (chosenValue.Mean - baseValue.Mean) / overallAverage;
                double relativeVariance = chosenValue.Variance / (overallAverage * overallAverage);
                Distribution difference = Distribution.MakeDistribution(relativeImprovement, Math.Sqrt(relativeVariance), 1);

                bonusInHours = difference.CopyAndStretchBy(1.0 / weightOfThisMoment);
            }
            return bonusInHours;
        }

        private bool get_wasSuggested(ActivityDescriptor activityDescriptor)
        {
            Activity activity = this.activityDatabase.ResolveDescriptor(activityDescriptor);
            DateTime? lastSuggested = activity.LatestSuggestionDate;
            if (lastSuggested == null)
                return false;
            DateTime latestInteraction = this.LatestInteractionDate;
            if (lastSuggested.Value.CompareTo(latestInteraction) < 0)
            {
                // Something happened more recently than our suggestion
                return false;
            }
            // the last thing we did was suggest this activity
            return true;
        }
        public ParticipationFeedback computeStandardParticipationFeedback(Activity chosenActivity, DateTime startDate, DateTime endDate, bool alreadyHappened = true)
        {
            this.ApplyParticipationsAndRatings();
            DateTime comparisonDate = this.chooseRandomBelievableParticipationStart(chosenActivity, startDate);
            if (comparisonDate.CompareTo(startDate) == 0)
            {
                // not enough data
                return null;
            }

            Distribution comparisonBonusInDays = this.compute_longtermValue_increase(chosenActivity, comparisonDate, startDate);
            if (comparisonBonusInDays.Mean == 0)
            {
                // not enough data
                return null;
            }
            Distribution comparisonEfficiencyBonusInHours = this.computeEfficiencyIncrease(chosenActivity, comparisonDate, startDate);
            Distribution comparisonValueRatio = this.compute_estimatedRating_ratio(chosenActivity, comparisonDate);

            Distribution longtermBonusInDays = this.compute_longtermValue_increase(chosenActivity, startDate, startDate);
            if (longtermBonusInDays.Mean == 0)
            {
                // no data
                return null;
            }
            Distribution efficiencyBonusInHours = this.computeEfficiencyIncrease(chosenActivity, startDate, startDate);
            Distribution shorttermValueRatio = this.compute_estimatedRating_ratio(chosenActivity, startDate);

            double roundedShorttermRatio = Math.Round(shorttermValueRatio.Mean, 3);
            double roundedShortTermStddev = Math.Round(shorttermValueRatio.StdDev, 3);
            double roundedComparisonBonus = Math.Round(comparisonValueRatio.Mean, 3);

            double roundedLongtermBonus = Math.Round(longtermBonusInDays.Mean, 3);
            double roundedLongtermStddev = Math.Round(longtermBonusInDays.StdDev, 3);
            double roundedComparisonLongtermBonus = Math.Round(comparisonBonusInDays.Mean, 3);

            double roundedEfficiencyBonus = Math.Round(efficiencyBonusInHours.Mean, 3);
            double roundedEfficiencyStddev = Math.Round(efficiencyBonusInHours.StdDev, 3);
            double roudnedComparisonEfficiencyLongtermBonus = Math.Round(comparisonEfficiencyBonusInHours.Mean, 3);

            // compute how long the user spent doing this and how long they usually spend doing it
            // TODO: do we want to change this calculation to use Math.Exp(LogActiveTime) like Engine.GuessParticipationEndDate does?
            double typicalNumSeconds = chosenActivity.MeanParticipationDuration;
            double actualNumSeconds = endDate.Subtract(startDate).TotalSeconds;
            double durationRatio;
            if (typicalNumSeconds != 0)
                durationRatio = Math.Round(actualNumSeconds / typicalNumSeconds, 3);
            else
                durationRatio = 0;

            bool fast = (actualNumSeconds <= typicalNumSeconds);
            bool funActivity = (shorttermValueRatio.Mean >= 1);
            bool funTime = (shorttermValueRatio.Mean >= comparisonValueRatio.Mean);
            bool soothingActivity = (longtermBonusInDays.Mean >= 0);
            bool soothingTime = (longtermBonusInDays.Mean > comparisonBonusInDays.Mean);
            bool efficientActivity = (efficiencyBonusInHours.Mean >= 0);
            bool efficientTime = (efficiencyBonusInHours.Mean >= comparisonEfficiencyBonusInHours.Mean);
            bool suggested = this.get_wasSuggested(chosenActivity.MakeDescriptor()) || !alreadyHappened;

            bool comparisonDateIsLaterDay = (comparisonDate.Date.CompareTo(startDate.Date) > 0);
            bool comparisonDateIsEarlierDay = (comparisonDate.Date.CompareTo(startDate.Date) < 0);
            bool comparisonDateIsEarlierTime = (comparisonDate.TimeOfDay.CompareTo(startDate.TimeOfDay) < 0);

            string recommendedTime;
            if (comparisonDateIsLaterDay)
            {
                recommendedTime = "a later day";
            }
            else
            {
                if (comparisonDateIsEarlierDay)
                {
                    recommendedTime = "an earlier day";
                }
                else
                {
                    if (comparisonDateIsEarlierTime)
                    {
                        recommendedTime = "an earlier time";
                    }
                    else
                    {
                        recommendedTime = "a later time";
                    }
                }
            }

            string remark;

            if (funActivity)
            {
                if (funTime)
                {
                    if (soothingActivity)
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Thank you!";
                                    else
                                        remark = "Spectacular!!!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Fireworks!";
                                    else
                                        remark = "Solid work!";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "What a party!";
                                    else
                                        remark = "Hey don't forget to work too :p";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good! But can you do better?";
                                    else
                                        remark = "Great! Don't forget to take a good break, though";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Phenomenal!";
                                    else
                                        remark = "Phenomenal!!!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good job! But could you choose " + recommendedTime + "?";
                                    else
                                        remark = "Great job! But could you choose " + recommendedTime + "?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // This activity is soothing but this time is not
                                    // This time is efficient but this activity is not
                                    if (fast)
                                        remark = "OMG!";
                                    else
                                        remark = "Yay!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "But maybe " + recommendedTime + " would have fewer interruptions?";
                                    else
                                        remark = "But is there any chance that you could choose " + recommendedTime + "?";
                                }
                            }
                        }
                    }
                    else // !soothingActivity
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Good job!";
                                    else
                                        remark = "Strong work!";
                                }
                                else // !efficientTime
                                {
                                    // a soothing time but not a soothing activity
                                    // an efficient activity but not an efficient time
                                    if (fast)
                                        remark = "Trying singing while you work";
                                    else
                                        remark = "Try music while you work";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // it's not a soothing activity but it is a soothing time
                                    // it's not an efficient activity but it is an efficient time
                                    if (fast)
                                        remark = "A good idea, but something else might be more wholesome";
                                    else
                                        remark = "Something else might be more wholesome";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good job on stopping early";
                                    else
                                        remark = "That's a bit indulgent :p";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "But you're done!";
                                    else
                                        remark = "Such hard work!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good work! You might burn out less if you choose " + recommendedTime + " though";
                                    else
                                        remark = "Good job! You might burn out less if you choose " + recommendedTime + " though";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // this activity isn't soothing and the time wasn't soothing
                                    // this activity isn't efficient but the time is efficient
                                    if (fast)
                                        remark = "Try doing something more wholesome next?";
                                    else
                                        remark = "Have you tried looking for a more wholesome activity?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good call on stopping early";
                                    else
                                        remark = "Are you sure you're not burning out?";
                                }
                            }
                        }
                    }
                }
                else // !funTime
                {
                    if (soothingActivity)
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Nice!";
                                    else
                                        remark = "Well done!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Pretty good! I think you'd be better off rescheduling though";
                                    else
                                        remark = "Not bad! I think you'd be better off rescheduling though";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // Fun activity but not a fun time
                                    // Soothing activity and also a soothing time
                                    // Not an efficient activity, but an efficient time
                                    if (fast)
                                        remark = "A turbocharged break!";
                                    else
                                        remark = "Not bad for a break";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "A short party!";
                                    else
                                        remark = "Nice party :p";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    // fun but not a fun time
                                    // soothing but not a soothing time
                                    // efficient and an efficient time
                                    if (fast)
                                        remark = "Mission complete";
                                    else
                                        remark = "Done";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good work, but you should choose " + recommendedTime;
                                    else
                                        remark = "Good work, but please choose " + recommendedTime;
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // fun but not a fun time
                                    // soothing but not a soothing time
                                    // not an efficient activity, but an efficient time
                                    if (fast)
                                        remark = "Maybe " + recommendedTime + " could be even better?";
                                    else
                                        remark = "I think " + recommendedTime + " could be even better";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "But I recommend " + recommendedTime;
                                    else
                                        remark = "But I strongly recommend " + recommendedTime;
                                }
                            }
                        }
                    }
                    else // !soothingActivity
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    // fun but not a fun time
                                    // not soothing, but a soothing time
                                    // efficent and an efficient time
                                    if (fast)
                                        remark = "Nice! Are you recharged?";
                                    else
                                        remark = "Nice! How was it?";
                                }
                                else // !efficientTime
                                {
                                    // fun but not a fun time
                                    // not soothing, but a soothing time
                                    // efficent but not efficient time
                                    if (fast)
                                        remark = "How about " + recommendedTime + " instead?";
                                    else
                                        remark = "I think " + recommendedTime + " would be better.";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // it's not a soothing activity but it is a soothing time
                                    // it's not an efficient activity but it is an efficient time
                                    if (fast)
                                        remark = "A decent idea. Is there something better you could do, though?";
                                    else
                                        remark = "This is a decent time, but is there something better you could be doing?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Is there anything better you could do?";
                                    else
                                        remark = "There must be something better you could be doing";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Are you recharged?";
                                    else
                                        remark = "Hope you're recharged!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Are you sure that now is the right time?";
                                    else
                                        remark = "Are you sure it's worth doing this now?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // this activity isn't soothing and the time wasn't soothing
                                    // this activity isn't efficient but the time is efficient
                                    if (fast)
                                        remark = "Really?";
                                    else
                                        remark = "Why?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Thank goodness that you stopped";
                                    else
                                        remark = "That must be the biggest indulgence ever";
                                }
                            }
                        }
                    }
                }
            }
            else // !funActivity
            {
                if (funTime)
                {
                    if (soothingActivity)
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Pretty good";
                                    else
                                        remark = "Awesome!!!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Impressive!";
                                    else
                                        remark = "Impressive!!!";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Sigh";
                                    else
                                        remark = "Sigh :(";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "But can you do better?";
                                    else
                                        remark = "Not bad, but can you do better?";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Ok";
                                    else
                                        remark = "Good";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good job! But I recommend " + recommendedTime + " if possible";
                                    else
                                        remark = "Great job! But I recommend " + recommendedTime + " if possible";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // This activity is soothing but this time is not
                                    // This time is efficient but this activity is not
                                    if (fast)
                                        remark = "Complete!";
                                    else
                                        remark = "Nice";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Maybe try " + recommendedTime + " to avoid being interrupted?";
                                    else
                                        remark = "I think " + recommendedTime + " would be less chaotic";
                                }
                            }
                        }
                    }
                    else // !soothingActivity
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                    {
                                        // fun time and soothing time but not fun activity or soothing activity
                                        remark = "How about some new habits?";
                                    }
                                    else
                                    {
                                        remark = "Pretty long";
                                    }
                                }
                                else // !efficientTime
                                {
                                    // a soothing time but not a soothing activity
                                    // an efficient activity but not an efficient time
                                    if (fast)
                                        remark = "Not bad";
                                    else
                                        remark = "Not bad, but something else might be even better";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // it's not a soothing activity but it is a soothing time
                                    // it's not an efficient activity but it is an efficient time
                                    if (fast)
                                        remark = "A good idea, but something else might be more enjoyable";
                                    else
                                        remark = "Not a bad time, but something else might be more enjoyable";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Probably don't want to do that for too long";
                                    else
                                        remark = "How'd you decide on that?";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                    {
                                        // If the user is doing something non-fun, non-soothing, and short, then they probably realized
                                        // it was a mistake
                                        remark = "Careful!";
                                    }
                                    else
                                    {
                                        // If the user is doing something non-fun, non-soothing, and long, but chose a fun time,
                                        // then the user probably planned their work well
                                        remark = "Work work work";
                                    }
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good work, but something else might be more fulfilling";
                                    else
                                        remark = "Good job, but something else might be more fulfilling";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // this activity isn't soothing and the time wasn't soothing
                                    // this activity isn't efficient but the time is efficient
                                    if (fast)
                                        remark = "Glad that that's over";
                                    else
                                        remark = "Do you have to do that?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Feel free not to do that";
                                    else
                                        remark = "Oops";
                                }
                            }
                        }
                    }
                }
                else // !funTime
                {
                    if (soothingActivity)
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "All right!";
                                    else
                                        remark = "I believe in you!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Pretty good! But have you considered " + recommendedTime + "?";
                                    else
                                        remark = "Not bad! But have you considered " + recommendedTime + "?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // not a fun activity, not a fun time
                                    // Soothing activity and also a soothing time
                                    // Not an efficient activity, but an efficient time
                                    if (fast)
                                        remark = "It's important";
                                    else
                                        remark = "I see some relaxation in your future";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "The routine";
                                    else
                                        remark = "I think this will make you happier and less efficient";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    // not fun, not a fun time
                                    // soothing but not a soothing time
                                    // efficient and an efficient time
                                    if (fast)
                                    {
                                        // If we're getting a lot of work done, then things we're doing briefly are probably rest
                                        remark = "Recharging...";
                                    }
                                    else
                                    {
                                        // If we're getting a lot of work done, then things we're doing for a long time are probably work
                                        remark = "Solid work";
                                    }
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good work, but you can choose " + recommendedTime;
                                    else
                                        remark = "Good work, but you'd be better off choosing " + recommendedTime;
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // not fun, not a fun time
                                    // soothing but not a soothing time
                                    // not an efficient activity, but an efficient time
                                    if (fast)
                                        remark = "Not bad, but I don't think you had to do this now";
                                    else
                                        remark = "Not bad, but I don't think you have to do this now";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "But I think you would prefer " + recommendedTime;
                                    else
                                        remark = "But I still recommend " + recommendedTime;
                                }
                            }
                        }
                    }
                    else // !soothingActivity
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    // not fun, not a fun time
                                    // not soothing, but a soothing time
                                    // efficent and an efficient time
                                    if (fast)
                                        remark = "Great progress! Does this matter?";
                                    else
                                        remark = "Alright";
                                }
                                else // !efficientTime
                                {
                                    // not fun, not a fun time
                                    // not soothing, but a soothing time
                                    // efficent but not efficient time
                                    if (fast)
                                        remark = "Hmm. Are you sure?";
                                    else
                                        remark = "Is this something you care about and is this the best time for it?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // it's not a soothing activity but it is a soothing time
                                    // it's not an efficient activity but it is an efficient time
                                    if (fast)
                                        remark = "I suspect you don't have to do that";
                                    else
                                        remark = "I suspect you didn't have to do that";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Come on!";
                                    else
                                        remark = "Seriously?";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "I think you can do better!";
                                    else
                                        remark = "I think you can do better";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Maybe write this down and do it later?";
                                    else
                                        remark = "Can this wait?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // this activity isn't soothing and the time wasn't soothing
                                    // this activity isn't efficient but the time is efficient
                                    if (fast)
                                        remark = "Yeah, don't do that";
                                    else
                                        remark = "What are you sacrificing for a little efficiency?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Oops!";
                                    else
                                        remark = "Oh no!";
                                }
                            }
                        }
                    }
                }
            }

            ParticipationNumericFeedback detailsProvider = new ParticipationNumericFeedback();
            detailsProvider.engine = this;
            detailsProvider.ActivityDatabase = this.activityDatabase;
            detailsProvider.StartDate = startDate;
            detailsProvider.EndDate = endDate;
            detailsProvider.ComparisonDate = comparisonDate;
            detailsProvider.ParticipationDurationDividedByAverage = durationRatio;
            detailsProvider.ChosenActivity = chosenActivity;

            detailsProvider.ExpectedEfficiency = roundedEfficiencyBonus;
            detailsProvider.ComparisonExpectedEfficiency = roudnedComparisonEfficiencyLongtermBonus;
            detailsProvider.ExpectedEfficiencyStddev = roundedEfficiencyStddev;

            detailsProvider.ExpectedFutureFun = roundedLongtermBonus;
            detailsProvider.ComparisonExpectedFutureFun = roundedComparisonLongtermBonus;
            detailsProvider.ExpectedFutureFunStddev = roundedLongtermStddev;

            detailsProvider.PredictedValue = roundedShorttermRatio;
            detailsProvider.PredictedCurrentValueStddev = roundedShortTermStddev;
            detailsProvider.ComparisonPredictedValue = roundedComparisonBonus;

            detailsProvider.Suggested = suggested;


            bool happySummary = soothingActivity;

            int longtermBonus = (int)longtermBonusInDays.Mean;
            int numExclamationPoints = (int)Math.Min((Math.Abs(longtermBonus) / 10), 10);
            string exclamationPoints = ":";
            if (numExclamationPoints > 0)
            {
                exclamationPoints = "";
                for (int i = 0; i < numExclamationPoints; i++)
                {
                    exclamationPoints += "!";
                }
            }

            string longtermBonusText;
            if (longtermBonus > 0)
            {
                longtermBonusText = "+" + longtermBonus + exclamationPoints + " ";
            }
            else
            {
                if (longtermBonus < 0)
                    longtermBonusText = "" + longtermBonus + exclamationPoints + " ";
                else
                    longtermBonusText = "";
            }
            if (detailsProvider.SuggestedBadIdea)
            {
                if (alreadyHappened)
                {
                    // If we suggested a bad idea and the user did it, override the comment into an apology.
                    // Users can find more info by pressing this button.
                    // Developers can find more info in ParticipationFeedback
                    remark = "Sorry! I only intended you to imagine this!";
                }
                else
                {
                    // If we suggested a bad idea and the user hasn't done it yet, clarify that we don't expect them to actually do it
                    // Instead we hope that this suggestion will help them think of a better idea
                    remark = "You can do better, right?";
                }
            }

            string summary = longtermBonusText + remark;

            return new ParticipationFeedback(chosenActivity, new ParticipationFeedbackFormatter(remark, detailsProvider), happySummary, detailsProvider);
        }


        public string ComputeBriefFeedback(DateTime when)
        {
            ScoreSummarizer ratingSummarizer = this.RatingSummarizer;
            DateTime oneWeekAgo = when.AddDays(-7);
            Distribution lastWeek = ratingSummarizer.GetValueDistributionForDates(oneWeekAgo, when, true, true);
            DateTime start = this.ActivityDatabase.RootActivity.DiscoveryDate;
            Distribution overall = ratingSummarizer.GetValueDistributionForDates(start, when, true, true);
            if (lastWeek.Weight <= 0 || overall.Weight <= 0)
                return "";
            if (lastWeek.Mean >= overall.Mean)
                return "Great!";
            else
                return "You can do it!";
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
        public EfficiencyCorrelator EfficiencyCorrelator
        {
            get
            {
                return this.efficiencyCorrelator;
            }
        }
        private CommonPreferences Get_UserPreferences()
        {
            return CommonPreferences.Instance;
        }

        public Random Randomness
        {
            set
            {
                this.randomGenerator = value;
            }
        }
        private UtilitiesAnalysis utilitiesCacheFor(ActivityRequest activityRequest)
        {
            DateTime when = activityRequest.Date;
            int weight = activityRequest.NumAcceptancesPerParticipation;
            if (this.currentUtilitiesCache != null)
            {
                if (!when.Equals(this.currentUtilitiesCache.ApplicableDate))
                {
                    this.currentUtilitiesCache = null;
                }
                else
                {
                    if (!(weight == this.currentUtilitiesCache.NumAcceptancesPerParticipation))
                        this.currentUtilitiesCache = null;
                }
            }
            if (this.currentUtilitiesCache == null)
                this.currentUtilitiesCache = new UtilitiesAnalysis(activityRequest.Date, activityRequest.NumAcceptancesPerParticipation);
            return this.currentUtilitiesCache;
        }
        private RatingsAnalysis ratingsCacheFor(DateTime when)
        {
            if (this.currentRatingsCache != null)
            {
                if (!when.Equals(this.currentRatingsCache.ApplicableDate))
                {
                    this.currentRatingsCache = null;
                }
            }
            if (this.currentRatingsCache == null)
                this.currentRatingsCache = new RatingsAnalysis(when);
            return this.currentRatingsCache;
        }

        private ActivityDatabase activityDatabase;                  // stores all Activities
        private List<AbsoluteRating> unappliedRatings;              // lists all Ratings that the RatingProgressions don't know about yet
        private List<Participation> unappliedParticipations;        // lists all Participations that the ParticipationProgressions don't know about yet
        private List<ActivitySkip> unappliedSkips;                  // lists all skips that the progressions don't know about yet
        private List<ActivitySuggestion> unappliedSuggestions;      // lists all ActivitySuggestions that the Activities don't know about yet
        DateTime firstInteractionDate;
        DateTime latestInteractionDate;
        bool requiresFullUpdate = true;
        Distribution thinkingTime;      // how long the user spends before skipping a suggestion
        ExponentialRatingSummarizer weightedRatingSummarizer;
        ExponentialRatingSummarizer efficiencySummarizer;
        EfficiencyCorrelator efficiencyCorrelator;
        
        
        Distribution ratingsOfUnpromptedActivities;
        int numSkips;
        int numPromptedParticipations;
        int numUnpromptedParticipations;
        private Random randomGenerator = new Random();

        // for each of several activities and metrics, tells which PlannedExperiment that needs updating if the user participates in that activity
        Dictionary<Activity, Dictionary<string, PlannedExperiment>> experimentToUpdate = new Dictionary<Activity, Dictionary<string, PlannedExperiment>>();
        int numCompletedExperiments;
        int numExperimentStartParticipations;
        RatingsAnalysis currentRatingsCache;
        UtilitiesAnalysis currentUtilitiesCache;

        LongtermValuePredictor longTerm_skipValue_interpolator;
        LongtermValuePredictor longTerm_participationValue_interpolator;
        LongtermValuePredictor longTerm_efficiency_interpolator;

        List<Activity> activitiesForLongtermInterpolation;
    }
}
