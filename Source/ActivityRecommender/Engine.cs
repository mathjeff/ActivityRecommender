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
            this.activityDatabase = new ActivityDatabase();
            this.unappliedRatings = new List<AbsoluteRating>();
            this.unappliedParticipations = new List<Participation>();
            this.unappliedSkips = new List<ActivitySkip>();
            this.allActivityDescriptors = new List<ActivityDescriptor>();
            this.inheritances = new List<Inheritance>();
            this.firstInteractionDate = DateTime.Now;
            this.latestInteractionDate = new DateTime(0);
            this.thinkingTime = Distribution.MakeDistribution(60, 0, 1);      // default amount of time thinking about a suggestion is 1 minute

        }
        // gives to the necessary objects the data that we've read. Optimized for when there are large quantities of data to give to the different objects
        public void FullUpdate()
        {
            this.CreateActivities();

            this.ApplyInheritances();

            // check for any activities that don't have parents
            foreach (Activity activity in this.activityDatabase.AllActivities)
            {
                if (activity.Parents.Count == 0 && activity.Name != "Activity")
                {
                    Console.WriteLine("Warning: Activity named " + activity.Name + " has no parents and may be a typo");
                }
            }
            this.ApplyParticipationsAndRatings();
            this.requiresFullUpdate = false;
        }
        // creates an Activity object for each ActivityDescriptor that needs one
        public void CreateActivities()
        {
            // first, create the necessary Activities
            foreach (ActivityDescriptor descriptor in this.allActivityDescriptors)
            {
                Activity newActivity = this.activityDatabase.AddOrCreateActivity(descriptor);
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
            while (true)
            {
                // find the next date at which something happened
                List<DateTime> dates = new List<DateTime>();
                if (this.unappliedRatings.Count > 0)
                    dates.Add((DateTime)this.unappliedRatings[0].Date);
                if (this.unappliedParticipations.Count > 0)
                    dates.Add((DateTime)this.unappliedParticipations[0].StartDate);
                if (this.unappliedSkips.Count > 0)
                    dates.Add((DateTime)this.unappliedSkips[0].Date);
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
                while (this.unappliedSkips.Count > 0 && ((DateTime)this.unappliedSkips[0].Date).CompareTo(nextDate) == 0)
                {
                    this.CascadeSkip(this.unappliedSkips[0]);
                    this.unappliedSkips.RemoveAt(0);
                }
            }
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
            return this.MakeRecommendation(this.activityDatabase.AllActivities, when);           
        }
        public Activity MakeRecommendation(Activity categoryToConsider, DateTime when)
        {
            List<Activity> candidates = this.FindAllSubCategoriesOf(categoryToConsider);
            return this.MakeRecommendation(candidates, when);
        }
        public Activity MakeRecommendation(IEnumerable<Activity> candidates, DateTime when)
        {
            if (this.requiresFullUpdate)
            {
                this.FullUpdate();
            }
            else
            {
                this.ApplyParticipationsAndRatings();
            }
            //
            foreach (Activity activity in candidates)
            {
                activity.PredictionsNeedRecalculation = true;
            }
            Activity bestActivity = null;
            Distribution bestRating = null;
            foreach (Activity candidate in candidates)
            {
                if (candidate.Choosable)
                {
                    this.EstimateValue(candidate, when);
                    Distribution currentRating = candidate.SuggestionValue.Distribution;
                    if (bestRating == null || bestRating.Mean < currentRating.Mean)
                    {
                        bestActivity = candidate;
                        bestRating = currentRating;
                    }
                }
            }
            return bestActivity;
        }
        // update all of the time-sensitive estimates for Activity
        public void EstimateValue(Activity activity, DateTime when)
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
            // First make sure that all parents' ratings are up-to-date
            foreach (Activity parent in activity.Parents)
            {
                this.EstimateValue(parent, when);
            }
            // Estimate the rating that the user would give to this activity if it were done
            List<Prediction> ratingPredictions = this.GetRatingEstimates(activity, when);
            // now that we've made a list of guesses, combine them to make one final guess of what we expect the user's rating to be
            Prediction ratingPrediction = this.CombineRatingPredictions(ratingPredictions);
            activity.PredictedScore = ratingPrediction;

            // Estimate the probability that the user would do this activity
            List<Prediction> probabilityPredictions = activity.GetParticipationProbabilityEstimates(when);
            Prediction probabilityPrediction = this.CombineProbabilityPredictions(probabilityPredictions);
            activity.PredictedParticipationProbability = probabilityPrediction;
            // (1 - probabilityPrediction.Distribution.Mean)
            // let p be the probability of a skip. the expected number of skips then is p + p^2 ... = -1 + 1 + p + p^2... = -1 + 1 / (1 - p)
            // = -1 + 1 / (the probability that the user will do the activity)
            // So the amount of waste is (the average length of a skip) * (-1 + 1 / (the probability that the user will do the activity))
            double averageWastedSeconds = this.thinkingTime.Mean * (-1 + 1 / (probabilityPrediction.Distribution.Mean));
            double usefulFraction = activity.MeanParticipationDuration / (averageWastedSeconds + activity.MeanParticipationDuration);
            // Now we have an estimate for what fraction of the user's time will be spent actually doing something useful
            // Finally, when calculating the suggestionValue, we can rescale all the scores
            foreach (Prediction prediction in ratingPredictions)
            {
                prediction.Distribution = prediction.Distribution.CopyAndStretchBy(usefulFraction);
            }


            // Now we estimate how useful it would be to suggest this activity to the user
            List<Prediction> extraPredictions = this.GetSuggestionEstimates(activity, when);
            IEnumerable<Prediction> suggestionPredictions = ratingPredictions.Concat(extraPredictions);
            activity.SuggestionValue = this.CombineRatingPredictions(suggestionPredictions);

        }

        // attempt to calculate the probability that the user would do this activity if we suggested it at this time
        public void EstimateParticipationProbability(Activity activity, DateTime when)
        {
            List<Prediction> predictions = activity.GetParticipationProbabilityEstimates(when);
            Prediction prediction = this.CombineProbabilityPredictions(predictions);
            activity.PredictedParticipationProbability = prediction;            
        }

        // returns a list of Distributions that are to be used to estimate the rating the user will assign to this Activity
        private List<Prediction> GetRatingEstimates(Activity activity, DateTime when)
        {
            // Now that the parents' ratings are up-to-date, the work begins
            // Make a list of predictions based on all the different factors
            List<Prediction> predictions = activity.GetRatingEstimates(when);
            return predictions;
        }
        // returns a list of Predictions that were only used to predict the value of suggesting the given activity at the given time
        private List<Prediction> GetSuggestionEstimates(Activity activity, DateTime when)
        {
            List<Prediction> predictions = new List<Prediction>();
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
                guess.Justification ="how long it's been since you thought about this activity";
                predictions.Add(guess);
            }
            return predictions;
        }
        // returns a list of all predictions that were used to predict the value of suggesting the given activity at the given time
        private IEnumerable<Prediction> GetAllSuggestionEstimates(Activity activity, DateTime when)
        {
            List<Prediction> ratingPredictions = this.GetRatingEstimates(activity, when);
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
            foreach (Distribution currentDistribution in distributions)
            {
                if (currentDistribution.Weight != 0)
                {
                    double weightFactor = (double)1 / currentDistribution.StdDev;
                    Distribution weightedDistribution = currentDistribution.CopyAndReweightBy(weightFactor);
                    sum = sum.Plus(weightedDistribution);
                }
            }
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
        }
        // gets called whenever any outside source provides a rating
        public void DiscoveredRating(AbsoluteRating newRating)
        {
            if (newRating.Date != null)
            {
                DateTime when = (DateTime)newRating.Date;
                if (when.CompareTo(this.firstInteractionDate) < 0)
                {
                    this.firstInteractionDate = when;
                }
                if (when.CompareTo(this.latestInteractionDate) > 0)
                {
                    this.latestInteractionDate = when;
                }
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
            // keep track of the first and last date at which anything happened
            this.DiscoveredParticipation(newParticipation);
            this.unappliedParticipations.Add(newParticipation);
            this.PutActivityDescriptorInMemory(newParticipation.ActivityDescriptor);

            
            Rating rating = newParticipation.GetCompleteRating();
            if (rating != null)
                this.PutRatingInMemory(rating);
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
                child = this.activityDatabase.AddOrCreateActivity(childDescriptor);
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
                parent = this.activityDatabase.AddOrCreateActivity(parentDescriptor);
            }
            else
            {
                this.CreatingActivity(child);
            }
            child.AddParent(parent);
            // Important! if (this.requiresFullUpdate) then the value calculated in EstimateRating will be wrong
            // However, when we need the correct value, we'll go calculate it, so it's okay
            // It's only when we're doing autocomplete that we don't bother with the full update
            // if we just created an empty child, then we can estimate its rating based on the parent's rating
            this.EstimateValue(child, DateTime.Now);
            /*if (!this.requiresFullUpdate)
            {
                // if we just created an empty child, then we can estimate its rating based on the parent's rating
                this.EstimateRating(child, DateTime.Now);
                //this.MakeRecommendation(child, DateTime.Now);
            }*/
            //this.WriteInheritance(newInheritance);
        }
        /*
        public void WriteInheritance(Inheritance newInheritance)
        {
            string text = this.textConverter.ConvertToString(newInheritance) + Environment.NewLine;
            StreamWriter writer = new StreamWriter(this.inheritancesFileName, true);
            writer.Write(text);
            writer.Close();
        }*/
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

            if (newSkip.Date.CompareTo(this.firstInteractionDate) < 0)
            {
                this.firstInteractionDate = newSkip.Date;
            }
            if (newSkip.Date.CompareTo(this.latestInteractionDate) > 0)
            {
                this.latestInteractionDate = newSkip.Date;
            }


            if (newSkip.SuggestionDate != null)
            {
                TimeSpan duration = newSkip.Date.Subtract((DateTime)newSkip.SuggestionDate);
                if (duration.TotalDays > 1)
                    Console.WriteLine("skip duration > 1 day, this is probably a mistake");
                this.thinkingTime = this.thinkingTime.Plus(Distribution.MakeDistribution(duration.TotalSeconds, 0, 1));
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
        /*
        // writes to disk a textual representation of the rating
        private void WriteRating(AbsoluteRating newRating)
        {
            string text = this.textConverter.ConvertToString(newRating) + Environment.NewLine;
            StreamWriter writer = new StreamWriter(this.ratingsFileName, true);
            writer.Write(text);
            writer.Close();
        }*/
        /*
        // writes to disk a textual representation of the participation
        private void WriteParticipation(Participation newParticipation)
        {
            string text = this.textConverter.ConvertToString(newParticipation) + Environment.NewLine;
            StreamWriter writer = new StreamWriter(this.ratingsFileName, true);
            writer.Write(text);
            writer.Close();
        }*/
        private ActivityDatabase activityDatabase;                  // stores all Activities
        private List<AbsoluteRating> unappliedRatings;              // lists all Ratings that the RatingProgressions don't know about yet
        private List<Participation> unappliedParticipations;        // lists all Participations that the ParticipationProgressions don't know about yet
        private List<ActivitySkip> unappliedSkips;                  // lists all skips that the progressions don't know about yet
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

    }
}
