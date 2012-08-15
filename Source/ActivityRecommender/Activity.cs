using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveLinearInterpolation;

// The Activity class represents a way for the user to spend his/her time
// The entire goal of this program is to tell the user which Activity to spend time on (and how long to do so)
namespace ActivityRecommendation
{
    public class Activity
    {

        #region Constructors

        static int nextID = 0;

        // public
        public Activity()
        {
            this.Initialize("");
        }
        public Activity(string activityName)
        {
            this.Initialize(activityName);
        }
        private void Initialize(string activityName)
        {
            this.name = activityName;
            this.parents = new List<Activity>();
            this.children = new List<Activity>();
            this.parentDescriptors = new List<ActivityDescriptor>();

            this.SetupProgressions();
            this.SetupRatingPredictors();
            this.SetupParticipationProbabilityPredictors();

            this.PredictedScore = new Prediction();
            this.PredictedParticipationProbability = new Prediction();
            //this.LatestPrediction = new Prediction();
            //this.LatestPrediction.Date = new DateTime(0);
            this.latestRatingEstimationDate = new DateTime(0);
            this.latestParticipationDate = null;
            this.latestInteractionDate = null;
            this.uniqueIdentifier = nextID;
            this.defaultDiscoveryDate = DateTime.Now;
            this.participationDurations = Distribution.MakeDistribution(3600, 0, 1);    // default duration of an activity is 1 hour
            this.scoresWhenSuggested = new Distribution(0, 0, 0);
            this.scoresWhenNotSuggested = new Distribution(0, 0, 0);
            this.thinkingTimes = new Distribution(0, 0, 0);
            this.PredictionsNeedRecalculation = true;
            nextID++;
        }

        // initialize the Progressions from which we predict different values
        private void SetupProgressions()
        {
            this.ratingProgression = new RatingProgression(this);
            this.participationProgression = new ParticipationProgression(this);
            this.idlenessProgression = new IdlenessProgression(this);
            this.timeOfDayProgression = TimeProgression.DayCycle;
            DateTime Sunday = new DateTime(2011, 1, 7);
            this.timeOfWeekProgression = TimeProgression.WeekCycle;
            this.considerationProgression = new ConsiderationProgression(this);
            this.skipProgression = new SkipProgression(this);
            this.expectedRatingProgression = new ExpectedRatingProgression(this);
            this.expectedParticipationProbabilityProgression = new ExpectedParticipationProbabilityProgression(this);
        }
        // initialize the PredictionLinks that estimate the rating of this Activity
        private void SetupRatingPredictors()
        {
            this.extraRatingPredictionLinks = new List<IPredictionLink>();
            this.ratingTrainingProgressions = new List<IProgression>();
            this.ratingTestingProgressions = new List<IProgression>();

            this.ratingTrainingProgressions.Add(this.idlenessProgression);
            this.ratingTestingProgressions.Add(this.idlenessProgression);
            this.ratingTrainingProgressions.Add(this.participationProgression);
            this.ratingTestingProgressions.Add(this.participationProgression);
            this.ratingTrainingProgressions.Add(this.timeOfDayProgression);
            this.ratingTestingProgressions.Add(this.timeOfDayProgression);
            //this.ratingTrainingProgressions.Add(this.timeOfWeekProgression);
            //this.ratingTestingProgressions.Add(this.timeOfWeekProgression);
            //this.ratingTrainingProgressions.Add(this.ratingProgression);
            //this.ratingTestingProgressions.Add(this.ratingProgression);
            //this.ratingTrainingProgressions.Add(this.skipProgression);
            //this.ratingTestingProgressions.Add(this.skipProgression);
            //this.ratingTrainingProgressions.Add(this.considerationProgression);
            //this.ratingTestingProgressions.Add(this.considerationProgression);

            this.SetupRatingInterpolator();
            /*
            PredictionLink predictorFromOwnParticipations = new PredictionLink(this.participationProgression, this.ratingProgression);
            this.ratingPredictors.Add(predictorFromOwnParticipations);

            PredictionLink predictorFromOwnIdleness = new PredictionLink(this.idlenessProgression, this.ratingProgression);
            this.ratingPredictors.Add(predictorFromOwnIdleness);

            PredictionLink predictorFromTimeOfDay = new PredictionLink(this.timeOfDayProgression, this.ratingProgression);
            predictorFromTimeOfDay.InputWrapsAround = true;
            this.ratingPredictors.Add(predictorFromTimeOfDay);

            */
        }

