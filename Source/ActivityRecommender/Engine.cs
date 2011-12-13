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
            //this.textConverter = new TextConverter(this);
            //this.ratingsFileName = "ActivityRatings.txt";
            //this.inheritancesFileName = "ActivityInheritances.txt";
            this.allActivityDescriptors = new List<ActivityDescriptor>();
            this.inheritances = new List<Inheritance>();
            this.firstInteractionDate = DateTime.Now;
            this.latestInteractionDate = new DateTime(0);
            //this.Initialize();
            //this.MakeRecommendation();

        }
        /*
        public void Initialize()
        {
            this.ReadFile();
            this.FullUpdate();
        }
        // reads the data file and puts their contents in memory
        public void ReadFile()
        {
            this.textConverter.ReadFile(inheritancesFileName);
            this.textConverter.ReadFile(ratingsFileName);
        }*/
        // gives to the necessary objects the data that we've read. Optimized for when there are large quantities of data to give to the different objects
        public void FullUpdate()
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

            // next, add the necessary parent pointers
            foreach (Inheritance inheritance in this.inheritances)
            {
                Activity child = this.activityDatabase.ResolveDescriptor(inheritance.ChildDescriptor);
                Activity parent = this.activityDatabase.ResolveDescriptor(inheritance.ParentDescriptor);
                child.ApplyKnownInteractionDate(inheritance.DiscoveryDate);
                child.AddParent(parent);
            }
            this.inheritances.Clear();

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
        // moves the ratings and participations from the pending queues into the Activities
        public void ApplyParticipationsAndRatings()
        {
            // Optimization opportunity for the future: cache the lists of superCategories
            // finally, cascade all of the ratings and participations to each activity
            foreach (AbsoluteRating rating in this.unappliedRatings)
            {
                this.CascadeRating(rating);
            }
            this.unappliedRatings.Clear();
            // Optimization opportunity for the future: cache the lists of superCategories
            foreach (Participation participation in this.unappliedParticipations)
            {
                this.CascadeParticipation(participation);
            }
            this.unappliedParticipations.Clear();
        }
        // assume initially that each activity was discovered when the engine was first started
        public void CreatingActivity(Activity activity)
        {
            activity.SuspectDiscoveryDate(this.firstInteractionDate);
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
        // performs Depth First Search to find all superCategories of the given Activity
        public List<Activity> FindAllSupercategoriesOf(Activity child)
        {
            List<Activity> superCategories = new List<Activity>();
            superCategories.Add(child);
            int i = 0;
            for (i = 0; i < superCategories.Count; i++)
            {
                Activity activity = superCategories[i];
                foreach (Activity parent in activity.Parents)
                {
                    if (!superCategories.Contains(parent))
                    {
                        superCategories.Add(parent);
                    }
                }
            }
            return superCategories;
        }
        public List<Activity> FindAllSubCategoriesOf(Activity parent)
        {
            List<Activity> superCategories = new List<Activity>();
            superCategories.Add(parent);
            int i = 0;
            for (i = 0; i < superCategories.Count; i++)
            {
                Activity activity = superCategories[i];
                foreach (Activity child in activity.Children)
                {
                    if (!superCategories.Contains(child))
                    {
                        superCategories.Add(child);
                    }
                }
            }
            return superCategories;
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
            Activity bestActivity = null;
            Distribution bestRating = null;
            foreach (Activity candidate in candidates)
            {
                if (candidate.Choosable)
                {
                    this.EstimateRating(candidate, when);
                    Distribution currentRating = candidate.SuggestionValue;
                    if (bestRating == null || bestRating.Mean < currentRating.Mean)
                    {
                        bestActivity = candidate;
                        bestRating = currentRating;
                    }
                }
            }
            return bestActivity;
        }
        public void EstimateRating(string activityName)
        {
            ActivityDescriptor descriptor = new ActivityDescriptor();
            descriptor.ActivityName = activityName;
            this.EstimateRating(new ActivityDescriptor());
        }
        public void EstimateRating(ActivityDescriptor descriptor)
        {
            Activity activity = this.activityDatabase.ResolveDescriptor(descriptor);
            DateTime when = DateTime.Now;
            this.EstimateRating(activity, when);
        }
        // returns a list of Distributions that are to be used to estimate the rating the user will assign to this Activity
        private List<Prediction> GetRatingEstimates(Activity activity, DateTime when)
        {
            // Now that the parents' ratings are up-to-date, the work begins
            // Make a list of predictions based on all the different factors
            List<Prediction> predictions = new List<Prediction>();
            // make a prediction based on the recent ratings of this Activity
            Prediction guess;
            guess = new Prediction();
            guess.Distribution = activity.PredictorFromOwnRatings.Guess(when);
            guess.Justification = "how you've rated this activity recently";
            predictions.Add(guess);
            // make a prediction based on the recent participations of this Activity
            guess = new Prediction();
            guess.Distribution = activity.PredictorFromOwnParticipations.Guess(when);
            guess.Justification = "how much you've done this activity recently";
            predictions.Add(guess);
            int numChildRatings = activity.RatingProgression.NumRatings;
            double weightFraction;
            // We train the parental Predictions to predict the rating of this Activity based on the known ratings of the parent
            // Now we use them to predict the rating of this Activity based on the expected ratings of the parent
            // This seems a little strange but is probably worthwhile
            foreach (PredictionLink link in activity.ParentPredictionLinks)
            {
                // rescale the weights!
                // possible concerns when rescaling with weights:
                // I don't want the addition of a new, empty parent to affect the rating distribution (by much). This suggests that I should scale by (parentCount - childCount)
                // I don't want a parent with thousands of ratings to have much more weight than a parent with hundreds of ratings (so set a constant weight)
                // I do want to give extra weight to the ratings of the activity itself. This is already taken care of by the PredictorFromOwnRatings
                // I don't want to double-count ratings any more than needed.
                // Like most things here, this isn't perfect but it's sufficient for now

                // set the weight of the parent's prediction to 1 - numChildRatings / numParentRatings
                Activity parent = link.Predictor.Owner;
                Distribution input = parent.LatestEstimatedRating;
                guess = new Prediction();
                Distribution output = link.Guess(input);
                guess.Justification = "predicted based on the rating of " + parent.Name;
                int numParentRatings = parent.NumRatings;
                weightFraction = 1;
                if (numParentRatings != 0)
                {
                    weightFraction = ((double)1 - (double)numChildRatings / (double)numParentRatings) * (double)numChildRatings;
                }
                guess.Distribution = output.CopyAndReweightTo(weightFraction);
                predictions.Add(guess);
                // Furthermore, in addition to using a PredictionLink to estimate the child's rating, we can make an additional guess
                // based on the fact that the parent is a supercategory of the child
                guess = new Prediction();
                output = parent.LatestEstimatedRating;
                guess.Justification = "probably close to the rating of " + parent.Name;
                weightFraction = Math.Sqrt(numChildRatings + 1);
                guess.Distribution = output.CopyAndReweightTo(weightFraction);
                predictions.Add(guess);
            }
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
                Prediction spacer = new Prediction(new Distribution(0, 0.5, weightFraction * 8), "you did this recently");
                predictions.Add(spacer);
            }
            // Finally, take into account the fact that we gain more information by suggesting activities that haven't been remembered in a while
            DateTime latestActivityInteractionDate = activity.LatestInteractionDate;
            TimeSpan idleDuration = when.Subtract(latestActivityInteractionDate);
            double numIdleHours = idleDuration.TotalHours;
            weightFraction = Math.Pow(numIdleHours, 0.7);
            double stdDev = 1;
            //guess = new Distribution(
            Distribution scores = Distribution.MakeDistribution(1, stdDev, weightFraction);
            Prediction guess = new Prediction(scores, "how long it's been since you thought about this activity");
            predictions.Add(guess);
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
        
        // I could simply add a special case that says that if an activity has exactly one parent, its rating equals the parent's
        
        public void EstimateRating(Activity activity, DateTime when)
        {
            // If we've already estimated the rating at this date, then just return what we calculated
            DateTime latestUpdateDate = activity.LatestRatingEstimationDate;
            if (when.CompareTo(latestUpdateDate) == 0)
            {
                return;
            }
            // If we get here, then we have to do some calculations
            // First make sure that all parents' ratings are up-to-date
            foreach (Activity parent in activity.Parents)
            {
                this.EstimateRating(parent, when);
            }
            List<Prediction> predictions = this.GetRatingEstimates(activity, when);
            // now that we've made a list of guesses, combine them to make one final guess of what we expect the user's rating to be
            Distribution actualGuess = this.CombineDistributions(predictions);
            activity.LatestEstimatedRating = actualGuess;

            // Now we estimate how useful it would be to suggest this activity to the user
            List<Prediction> extraDistributions = this.GetSuggestionEstimates(activity, when);
            IEnumerable<Prediction> allDistributions = predictions.Concat(extraDistributions);
            activity.SuggestionValue = this.CombineDistributions(allDistributions);

            // record that we performed a calculation at this date, so we know that we don't need to redo it
            activity.LatestRatingEstimationDate = when;
            //Console.WriteLine("Activity Name = " + activity.Name + " rating = " + activity.LatestEstimatedRating.Mean.ToString() + " suggestion value = " + activity.SuggestionValue.Mean.ToString());
        }
        // returns a string telling the most important reason that 'activity' was last rated as it was
        public string JustifyRating(Activity activity)
        {
            DateTime when = activity.LatestRatingEstimationDate;
            IEnumerable<Prediction> predictions = this.GetAllSuggestionEstimates(activity, when);
            double lowestScore = 1;
            string bestReason = null;
            foreach (Prediction candidate in predictions)
            {
                // make a list of all predictions except this one
                List<Prediction> predictionsMinusOne = new List<Prediction>(predictions);
                predictionsMinusOne.Remove(candidate);
                Distribution guess = this.CombineDistributions(predictionsMinusOne);
                if ((guess.Mean < lowestScore) || (bestReason == null))
                {
                    lowestScore = guess.Mean;
                    bestReason = candidate.Justification;
                }
            }
            return bestReason;
        }
        public Distribution CombineDistributions(IEnumerable<Prediction> predictions)
        {
            List<Distribution> distributions = new List<Distribution>();
            foreach (Prediction prediction in predictions)
            {
                distributions.Add(prediction.Distribution);
            }
            return this.CombineDistributions(distributions);
        }
        public Distribution CombineDistributions(IEnumerable<Distribution> distributions)
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
        // provides a previously unknown rating to the Engine
        /*public void AddRating(AbsoluteRating newRating)
        {
            // write it to the hard drive
            this.WriteRating(newRating);
            // adjust any global dates for having found it
            this.DiscoveredRating(newRating);
            // give it to any relevant Activities
            this.CascadeRating(newRating);
        }*/
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
            DateTime when = newRating.Date;
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
            // keep track of the first and last date at which anything happened
            this.DiscoveredParticipation(newParticipation);
            this.unappliedParticipations.Add(newParticipation);
            this.PutActivityDescriptorInMemory(newParticipation.ActivityDescriptor);
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
                child.ApplyKnownInteractionDate(newInheritance.DiscoveryDate);
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
            this.EstimateRating(child, DateTime.Now);
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
        public void PutInheritanceInMemory(Inheritance newInheritance)
        {
            this.inheritances.Add(newInheritance);
            this.PutActivityDescriptorInMemory(newInheritance.ParentDescriptor);
            this.PutActivityDescriptorInMemory(newInheritance.ChildDescriptor);
        }
        // tells the Engine about an Activity that it may choose from
        public void PutActivityDescriptorInMemory(ActivityDescriptor newDescriptor)
        {
            this.allActivityDescriptors.Add(newDescriptor);
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
    }
}
