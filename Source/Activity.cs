//#define SUPER_FAST_SPEED_TEST


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveLinearInterpolation;

// The Activity class represents a way for the user to spend his/her time
// The entire goal of this program is to tell the user which Activity to spend time on (and how long to do so)
namespace ActivityRecommendation
{
    public class Activity : INumerifier<WillingnessSummary>
    {
        #region Constructors

        static int nextID = 0;

        // public
        public Activity(RatingSummarizer overallRatings_summarizer)
        {
            this.Initialize("", overallRatings_summarizer);
        }
        public Activity(string activityName, RatingSummarizer overallRatings_summarizer)
        {
            this.Initialize(activityName, overallRatings_summarizer);
        }
        private void Initialize(string activityName, RatingSummarizer overallRatings_summarizer)
        {
            this.name = activityName;
            this.parents = new List<Activity>();
            this.parentsUsedForPrediction = new List<Activity>();
            this.children = new List<Activity>();
            this.parentDescriptors = new List<ActivityDescriptor>();
            this.overallRatings_summarizer = overallRatings_summarizer;

            this.SetupProgressions();
            //this.SetupRatingPredictors();
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
            this.participationProgression = new AutoSmoothed_ParticipationProgression(this);
            this.idlenessProgression = new IdlenessProgression(this);
            this.timeOfDayProgression = TimeProgression.DayCycle;
            DateTime Sunday = new DateTime(2011, 1, 7);
            this.timeOfWeekProgression = TimeProgression.WeekCycle;
            this.considerationProgression = new ConsiderationProgression(this);
            this.skipProgression = new SkipProgression(this);
            this.expectedRatingProgression = new ExpectedRatingProgression(this);
            this.expectedParticipationProbabilityProgression = new ExpectedParticipationProbabilityProgression(this);
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
        // tells whether this Activity is a valid suggestion for the user
        private bool? choosable;
        public bool Choosable
        {
            get
            {
                if (this.choosable != null)
                {
                    return this.choosable.Value;
                }
                return (this.children.Count <= 0);
            }
            set
            {
                this.choosable = value;
            }
        }
        public override string ToString()
        {
            return "Activity " + this.name;
        }

        public LinkedList<AbsoluteRating> PendingRatings = new LinkedList<AbsoluteRating>();
        public LinkedList<Participation> PendingParticipations = new LinkedList<Participation>();
        public LinkedList<ActivitySkip> PendingSkips = new LinkedList<ActivitySkip>();
        public LinkedList<ActivitySuggestion> PendingSuggestions = new LinkedList<ActivitySuggestion>();
        public ConsiderationProgression ConsiderationProgression {  get { return this.considerationProgression; } }
        public int NumParticipations { get { return (int)this.participationDurations.Weight; } }

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

                // need to rebuild the interpolator because the number of dimensions is wrong
                //this.SetupRatingInterpolator();
            }
        }
        public List<Activity> Parents
        {
            get
            {
                return this.parents;
            }
        }
        public List<Activity> ParentsUsedForPrediction
        {
            get
            {
                return this.parentsUsedForPrediction;
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
            descriptor.Choosable = this.Choosable; // we can require that the descriptor only match activities with the same Choosable as this, but it doesn't give much benefit
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

        public AutoSmoothed_ParticipationProgression ParticipationProgression
        {
            get
            {
                return this.participationProgression;
            }
        }
        public Participation SummarizeParticipationsBetween(DateTime startDate, DateTime endDate)
        {
            this.ApplyPendingParticipations();
            Participation result = this.participationProgression.SummarizeParticipationsBetween(startDate, endDate);
            return result;
        }


        // the most recent estimate about the rating of the activity
        public Prediction PredictedScore { get; set; }
        // the most recent estimate about the importants of suggesting the activity
        public Prediction SuggestionValue { get; set; }
        // the most recent estimate about how likely the user is to do the activity if we suggest it
        public Prediction PredictedParticipationProbability { get; set; }
        // the most recent estimate of the short-term average of what the user's happiness will be if this is suggested
        public double Utility { get; set; } // TODO change into a distribution having variance
        public bool PredictionsNeedRecalculation { get; set; }
        public TimeSpan AverageTimeBetweenConsiderations
        {
            get
            {
                this.ApplyPendingSkips();

                DateTime startDate = this.DiscoveryDate;
                DateTime endDate = DateTime.Now;
                int numConsiderations = this.considerationProgression.NumItems;
                TimeSpan duration = endDate.Subtract(startDate);
                double secondsInbetween = duration.TotalSeconds / (numConsiderations + 1);
                if (secondsInbetween < 0)
                    return new TimeSpan(0);
                return TimeSpan.FromSeconds(secondsInbetween);
            }
        }

        public int NumRatings
        {
            get
            {
                return this.ratingProgression.NumItems + this.PendingRatings.Count;
            }
        }
        public int NumSuggestions
        {
            get
            {
                return this.numSuggestions;
            }
        }
        public double MeanParticipationDuration // in seconds
        {
            get
            {
                this.ApplyPendingParticipations();
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
                return this.considerationProgression.NumItems + this.PendingParticipations.Count + this.PendingSkips.Count;
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
        public Distribution Scores
        {
            get
            {
                return this.scoresWhenSuggested.Plus(this.scoresWhenNotSuggested);
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
        private void AddParticipationDatapoint(DateTime when, WillingnessSummary willingness)
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
            AdaptiveLinearInterpolation.Datapoint<WillingnessSummary> newDatapoint = new AdaptiveLinearInterpolation.Datapoint<WillingnessSummary>(coordinates, willingness);
            this.participationInterpolator.AddDatapoint(newDatapoint);

            this.UpdateNext_RatingSummaries(4);
        }


        public void AddRating(AbsoluteRating newRating)
        {
            // keep track of the latest date at which anything happened
            if (newRating.Date != null)
                this.ApplyKnownInteractionDate((DateTime)newRating.Date);
            this.PendingRatings.AddLast(newRating);
        }
        private void ApplyPendingRatings()
        {
            this.SetupPredictorsIfNeeded();

            foreach (AbsoluteRating newRating in this.PendingRatings)
            {
                // For now, we don't care which activity the rating applied to. We just care what ratings were provided after the user participated in this activity
                if (newRating.Date != null)
                {
                    double[] coordinates = this.Get_Rating_TrainingCoordinates((DateTime)newRating.Date);

                    Distribution score = Distribution.MakeDistribution(newRating.Score, 0, 1);
                    AdaptiveLinearInterpolation.Datapoint<Distribution> datapoint = new AdaptiveLinearInterpolation.Datapoint<Distribution>(coordinates, score);
                    this.shortTerm_ratingInterpolator.AddDatapoint(datapoint);
                }

                // keep track of the ratings
                this.ratingProgression.AddRating(newRating);
            }
            this.PendingRatings.Clear();
        }
        public void AddParticipation(Participation newParticipation)
        {
            // keep track of startDate, endDate, count, and total time spent
            DateTime when = newParticipation.EndDate;
            if (!newParticipation.Hypothetical)
            {
                // keep track of the earliest and latest date at which anything happened
                this.ApplyKnownInteractionDate(newParticipation.StartDate);
                this.ApplyKnownInteractionDate(newParticipation.EndDate);
                // keep track of the latest date at which the user interacted with the activity
                if (when.CompareTo(this.latestParticipationDate) > 0)
                {
                    this.latestParticipationDate = when;
                }
                // keep track of the average participation duration
                this.participationDurations = this.participationDurations.Plus(Distribution.MakeDistribution(newParticipation.Duration.TotalSeconds, 0, 1));
            }


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
                // It would seem that we want to predict the user's overall happiness after this activity gets suggested. 
                // However, it turns out if we predict happiness from recent activity, then our suggestions become more boring
                // It seems that this is caused by a few things:
                // 1. If a long time (maybe a couple of weeks) goes by, then when we normalize the weight, ratings after that period become disproportionately important
                //   -This is a bug (design flaw) that should be fixed by making a more-intelligent estimate of overall ratings, rather than just the average of nearby ratings
                // 2. If the user does lots of short activities, that puts more weight on the near future's ratings than if the user did a few long activities
                //   -It's ok to increase weight based on number of ratings, because: A.
                //    A. that trains the user to give more ratings
                //    B. when the user offers more ratings, the user probably cares more about the outcome
                // For now we approximate that by recording the date whenever the activity is both executed and given a rating
                //this.AddNew_RatingSummary(newParticipation.StartDate, this.overallRatings_summarizer);
            }

            this.PendingParticipations.AddLast(newParticipation);
        }
        private void ApplyPendingParticipations()
        {
            foreach (Participation newParticipation in this.PendingParticipations)
            {
                if (!newParticipation.Hypothetical)
                {
                    // get the coordinates at that time and save them
                    WillingnessSummary willingness = new WillingnessSummary();
                    if (newParticipation.Suggested == null || newParticipation.Suggested == true)
                        willingness.NumPromptedParticipations = 1;
                    else
                        willingness.NumUnpromptedParticipations = 1;
                    this.AddParticipationDatapoint(newParticipation.StartDate, willingness);

                    // make a note to use this date for predicting longterm happiness from participations
                    this.AddNew_LongTerm_Participation_Summary_At(newParticipation.StartDate);
                }

                // keep track of the participation itself in the list
                this.participationProgression.AddParticipation(newParticipation);
                this.idlenessProgression.AddParticipation(newParticipation);
                this.considerationProgression.AddParticipation(newParticipation);
            }
            this.PendingParticipations.Clear();
        }
        public void RemoveParticipation(Participation unwantedParticipation)
        {
            this.ApplyPendingParticipations();

            // remove it from the progressions
            this.participationProgression.RemoveParticipation(unwantedParticipation);
            this.idlenessProgression.RemoveParticipation(unwantedParticipation);
            this.considerationProgression.RemoveParticipation(unwantedParticipation);
            if (!unwantedParticipation.Hypothetical)
            {
                // Note that we should never get here

                // remove it from our other aggregates
                this.participationDurations = this.participationDurations.Minus(Distribution.MakeDistribution(unwantedParticipation.Duration.TotalSeconds, 0, 1));
            }
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

        public void AddSkip(ActivitySkip newSkip)
        {
            // update the knowledge of how long the user thinks
            if (newSkip.SuggestionCreationDate != null)
            {
                TimeSpan duration = newSkip.CreationDate.Subtract((DateTime)newSkip.SuggestionCreationDate);
                this.thinkingTimes = this.thinkingTimes.Plus(Distribution.MakeDistribution(duration.TotalSeconds, 0, 1));
            }

            // keep track of the earliest and latest date at which anything happened
            this.ApplyKnownInteractionDate(newSkip.CreationDate);

            this.PendingSkips.AddLast(newSkip);
        }

        // Returns (the amount of time that the user spends doing this activity) divided (by the amount of time that the user is either doing this activity or considering doing it)
        private double GetAverageParticipationUsageFraction()
        {
            double activeDuration = this.participationDurations.SumValue;
            double idleDuration = this.thinkingTimes.SumValue;
            double totalDuration = activeDuration + idleDuration;
            if (totalDuration == 0)
                return 0;
            return activeDuration / totalDuration;
        }
        private void ApplyPendingSkips()
        {
            foreach (ActivitySkip newSkip in this.PendingSkips)
            {
                // get the coordinates at that time and save them
                WillingnessSummary willingness = new WillingnessSummary(0, 0, 1);
                this.AddParticipationDatapoint(newSkip.ApplicableDate.Value, willingness);


                this.skipProgression.AddSkip(newSkip);
                this.considerationProgression.AddSkip(newSkip);

                // We want to predict the user's overall happiness after this activity gets suggested.
                // For now we approximate that by recording the date whenever it is executed or skipped, and recording the user's overall future happiness
                //this.AddNew_RatingSummary(newSkip.Date, this.overallRatings_summarizer);
            }


            this.PendingSkips.Clear();

        }

        public void AddSuggestion(ActivitySuggestion newSuggestion)
        {
            this.numSuggestions++;
            this.PendingSuggestions.AddLast(newSuggestion);
        }
        private void ApplyPendingSuggestions()
        {
            foreach (ActivitySuggestion newSuggestion in this.PendingSuggestions)
            {
                // We want to predict the user's overall happiness after this activity gets suggested, so that we can detect if merely suggesting this activity has a positive impact
                this.AddNew_LongTerm_SuggestionValue_Summary_At(newSuggestion.GuessCreationDate());
            }
            // Also update an older datapoint so that no datapoint's value gets old
            this.UpdateNext_RatingSummaries(this.PendingSuggestions.Count);

            this.PendingSuggestions.Clear();
        }

        public void ApplyPendingData()
        {
            this.ApplyPendingRatings();
            this.ApplyPendingParticipations();
            this.ApplyPendingSkips();
            this.ApplyPendingSuggestions();
        }

        // 
        public Distribution Predict_LongtermValue_If_Suggested(DateTime when)
        {
            double[] coordinates = this.Get_Rating_PredictionCoordinates(when);
            Distribution estimate = new Distribution(this.longTerm_suggestionValue_interpolator.Interpolate(coordinates));            
            return estimate;
        }
        public Distribution Predict_LongtermValue_If_Participated(DateTime when)
        {
            double[] coordinates = this.Get_Rating_PredictionCoordinates(when);
            Distribution estimate = new Distribution(this.longTerm_participationValue_interpolator.Interpolate(coordinates));
            return estimate;
        }

        /*public ActivitySuggestionJustification JustifyInterpolation(ActivitySuggestion suggestion)
        {
            DateTime when = suggestion.StartDate;
            double[] coordinates = this.Get_Rating_PredictionCoordinates(when);
            // find size of neighborhood
            HyperBox<Distribution> box = this.longTerm_suggestionValue_interpolator.FindNeighborhoodCoordinates(coordinates);
            // get predicted value
            Distribution prediction = new Distribution(this.longTerm_suggestionValue_interpolator.Interpolate(coordinates));
            InterpolatorSuggestionJustification justification = new InterpolatorSuggestionJustification(suggestion);
            int i;
            // copy the coordinates and their labels onto the justification
            for (i = 0; i < coordinates.Length; i++)
            {
                String description = this.ratingTestingProgressions[i].Description;
                double value = coordinates[i];
                justification.AddInput(description, value, box.Coordinates[i]);
            }
            justification.AddOutput("Score", suggestion.PredictedScore.Mean);
            return justification;
        }*/

        // returns the coordinates from which a rating prediction is trained
        private double[] Get_Rating_TrainingCoordinates(DateTime when)
        {
            this.SetupPredictorsIfNeeded();

            double[] coordinates = new double[this.ratingTrainingProgressions.Count];
            int i;
            for (i = 0; i < coordinates.Length; i++)
            {
                ProgressionValue value = this.ratingTrainingProgressions[i].GetValueAt(when, false);
                if (value != null)
                    coordinates[i] = value.Value.Mean;
                else
                    coordinates[i] = this.ratingTrainingProgressions[i].EstimateOutputRange().Middle;
            }
            return coordinates;
        }

        // returns the coordinates from which a rating prediction is made
        private double[] Get_Rating_PredictionCoordinates(DateTime when)
        {
            this.ApplyPendingData();

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
            return coordinates;
        }

        // returns a bunch of estimates about how it will be rated at this date
        public List<Prediction> Get_ShortTerm_RatingEstimates(DateTime when)
        {
            this.ApplyPendingRatings();

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
            Distribution estimate = new Distribution(this.shortTerm_ratingInterpolator.Interpolate(coordinates));
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

        #region Required by INumerifier<double>
        
        public double Combine(double a, double b)
        {
            return a + b;
        }
        public AdaptiveLinearInterpolation.Distribution ConvertToDistribution(double a)
        {
            AdaptiveLinearInterpolation.Distribution distribution = AdaptiveLinearInterpolation.Distribution.MakeDistribution(a, 0, 1);
            return distribution;
        }

        #endregion

        #region Related to INumerifier<WillingnessSummary>

        public WillingnessSummary Combine(WillingnessSummary a, WillingnessSummary b)
        {
            return a.Plus(b);
        }
        public WillingnessSummary Remove(WillingnessSummary sum, WillingnessSummary valueToRemove)
        {
            return sum.Minus(valueToRemove);
        }
        /*
        public double ConvertToDouble(WillingnessSummary willingness)
        {
            double numParticipations = this.GetNumParticipations(willingness);
            double denominator = numParticipations + willingness.NumSkips;
            if (denominator == 0)
                return 0;
            return numParticipations / denominator;
        }
        */
        public AdaptiveLinearInterpolation.Distribution ConvertToDistribution(WillingnessSummary willingness)
        {
            double numParticipations = this.GetNumParticipations(willingness);
            double numSkips = willingness.NumSkips;

            AdaptiveLinearInterpolation.Distribution distribution = new AdaptiveLinearInterpolation.Distribution(numParticipations, numParticipations, numParticipations + numSkips);
            return distribution;
        }

        public WillingnessSummary Default()
        {
            return new WillingnessSummary();
        }

        // We pretend that this WillingnessSummary was a bunch of skips and suggested participations
        // Some of the unsuggested participations get converted into suggested participations
        // This function calculates the value we use for the number of suggested participations
        private double GetNumParticipations(WillingnessSummary willingness)
        {
            double bonus;   // = Math.Sqrt(willingness.NumUnpromptedParticipations);
            if (willingness.NumUnpromptedParticipations == 0)
                bonus = 0;
            else
                bonus = willingness.NumUnpromptedParticipations / (willingness.NumUnpromptedParticipations + willingness.NumSkips);
            
            return willingness.NumPromptedParticipations + bonus;
        }

        // Adds the following date as one that we will track for the purpose of predicting rating based on coordinates
        private void AddNew_LongTerm_SuggestionValue_Summary_At(DateTime when)
        {
            // compute the input coordinates
            double[] inputCoordinates = this.Get_Rating_TrainingCoordinates(when);

            // give it to the interpolator
            this.longTerm_suggestionValue_interpolator.AddDatapoint(when, inputCoordinates);
        }

        private void AddNew_LongTerm_Participation_Summary_At(DateTime when)
        {
            // compute the input coordinates
            double[] inputCoordinates = this.Get_Rating_TrainingCoordinates(when);

            // give it to the interpolator
            this.longTerm_participationValue_interpolator.AddDatapoint(when, inputCoordinates);
        }
        public void UpdateNext_RatingSummaries(int numRatingsToUpdate)
        {
            // If the interpolators don't exist yet, then it means we don't need them yet
            // Only do the update if the interpolators do exist already
            if (this.longTerm_participationValue_interpolator != null)
            {
                this.longTerm_participationValue_interpolator.UpdateMany(numRatingsToUpdate);
                this.longTerm_suggestionValue_interpolator.UpdateMany(numRatingsToUpdate);
            }
        }

        public void SetupPredictorsIfNeeded()
        {
            if (this.extraRatingPredictionLinks == null)
                this.SetupRatingPredictors();
        }


        #endregion

        #region Private Member Functions

        private bool shouldIncludeRatingSummaryInInterpolator(RatingSummary summary)
        {
            return summary.Item.Weight > 0;
        }


        private bool shouldUseNewlyAddedParentForPrediction(Activity parent)
        {
            // If the parent activity has been done a bunch of times already, then it takes a long time to analyze it and we don't want that
            // If this activity has been done a bunch of times already, then we already have enough information and don't need it
            if (parent.NumConsiderations < 1000)
                return true;
            else
                return false;
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

            foreach (Activity parent in this.parents)
            {
                if (this.shouldUseNewlyAddedParentForPrediction(parent))
                {
                    this.parentsUsedForPrediction.Add(parent);

                    this.ratingTrainingProgressions.Add(parent.ratingProgression);
                    this.ratingTestingProgressions.Add(parent.expectedRatingProgression);

                    SimplePredictionLink link2 = new SimplePredictionLink(parent.ExpectedRatingProgression, this.RatingProgression, "Probably close to the rating of " + parent.Description);
                    this.extraRatingPredictionLinks.Add(link2);

                    SimplePredictionLink probabilityLink2 = new SimplePredictionLink(parent.expectedParticipationProbabilityProgression, this.considerationProgression, "Probably just about as likely as " + parent.Description);
                    this.extraParticipationPredictionLinks.Add(probabilityLink2);
                }
            }

            this.SetupInterpolators();
        }

        private void SetupInterpolators()
        {
            FloatRange[] coordinates = new FloatRange[this.ratingTrainingProgressions.Count];
            int i;
            for (i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = this.ratingTrainingProgressions[i].EstimateOutputRange();
            }
            this.shortTerm_ratingInterpolator = new AdaptiveLinearInterpolator<Distribution>(new HyperBox<Distribution>(coordinates), new DistributionAdder());
            this.longTerm_suggestionValue_interpolator = new LongtermValuePredictor(new HyperBox<Distribution>(coordinates), this.overallRatings_summarizer);
            this.longTerm_participationValue_interpolator = new LongtermValuePredictor(new HyperBox<Distribution>(coordinates), this.overallRatings_summarizer);
        }
        // initialize the PredictionLinks that estimate the probability that the user will do this activity
        private void SetupParticipationProbabilityPredictors()
        {
            this.extraParticipationPredictionLinks = new List<IPredictionLink>();

            this.participationTrainingProgressions = new List<IProgression>();
            this.participationTestingProgressions = new List<IProgression>();

            this.participationTrainingProgressions.Add(this.skipProgression);
            this.participationTestingProgressions.Add(this.skipProgression);

            this.participationTrainingProgressions.Add(this.considerationProgression);
            this.participationTestingProgressions.Add(this.considerationProgression);

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
            this.participationInterpolator = new AdaptiveLinearInterpolator<WillingnessSummary>(new HyperBox<WillingnessSummary>(coordinates), this);
        }
        private Distribution QueryParticipationProbabilityInterpolator(double[] coordinates)
        {
            this.ApplyPendingParticipations();
            this.ApplyPendingSkips();
            if (this.participationInterpolator == null)
            {
                this.SetupParticipationProbabilityInterpolator();
            }
            Distribution estimate = new Distribution(this.participationInterpolator.Interpolate(coordinates));
            return estimate;
        }

        #endregion

        #region Private Member Variables

        private string name;
        //private string id;
        //private string description;
        private DateTime latestRatingEstimationDate;
        private List<Activity> parents;
        private List<Activity> parentsUsedForPrediction;
        private List<Activity> children;
        private List<ActivityDescriptor> parentDescriptors;

        //private List<IPredictionLink> ratingPredictors;
        List<IProgression> ratingTrainingProgressions;
        List<IProgression> ratingTestingProgressions;
        AdaptiveLinearInterpolator<Distribution> shortTerm_ratingInterpolator;  // this interpolator is used to estimate how happy the user feels after having done this Activity
        LongtermValuePredictor longTerm_participationValue_interpolator; // this interpolator is used to estimate what user's average happiness will if they do this activity
        LongtermValuePredictor longTerm_suggestionValue_interpolator;    // this interpolator is used to estimate what user's average happiness will if this activity is suggested
        private List<IPredictionLink> extraRatingPredictionLinks;


        private RatingProgression ratingProgression;
        private ExpectedRatingProgression expectedRatingProgression;
        //private PredictionLink predictorFromOwnRatings;
        private AutoSmoothed_ParticipationProgression participationProgression;
        //private PredictionLink predictorFromOwnParticipations;
        private IdlenessProgression idlenessProgression;
        //private PredictionLink predictorFromOwnIdleness;
        private TimeProgression timeOfDayProgression;
        //private PredictionLink predictorFromTimeOfDay;
        private TimeProgression timeOfWeekProgression;

        private List<IProgression> participationTrainingProgressions;
        private List<IProgression> participationTestingProgressions;
        AdaptiveLinearInterpolator<WillingnessSummary> participationInterpolator;
        private List<IPredictionLink> extraParticipationPredictionLinks;
        //private List<IPredictionLink> participationProbabilityPredictors;

        private ConsiderationProgression considerationProgression;
        private SkipProgression skipProgression;
        private ExpectedParticipationProbabilityProgression expectedParticipationProbabilityProgression;

        //private Queue<RatingSummary> ratingSummariesToUpdate;
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
        
        int numSuggestions;

        Distribution scoresWhenSuggested;
        Distribution scoresWhenNotSuggested;
        Distribution thinkingTimes;
        RatingSummarizer overallRatings_summarizer;

        #endregion

    }
}