        private void SetupRatingInterpolator()
        {
            FloatRange[] coordinates = new FloatRange[this.ratingTrainingProgressions.Count];
            int i;
            for (i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = this.ratingTrainingProgressions[i].EstimateOutputRange();
            }
            this.ratingInterpolator = new AdaptiveLinearInterpolator(new HyperBox(coordinates));
        }
        // initialize the PredictionLinks that estimate the probability that the user will do this activity
        private void SetupParticipationProbabilityPredictors()
        {
            this.extraParticipationPredictionLinks = new List<IPredictionLink>();

            this.participationTrainingProgressions = new List<IProgression>();
            this.participationTestingProgressions = new List<IProgression>();

            this.participationTrainingProgressions.Add(this.skipProgression);
            this.participationTestingProgressions.Add(this.skipProgression);

            //this.participationTrainingProgressions.Add(this.timeOfDayProgression);
            //this.participationTestingProgressions.Add(this.timeOfDayProgression);

            this.participationTrainingProgressions.Add(this.considerationProgression);
            this.participationTestingProgressions.Add(this.considerationProgression);

            //this.participationTrainingProgressions.Add(this.participationProgression);
            //this.participationTestingProgressions.Add(this.participationProgression);

            //this.participationTrainingProgressions.Add(this.timeOfWeekProgression);
            //this.participationTestingProgressions.Add(this.timeOfWeekProgression);
            

            /*
            this.participationTrainingProgressions.Add(this.participationProgression);
            this.participationTestingProgressions.Add(this.participationProgression);


            this.participationTrainingProgressions.Add(this.idlenessProgression);
            this.participationTestingProgressions.Add(this.idlenessProgression);

            */

            //this.participationTrainingProgressions.Add(this.ratingProgression);
            //this.participationTestingProgressions.Add(this.ratingProgression);

            /*

            this.participationTrainingProgressions.Add(this.considerationProgression);
            this.participationTestingProgressions.Add(this.considerationProgression);
            */

            //this.participationTrainingProgressions.Add(this.expectedRatingProgression);
            //this.participationTestingProgressions.Add(this.considerationProgression);

            //this.participationTrainingProgressions.Add(this.ratingProgression);
            //this.participationTestingProgressions.Add(this.ratingProgression);

            /*
            //this.participationProbabilityPredictors = new List<IPredictionLink>();
            this.participationProbabilityPredictors.Add(new PredictionLink(this.skipProgression, this.considerationProgression));
            this.participationProbabilityPredictors.Add(new PredictionLink(this.idlenessProgression, this.considerationProgression));
            //this.participationProbabilityPredictors.Add(new PredictionLink(this.timeOfDayProgression, this.considerationProgression));
            //this.participationProbabilityPredictors.Add(new PredictionLink(this.participationProgression, this.considerationProgression));
            this.participationProbabilityPredictors.Add(new PredictionLink(this.considerationProgression, this.considerationProgression));
            this.participationProbabilityPredictors.Add(new PredictionLink(this.expectedRatingProgression, this.considerationProgression));
            this.participationProbabilityPredictors.Add(new PredictionLink(this.ratingProgression, this.considerationProgression));
            */
            
            
            
            //this.SetupParticipationProbabilityInterpolator();
        }
        private void SetupParticipationProbabilityInterpolator()
        {
            List<IProgression> progressions = new List<IProgression>();
            progressions.Add(this.timeOfDayProgression);
            List<Activity> activities = this.GetParticipationPredictionActivities();
            foreach (Activity activity in activities)
            {
                foreach (IProgression progression in activity.participationTrainingProgressions)
                {
                    progressions.Add(progression);
                }
            }
            FloatRange[] coordinates = new FloatRange[progressions.Count];
            int i;
            for (i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = progressions[i].EstimateOutputRange();
            }
            this.participationInterpolator = new AdaptiveLinearInterpolator(new HyperBox(coordinates));
        }
        private Distribution QueryParticipationProbabilityInterpolator(double[] coordinates)
        {
            if (this.participationInterpolator == null)
            {
                this.SetupParticipationProbabilityInterpolator();
            }
            Distribution estimate = new Distribution(this.participationInterpolator.Interpolate(coordinates));
            return estimate;
        }
        #endregion

        #region Public Member Functions

        public string Description
        {
            get
            {
                return this.name;
            }
        }
        public string ID
        {
            get
            {
                return this.uniqueIdentifier.ToString();
            }
        }
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
        public bool Choosable { get; set; } // tells whether this Activity is a valid suggestion for the user
        public void AddParentDescriptor(ActivityDescriptor newParent)
        {
            this.parentDescriptors.Add(newParent);
        }
        public void AddParent(Activity newParent)
        {
            if (!this.parents.Contains(newParent))
            {
                this.parents.Add(newParent);
                newParent.children.Add(this);
                // for the moment, we only allow leaf nodes to be chosen for recommendations
                newParent.Choosable = false;

                this.ratingTrainingProgressions.Add(newParent.ratingProgression);
                this.ratingTestingProgressions.Add(newParent.expectedRatingProgression);

                //this.ratingTrainingProgressions.Add(newParent.participationProgression);
                //this.ratingTestingProgressions.Add(newParent.participationProgression);

                //this.ratingTrainingProgressions.Add(newParent.ParticipationProgression);
                //this.ratingTestingProgressions.Add(newParent.ParticipationProgression);
                //this.ratingTestingProgressions.Add(newParent.expectedRatingProgression);
                //PredictionLink link1 = new PredictionLink(newParent.RatingProgression, newParent.ExpectedRatingProgression, this.RatingProgression);
                //link1.Justification = "predicted based on the rating of " + newParent.Description;
                //this.ratingPredictors.Add(link1);

                //this.participationTrainingProgressions.Add(newParent.participationProgression);
                //this.participationTestingProgressions.Add(newParent.participationProgression);

                SimplePredictionLink link2 = new SimplePredictionLink(newParent.ExpectedRatingProgression, this.RatingProgression, "Probably close to the rating of " + newParent.Description);
                this.extraRatingPredictionLinks.Add(link2);


                SimplePredictionLink probabilityLink2 = new SimplePredictionLink(newParent.expectedParticipationProbabilityProgression, this.considerationProgression, "Probably just about as likely as " + newParent.Description);
                this.extraParticipationPredictionLinks.Add(probabilityLink2);

                // need to rebuild the interpolator because the number of dimensions is wrong
                this.SetupRatingInterpolator();
                //this.SetupParticipationProbabilityInterpolator();
            }
        }
        public List<Activity> Parents
        {
            get
            {
                return this.parents;
            }
        }
        public List<Activity> Children
        {
            get
            {
                return this.children;
            }
        }
        // returns a list containing this activity and all of its ancestors
        public List<Activity> GetAllSuperactivities()
        {
            List<Activity> superCategories = new List<Activity>();
            superCategories.Add(this);
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
        public List<Activity> GetParticipationPredictionActivities()
        {
            List<Activity> activities = new List<Activity>();
            activities.Add(this);
            /*
            foreach (Activity parent in this.parents)
            {
                activities.Add(parent);
            }*/
            return activities;
        }
        // returns a list containing this activity and all of its descendents
        public List<Activity> GetAllSubactivities()
        {
            List<Activity> subCategories = new List<Activity>();
            subCategories.Add(this);
            int i = 0;
            for (i = 0; i < subCategories.Count; i++)
            {
                Activity activity = subCategories[i];
                foreach (Activity child in activity.Children)
                {
                    if (!subCategories.Contains(child))
                    {
                        subCategories.Add(child);
                    }
                }
            }
            return subCategories;
        }
        // makes an ActivityDescriptor that describes this Activity
        public ActivityDescriptor MakeDescriptor()
        {
            ActivityDescriptor descriptor = new ActivityDescriptor();
            descriptor.ActivityName = this.Name;
            descriptor.Choosable = this.Choosable;
            return descriptor;
        }
        public RatingProgression RatingProgression 
        {
            get
            {
                return this.ratingProgression;
            }
        }
        public ExpectedRatingProgression ExpectedRatingProgression
        {
            get
            {
                return this.expectedRatingProgression;
            }
        }

        public ParticipationProgression ParticipationProgression
        {
            get
            {
                return this.participationProgression;
            }
        }
        public Participation SummarizeParticipationsBetween(DateTime startDate, DateTime endDate)
        {
            Participation result = this.participationProgression.SummarizeParticipationsBetween(startDate, endDate);
            return result;
        }


        // the most recent estimate about the rating of the activity
        public Prediction PredictedScore { get; set; }
        // the most recent estimate about the importants of suggesting the activity
        public Prediction SuggestionValue { get; set; }
        // the most recent estimate about how likely the user is to do the activity if we suggest it
        public Prediction PredictedParticipationProbability { get; set; }
        public bool PredictionsNeedRecalculation { get; set; }
        /*
        // the latest date at which the rating of this Activity was estimated
        public DateTime LatestRatingEstimationDate
        {
            get
            {
                return this.latestRatingEstimationDate;
            }
            set
            {
                this.latestRatingEstimationDate = value;
            }
        }

        public Distribution LatestEstimatedRating { get; set; } // what the engine thinks the user would give as a rating to this Activity
        // how useful it would be to the user if we suggested this Activity now. This is close to LatestEstimatedRating, but modified slightly
        // based on how long it has been since the user interacted with this Activity and also based on the initial assumption that the user
        // doesn't want to repeat an activity that they recently stopped
        public Distribution ParticipationProbability { get; set; }
        */
        /*
        public PredictionLink PredictorFromOwnRatings
        {
            get
            {
                return this.predictorFromOwnRatings;
            }
        }
        public PredictionLink PredictorFromOwnParticipations
        {
            get
            {
                return this.predictorFromOwnParticipations;
            }
        }
        public PredictionLink PredictorFromOwnIdleness
        {
            get
            {
                return this.predictorFromOwnIdleness;
            }
        }
        public PredictionLink PredictorFromTimeOfDay
        {
            get
            {
                return this.predictorFromTimeOfDay;
            }
        }
        */
        public int NumRatings
        {
            get
            {
                return this.ratingProgression.NumItems;
            }
        }
        public double MeanParticipationDuration // in seconds
        {
            get
            {
                return this.participationDurations.Mean;
            }
        }
        // declares that we think the activity was discovered on the given date
        public void SetDefaultDiscoveryDate(DateTime when)
        {
            this.defaultDiscoveryDate = when;
        }
        // declares that we know that the user interacted with this Activity on this date
        public void ApplyKnownInteractionDate(DateTime when)
        {
            if ((this.latestInteractionDate == null) || (((DateTime)this.latestInteractionDate).CompareTo(when) < 0))
                this.latestInteractionDate = when;
            if ((this.earliestInteractionDate == null) || (((DateTime)this.earliestInteractionDate).CompareTo(when) > 0))
                this.earliestInteractionDate = when;
        }
        public void ApplyInheritanceDate(DateTime when)
        {
            if ((this.latestInheritanceDate == null) || (((DateTime)this.latestInheritanceDate).CompareTo(when) < 0))
                this.latestInheritanceDate = when;
            if ((this.earliestInheritenceDate == null) || (((DateTime)this.earliestInheritenceDate).CompareTo(when) < 0))
                this.earliestInheritenceDate = when;
            this.ApplyKnownInteractionDate(when);
        }
        private void UpdateInteractionDates()
        {
            if (this.considerationProgression.NumItems > 0)
            {
                this.latestInteractionDate = this.considerationProgression.LastDatePresent;
                this.earliestInteractionDate = this.considerationProgression.FirstDatePresent;
            }
            else
            {
                if (this.latestInheritanceDate != null)
                    this.latestInteractionDate = this.latestInheritanceDate;
                else
                    this.latestInteractionDate = null;
                if (this.earliestInheritenceDate != null)
                    this.earliestInteractionDate = this.earliestInheritenceDate;
                else
                    this.earliestInteractionDate = null;
            }
        }
        public DateTime LatestInteractionDate
        {
            get
            {
                if (this.latestInteractionDate != null)
                    return (DateTime)this.latestInteractionDate;
                return this.defaultDiscoveryDate;
            }
        }
        public DateTime LatestParticipationDate
        {
            get
            {
                if (this.latestParticipationDate != null)
                    return (DateTime)this.latestParticipationDate;
                return this.defaultDiscoveryDate;
            }
        }
        public DateTime DiscoveryDate
        {
            get
            {
                if (this.earliestInteractionDate != null)
                    return (DateTime)this.earliestInteractionDate;
                return this.defaultDiscoveryDate;
            }
        }
        public DateTime GetEarliestInteractionDate()
        {
            return this.earliestInteractionDate.GetValueOrDefault(DateTime.Now);
        }
        public int NumConsiderations    // the number of times where the user either did this activity or explicitly decided not to do it
        {
            get
            {
                return this.considerationProgression.NumItems;
            }
        }
        public Distribution ScoresWhenSuggested     // the scores assigned to it at times when it was executed after being suggested
        {
            get
            {
                return this.scoresWhenSuggested;
            }
        }
        public Distribution ScoresWhenNotSuggested  // the scores assigned to it at times when it was executed but was not suggested recently
        {
            get
            {
                return this.scoresWhenNotSuggested;
            }
        }
        public Distribution ThinkingTimes           // how long it takes the user to skip this activity
        {
            get
            {
                return this.thinkingTimes;
            }
        }
        // says that the participation intensity was value at when, and adds that data to the participation interpolator
        private void AddParticipationDatapoint(DateTime when, double intensity)
        {
            if (this.participationInterpolator == null)
            {
                this.SetupParticipationProbabilityInterpolator();
            }

            List<IProgression> progressions = new List<IProgression>();
            progressions.Add(this.timeOfDayProgression);
            List<Activity> activities = this.GetParticipationPredictionActivities();
            foreach (Activity activity in activities)
            {
                foreach (IProgression progression in activity.participationTrainingProgressions)
                {
                    progressions.Add(progression);
                }
            }
            double[] coordinates = new double[progressions.Count];
            int i;
            ProgressionValue value;
            for (i = 0; i < coordinates.Length; i++)
            {
                value = progressions[i].GetValueAt(when, false);
                if (value != null)
                    coordinates[i] = value.Value.Mean;
                else
                    coordinates[i] = progressions[i].EstimateOutputRange().Middle;
            }
            AdaptiveLinearInterpolation.Datapoint newDatapoint = new AdaptiveLinearInterpolation.Datapoint(coordinates, intensity);
            this.participationInterpolator.AddDatapoint(newDatapoint);
        }
        public void AddRating(AbsoluteRating newRating)
        {
            if (newRating.Date != null)
            {
                // get the coordinates at that time
                double[] coordinates = new double[this.ratingTrainingProgressions.Count];
                int i;
                for (i = 0; i < coordinates.Length; i++)
                {
                    ProgressionValue value = this.ratingTrainingProgressions[i].GetValueAt((DateTime)newRating.Date, false);
                    if (value != null)
                        coordinates[i] = value.Value.Mean;
                    else
                        coordinates[i] = this.ratingTrainingProgressions[i].EstimateOutputRange().Middle;
                }
                AdaptiveLinearInterpolation.Datapoint datapoint = new AdaptiveLinearInterpolation.Datapoint(coordinates, newRating.Score);
                this.ratingInterpolator.AddDatapoint(datapoint);
            }


            // keep track of the ratings
            this.ratingProgression.AddRating(newRating);
            // keep track of the latest date at which anything happened
            if (newRating.Date != null)
                this.ApplyKnownInteractionDate((DateTime)newRating.Date);
        }
        public void AddParticipation(Participation newParticipation)
        {
            // get the coordinates at that time and save them
            //if (newParticipation.Suggested == null || newParticipation.Suggested == true)
            if (!newParticipation.Hypothetical)
                this.AddParticipationDatapoint(newParticipation.StartDate, 1);


            // keep track of the participation
            this.participationProgression.AddParticipation(newParticipation);
            this.idlenessProgression.AddParticipation(newParticipation);
            this.considerationProgression.AddParticipation(newParticipation);
            // keep track of the earliest and latest date at which anything happened
            this.ApplyKnownInteractionDate(newParticipation.StartDate);
            this.ApplyKnownInteractionDate(newParticipation.EndDate);
            DateTime when = newParticipation.EndDate;
            // keep track of the latest date at which the user interacted with the activity
            if (when.CompareTo(this.latestParticipationDate) > 0)
            {
                this.latestParticipationDate = when;
            }
            // keep track of the average participation duration
            this.participationDurations = this.participationDurations.Plus(Distribution.MakeDistribution(newParticipation.Duration.TotalSeconds, 0, 1));

            // keep track of the ratings when suggested
            AbsoluteRating rating = newParticipation.GetAbsoluteRating();
            if (rating != null)
            {
                // decide whether we know whether it was suggested
                if (newParticipation.Suggested != null)
                {
                    // update the appropriate counts
                    if (newParticipation.Suggested.Value == true)
                        this.scoresWhenSuggested = this.scoresWhenSuggested.Plus(rating.Score);
                    else
                        this.scoresWhenNotSuggested = this.scoresWhenNotSuggested.Plus(rating.Score);
                }
            }
        }
        public void AddSkip(ActivitySkip newSkip)
        {
            // get the coordinates at that time and save them
            this.AddParticipationDatapoint(newSkip.Date, 0);

            // updatethe knowledge of how long the user thinks
            if (newSkip.SuggestionDate != null)
            {
                TimeSpan duration = newSkip.Date.Subtract((DateTime)newSkip.SuggestionDate);
                this.thinkingTimes = this.thinkingTimes.Plus(Distribution.MakeDistribution(duration.TotalSeconds, 0, 1));
            }


            this.skipProgression.AddSkip(newSkip);
            this.considerationProgression.AddSkip(newSkip);
            // keep track of the earliest and latest date at which anything happened
            this.ApplyKnownInteractionDate(newSkip.Date);
        }
        public void RemoveParticipation(Participation unwantedParticipation)
        {
            // remove it from the progressions
            this.participationProgression.RemoveParticipation(unwantedParticipation);
            this.idlenessProgression.RemoveParticipation(unwantedParticipation);
            this.considerationProgression.RemoveParticipation(unwantedParticipation);
            // remove it from our other aggregates
            this.participationDurations = this.participationDurations.Minus(Distribution.MakeDistribution(unwantedParticipation.Duration.TotalSeconds, 0, 1));
            // recalculate the boundary dates
            this.UpdateInteractionDates();
            Participation latestParticipation = this.participationProgression.LatestParticipation;
            if (latestParticipation != null)
                this.latestParticipationDate = latestParticipation.EndDate;
            else
                this.latestParticipationDate = null;

            // It would be desirable to remove it from the interpolator if it has a rating
            // However, that functionality currently wouldn't be used so it isn't supported
        }

        // returns a bunch of estimates about how it will be rated at this date
        public List<Prediction> GetRatingEstimates(DateTime when)
        {
            double[] coordinates = new double[this.ratingTestingProgressions.Count];
            int i;
            for (i = 0; i < coordinates.Length; i++)
            {
                ProgressionValue value = this.ratingTestingProgressions[i].GetValueAt(when, false);
                if (value != null)
                    coordinates[i] = value.Value.Mean;
                else
                    coordinates[i] = this.ratingTestingProgressions[i].EstimateOutputRange().Middle;
            }
            List<Prediction> results = new List<Prediction>();
            Distribution estimate = new Distribution(this.ratingInterpolator.Interpolate(coordinates));
            double weight = this.NumRatings * 4;
            Distribution scaledEstimate = estimate.CopyAndReweightTo(weight);
            
            // add a little bit of uncertainty
            Distribution extraError = Distribution.MakeDistribution(0.5, 0.5, 2);
            Prediction prediction = new Prediction();
            prediction.ApplicableDate = when;
            prediction.Distribution = scaledEstimate.Plus(extraError);
            results.Add(prediction);

            foreach (IPredictionLink link in this.extraRatingPredictionLinks)
            {
                results.Add(link.Guess(when));
            }
            return results;
        }
        // returns a bunch of estimates about the probability that the user would do this activity if it were suggested now
        public List<Prediction> GetParticipationProbabilityEstimates(DateTime when)
        {
            // get the current coordinates
            List<Activity> activities = this.GetParticipationPredictionActivities();
            List<double> coordinateList = new List<double>();
            // concatenate all coordinates from all supercategories
            coordinateList.Add(this.timeOfDayProgression.GetValueAt(when, false).Value.Mean);
            foreach (Activity activity in activities)
            {
                foreach (IProgression progression in activity.participationTestingProgressions)
                {
                    ProgressionValue value = progression.GetValueAt(when, false);
                    if (value != null)
                        coordinateList.Add(value.Value.Mean);
                    else
                        coordinateList.Add(progression.EstimateOutputRange().Middle);
                }
            }
            double[] coordinates = coordinateList.ToArray();

            // have the interpolator make an estimate for these coordinates
            List<Prediction> results = new List<Prediction>();
            Distribution estimate = this.QueryParticipationProbabilityInterpolator(coordinates);
            double weight = this.NumConsiderations;
            Distribution scaledEstimate = estimate.CopyAndReweightTo(weight);

            // add a little bit of uncertainty
            Distribution extraError = Distribution.MakeDistribution(0.5, 0.5, 2);
            Distribution finalEstimate = scaledEstimate.Plus(extraError);
            Prediction prediction = new Prediction();
            prediction.ApplicableDate = when;
            prediction.Distribution = finalEstimate;
            results.Add(prediction);

            // add the results from any extra PredictionLinks
            foreach (IPredictionLink link in this.extraParticipationPredictionLinks)
            {
                results.Add(link.Guess(when));
            }
            return results;

            /*
            List<Prediction> predictions = new List<Prediction>();
            foreach (IPredictionLink link in this.participationProbabilityPredictors)
            {
                predictions.Add(link.Guess(when));
            }
            return predictions;
            */
        }

        #endregion

        #region Private Member Variables
        
        private string name;
        //private string id;
        //private string description;
        private DateTime latestRatingEstimationDate;
        private List<Activity> parents;
        private List<Activity> children;
        private List<ActivityDescriptor> parentDescriptors;

        //private List<IPredictionLink> ratingPredictors;
        List<IProgression> ratingTrainingProgressions;
        List<IProgression> ratingTestingProgressions;
        AdaptiveLinearInterpolator ratingInterpolator;
        private List<IPredictionLink> extraRatingPredictionLinks;


        private RatingProgression ratingProgression;
        private ExpectedRatingProgression expectedRatingProgression;
        //private PredictionLink predictorFromOwnRatings;
        private ParticipationProgression participationProgression;
        //private PredictionLink predictorFromOwnParticipations;
        private IdlenessProgression idlenessProgression;
        //private PredictionLink predictorFromOwnIdleness;
        private TimeProgression timeOfDayProgression;
        //private PredictionLink predictorFromTimeOfDay;
        private TimeProgression timeOfWeekProgression;

        private List<IProgression> participationTrainingProgressions;
        private List<IProgression> participationTestingProgressions;
        AdaptiveLinearInterpolator participationInterpolator;
        private List<IPredictionLink> extraParticipationPredictionLinks;
        //private List<IPredictionLink> participationProbabilityPredictors;

        private ConsiderationProgression considerationProgression;
        private SkipProgression skipProgression;
        private ExpectedParticipationProbabilityProgression expectedParticipationProbabilityProgression;
        //private PredictionLink 

        //private List<IPredictionLink> parentPredictionLinks;    // a list of all PredictionLinks that are used to predict the value of this Activity's RatingProgression from parent ratings

        
        private int uniqueIdentifier;
        private DateTime? latestInteractionDate;
        private DateTime? earliestInteractionDate;
        private DateTime? latestParticipationDate;

        private DateTime? earliestInheritenceDate;
        private DateTime? latestInheritanceDate;

        private DateTime defaultDiscoveryDate;
        private Distribution participationDurations;

        Distribution scoresWhenSuggested;
        Distribution scoresWhenNotSuggested;
        Distribution thinkingTimes;
        #endregion

    }
}