using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActivityRecommendation.Effectiveness;
using AdaptiveInterpolation;
using StatLists;

// The Doable class represents a way for the user to spend his/her time
// The entire goal of this program is to tell the user which Doable to spend time on

// Category subclass represents a category of things to do, for example Fun, Exercise, or Work

// The ToDo subclass represents a specific task that has a completion status and has no child activities, for example 
namespace ActivityRecommendation
{
    public abstract class Activity : INumerifier<WillingnessSummary>
    {
        #region Constructors

        static int nextID = 0;

        // public
        public Activity(ScoreSummarizer overallRatings_summarizer, ScoreSummarizer overallEfficiency_summarizer)
        {
            this.Initialize("", overallRatings_summarizer, overallEfficiency_summarizer);
        }
        public Activity(string activityName, ScoreSummarizer overallRatings_summarizer, ScoreSummarizer overallEfficiency_summarizer)
        {
            this.Initialize(activityName, overallRatings_summarizer, overallEfficiency_summarizer);
        }
        private void Initialize(string activityName, ScoreSummarizer overallRatings_summarizer, ScoreSummarizer overallEfficiency_summarizer)
        {
            this.name = activityName;
            this.parents = new List<Activity>(1);
            this.parentsUsedForPrediction = new List<Activity>(1);
            this.parentDescriptors = new List<ActivityDescriptor>(1);
            this.overallRatings_summarizer = overallRatings_summarizer;
            this.overallEfficiency_summarizer = overallEfficiency_summarizer;

            this.SetupProgressions();
            //this.SetupRatingPredictors();
            this.SetupParticipationProbabilityPredictors();

            this.latestParticipationDate = null;
            this.latestInteractionDate = null;
            this.uniqueIdentifier = nextID;
            this.defaultDiscoveryDate = DateTime.Now;
            this.participationDurations = Distribution.Zero;
            this.ratingsWhenSuggested = Distribution.Zero;
            this.ratingsWhenNotSuggested = Distribution.Zero;
            this.thinkingTimes = Distribution.Zero;
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
            this.expectedRatingProgression = new ExpectedRatingProgression(this, null);
            this.expectedParticipationProbabilityProgression = new ExpectedParticipationProbabilityProgression(this, null);
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
        protected virtual Boolean isSuggestible()
        {
            return true;
        } 
        public bool Suggestible
        {
            get
            {
                return this.isSuggestible();
            }
        }
        public override string ToString()
        {
            return "Activity: " + this.name;
        }

        public List<AbsoluteRating> PendingRatings = new List<AbsoluteRating>();
        public List<Participation> PendingParticipationsForShorttermAnalysis = new List<Participation>();
        public List<ActivitySkip> PendingSkips = new List<ActivitySkip>();
        public List<ActivitySuggestion> PendingSuggestions = new List<ActivitySuggestion>();
        public List<EfficiencyMeasurement> PendingEfficiencyMeasurements = new List<EfficiencyMeasurement>();
        public ConsiderationProgression ConsiderationProgression {  get { return this.considerationProgression; } }
        public IdlenessProgression IdlenessProgression { get { return this.idlenessProgression; } }
        public AutoSmoothed_ParticipationProgression ParticipationProgression { get { return this.participationProgression; } }

        public int NumParticipations
        {
            get
            {
                return (int)this.participationDurations.Weight;
            }
        }

        public void AddParentDescriptor(ActivityDescriptor newParent)
        {
            this.parentDescriptors.Add(newParent);
        }
        public virtual void AddParent(Category newParent)
        {
            if (!this.parents.Contains(newParent))
            {
                this.parents.Add(newParent);
                newParent.AddChild(this);
            }
        }
        public virtual void AddParent(Problem newParent)
        {
            if (!this.parents.Contains(newParent))
            {
                this.parents.Add(newParent);
                newParent.AddChild(this);
            }
        }
        public List<Activity> Parents
        {
            get
            {
                return this.parents;
            }
        }
        public List<Activity> SelfAndAncestors
        {
            get
            {
                if (this.allAncestors == null)
                {
                    List<Activity> superActivities = new List<Activity>();
                    superActivities.Add(this);
                    int i = 0;
                    for (i = 0; i < superActivities.Count; i++)
                    {
                        Activity ancestor = superActivities[i];
                        foreach (Activity parent in ancestor.Parents)
                        {
                            if (!superActivities.Contains(parent))
                            {
                                superActivities.Add(parent);
                            }
                        }
                    }
                    this.allAncestors = superActivities;
                }
                return this.allAncestors;
            }
        }
        public bool HasAncestor(Activity other)
        {
            return this.SelfAndAncestors.Contains(other);
        }
        public void InvalidateAncestorList()
        {
            if (this.allAncestors != null)
            {
                this.allAncestors = null;
                foreach (Activity child in this.GetChildren())
                {
                    child.InvalidateAncestorList();
                }
            }
        }
        public List<Activity> ParentsUsedForPrediction
        {
            get
            {
                return this.parentsUsedForPrediction;
            }
        }

        public List<Activity> GetParticipationPredictionActivities()
        {
            List<Activity> activities = new List<Activity>(1);
            activities.Add(this);
            return activities;
        }
        // makes an ActivityDescriptor that describes this Doable
        public virtual ActivityDescriptor MakeDescriptor()
        {
            ActivityDescriptor descriptor = new ActivityDescriptor();
            descriptor.ActivityName = this.Name;
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
        public LinearProgression ParticipationsSmoothed(TimeSpan windowSize)
        {
            this.ApplyPendingData();
            return this.participationProgression.Smoothed(windowSize);
        }
        public ParticipationsSummary SummarizeParticipationsBetween(DateTime startDate, DateTime endDate)
        {
            this.ApplyPendingParticipations();
            ParticipationsSummary result = this.participationProgression.SummarizeParticipationsBetween(startDate, endDate);
            return result;
        }
        // Computes the average amount of time between consecutive participations in this activity
        public TimeSpan ComputeAverageIdlenessDuration(DateTime when)
        {
            if (this.NumParticipations < 1)
                return TimeSpan.FromSeconds(0);
            DateTime firstDate = this.DiscoveryDate;
            TimeSpan knownDuration = when.Subtract(firstDate);

            double participatedSeconds = this.participationDurations.SumValue;
            if (participatedSeconds <= 0)
                return TimeSpan.FromSeconds(0);

            if (knownDuration.TotalSeconds <= participatedSeconds)
                return TimeSpan.FromSeconds(0);

            double totalIdleSeconds = when.Subtract(firstDate).TotalSeconds;
            double averageIdleSeconds = totalIdleSeconds / this.NumParticipations;

            return TimeSpan.FromSeconds(averageIdleSeconds);
        }

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

        public int NumSkips
        {
            get
            {
                return this.skipProgression.NumItems + this.PendingSkips.Count;
            }
        }
        public double MeanParticipationDuration // in seconds
        {
            get
            {
                this.ApplyPendingParticipations();
                if (this.participationDurations.Weight <= 0)
                    return 3600; // we try to return a reasonable default
                return this.participationDurations.Mean;
            }
        }
        // declares that we think the Doable was discovered on the given date
        public void SetDefaultDiscoveryDate(DateTime when)
        {
            this.defaultDiscoveryDate = when;
        }
        // declares that we know that the user interacted with this Doable on this date
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
        public int NumConsiderations    // the number of times where the user either did this Doable or explicitly decided not to do it
        {
            get
            {
                return this.considerationProgression.NumItems + this.PendingParticipationsForShorttermAnalysis.Count + this.PendingSkips.Count;
            }
        }
        public Distribution RatingsWhenSuggested     // the scores assigned to it at times when it was executed after being suggested
        {
            get
            {
                return this.ratingsWhenSuggested;
            }
        }
        public Distribution RatingsWhenNotSuggested  // the scores assigned to it at times when it was executed but was not suggested recently
        {
            get
            {
                return this.ratingsWhenNotSuggested;
            }
        }
        public Distribution ThinkingTimes           // how long it takes the user to skip this Doable
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

            List<Activity> activities = this.GetParticipationPredictionActivities();
            List<IProgression> progressions = new List<IProgression>(activities.Count + 1);
            progressions.Add(this.timeOfDayProgression);
            foreach (Activity Doable in activities)
            {
                foreach (IProgression progression in Doable.participationTrainingProgressions)
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
            AdaptiveInterpolation.Datapoint<WillingnessSummary> newDatapoint = new AdaptiveInterpolation.Datapoint<WillingnessSummary>(coordinates, willingness);
            this.participationInterpolator.AddDatapoint(newDatapoint);
        }


        public void AddRating(AbsoluteRating newRating)
        {
            // keep track of the latest date at which anything happened
            if (newRating.Date != null)
                this.ApplyKnownInteractionDate((DateTime)newRating.Date);
            this.PendingRatings.Add(newRating);

            // keep track of the ratings when suggested
            bool suggested = false;
            AbsoluteRating rating = newRating as AbsoluteRating;
            if (rating != null && rating.FromUser && rating.Source != null)
            {
                Participation sourceParticipation = rating.Source.ConvertedAsParticipation;
                if (sourceParticipation != null && sourceParticipation.Suggested)
                    suggested = true;
            }
            if (suggested)
                this.ratingsWhenSuggested = this.ratingsWhenSuggested.Plus(rating.Score);
            else
                this.ratingsWhenNotSuggested = this.ratingsWhenNotSuggested.Plus(rating.Score);
            // It would seem that we want to predict the user's overall happiness after this Doable gets suggested. 
            // However, it turns out if we predict happiness from recent Doable, then our suggestions become more boring
            // It seems that this is caused by a few things:
            // 1. If a long time (maybe a couple of weeks) goes by, then when we normalize the weight, ratings after that period become disproportionately important
            //   -This is a bug (design flaw) that should be fixed by making a more-intelligent estimate of overall ratings, rather than just the average of nearby ratings
            // 2. If the user does lots of short activities, that puts more weight on the near future's ratings than if the user did a few long activities
            //   -It's ok to increase weight based on number of ratings, because: A.
            //    A. that trains the user to give more ratings
            //    B. when the user offers more ratings, the user probably cares more about the outcome
            // For now we approximate that by recording the date whenever the Doable is both executed and given a rating
            //this.AddNew_RatingSummary(newParticipation.StartDate, this.overallRatings_summarizer);
            this.ApplyPendingRatings();
        }
        private void ApplyPendingRatings()
        {
            this.SetupPredictorsIfNeeded();

            foreach (AbsoluteRating newRating in this.PendingRatings)
            {
                // For now, we don't care which Doable the rating applied to. We just care what ratings were provided after the user participated in this Doable
                if (newRating.Date != null)
                {
                    double[] coordinates = this.Get_Rating_TrainingCoordinates((DateTime)newRating.Date);

                    Distribution score = Distribution.MakeDistribution(newRating.Score, 0, 1);
                    AdaptiveInterpolation.Datapoint<Distribution> ratingDatapoint = new AdaptiveInterpolation.Datapoint<Distribution>(coordinates, score);
                    this.shortTerm_ratingInterpolator.AddDatapoint(ratingDatapoint);
                }

                // keep track of the ratings
                this.ratingProgression.AddRating(newRating);
            }
            this.PendingRatings.Clear();
        }
        public virtual void AddParticipation(Participation newParticipation)
        {
            // keep track of startDate, endDate, count, and total time spent
            DateTime when = newParticipation.EndDate;
            if (!newParticipation.Hypothetical)
            {
                // keep track of the earliest and latest date at which anything happened
                this.ApplyKnownInteractionDate(newParticipation.StartDate);
                this.ApplyKnownInteractionDate(newParticipation.EndDate);
                // keep track of the latest date at which the user interacted with the Doable
                if (this.latestParticipationDate == null)
                    this.latestParticipationDate = when;
                else
                {
                    if (when.CompareTo(this.latestParticipationDate.Value) > 0)
                        this.latestParticipationDate = when;
                }
                // keep track of the average participation duration
                this.participationDurations = this.participationDurations.Plus(Distribution.MakeDistribution(newParticipation.Duration.TotalSeconds, 0, 1));
            }

            this.PendingParticipationsForShorttermAnalysis.Add(newParticipation);
            this.ApplyPendingParticipations();
        }
        public void AddEfficiencyMeasurement(EfficiencyMeasurement efficiencyMeasurement)
        {
            // make a note to use this efficiency measurement in calculations
            this.PendingEfficiencyMeasurements.Add(efficiencyMeasurement);
        }
        private void ApplyPendingEfficiencies()
        {
            foreach (EfficiencyMeasurement measurement in this.PendingEfficiencyMeasurements)
            {
                double[] coordinates = this.Get_Rating_TrainingCoordinates((DateTime)measurement.StartDate);

                Distribution score = Distribution.MakeDistribution(measurement.RecomputedEfficiency.Mean, 0, 1);
                AdaptiveInterpolation.Datapoint<Distribution> ratingDatapoint = new AdaptiveInterpolation.Datapoint<Distribution>(coordinates, score);
                this.shortTerm_EfficiencyInterpolator.AddDatapoint(ratingDatapoint);
            }
            this.PendingEfficiencyMeasurements.Clear();
        }
        private void ApplyPendingParticipations()
        {
            this.ApplyPendingParticipationsForShorttermAnalysis();
        }
        private void ApplyPendingParticipationsForShorttermAnalysis()
        {
            foreach (Participation newParticipation in this.PendingParticipationsForShorttermAnalysis)
            {
                // get the coordinates at that time and save them
                WillingnessSummary willingness;
                if (newParticipation.Suggested)
                    willingness = WillingnessSummary.Prompted;
                else
                    willingness = WillingnessSummary.Unprompted;

                this.AddParticipationDatapoint(newParticipation.StartDate, willingness);

                // keep track of the participation itself in the list
                this.participationProgression.AddParticipation(newParticipation);
                this.idlenessProgression.AddParticipation(newParticipation);
                this.considerationProgression.AddParticipation(newParticipation);
            }
            this.PendingParticipationsForShorttermAnalysis.Clear();
        }
        public List<Participation> Participations
        {
            get
            {
                this.ApplyPendingParticipations();
                return this.participationProgression.Participations;
            }
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
            TimeSpan duration = newSkip.ThinkingTime;
            this.thinkingTimes = this.thinkingTimes.Plus(Distribution.MakeDistribution(duration.TotalSeconds, 0, 1));

            // keep track of the earliest and latest date at which anything happened
            this.ApplyKnownInteractionDate(newSkip.CreationDate);

            this.PendingSkips.Add(newSkip);
            this.ApplyPendingSkips();
        }

        // Returns (the amount of time that the user spends doing this Doable) divided (by the amount of time that the user is either doing this Doable or considering doing it)
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
                WillingnessSummary willingness = WillingnessSummary.Skipped;
                this.AddParticipationDatapoint(newSkip.SuggestionStartDate, willingness);


                this.skipProgression.AddSkip(newSkip);
                this.considerationProgression.AddSkip(newSkip);

                // We want to predict the user's overall happiness after this Doable gets suggested.
                // For now we approximate that by recording the date whenever it is executed or skipped, and recording the user's overall future happiness
                //this.AddNew_RatingSummary(newSkip.Date, this.overallRatings_summarizer);
            }

            this.PendingSkips.Clear();
        }

        public void AddSuggestion(ActivitySuggestion newSuggestion)
        {
            this.numSuggestions++;
            this.PendingSuggestions.Add(newSuggestion);
            if (this.latestSuggestionDate.CompareTo(newSuggestion.CreatedDate) < 0)
                this.latestSuggestionDate = newSuggestion.CreatedDate;
            this.ApplyKnownInteractionDate(newSuggestion.CreatedDate);
            this.ApplyPendingSuggestions();
        }
        public DateTime? LatestSuggestionDate
        {
            get
            {
                if (this.numSuggestions < 1)
                    return null;
                return this.latestSuggestionDate;
            }
        }
        private void ApplyPendingSuggestions()
        {
            this.PendingSuggestions.Clear();
        }

        public void ApplyPendingData()
        {
            this.ApplyPendingRatings();
            this.ApplyPendingParticipations();
            this.ApplyPendingSkips();
            this.ApplyPendingSuggestions();
        }

        // Predicts the relative efficiency with which the user is expected to be able to progress on this Activity at the given time
        // Note that this isn't supposed to be a measure of the overall difficulty of the activity
        // Because we don't have a way to separate the definitions of difficulty (1 / duration) and efficiency (effectiveness per unit time)
        // (that is, (effectiveness = duration * efficiency)),
        // the average return value from PredictEfficiency is supposed to tend toward 1
        // (of course, in practice, because we need a bunch of data before have good estimates of the difficulties of the various tasks,
        // in practice, for some activities this will tend to be higher than for others)
        public Distribution PredictEfficiency(DateTime when)
        {
            this.ApplyPendingEfficiencies();
            double[] coordinates = this.Get_Rating_PredictionCoordinates(when);
            Distribution estimate = new Distribution(this.shortTerm_EfficiencyInterpolator.Interpolate(coordinates));
            Distribution extraError = Distribution.MakeDistribution(1, 0, 1);
            Distribution result = estimate.Plus(extraError);
            return result;
        }

        // returns the coordinates from which a rating prediction is trained
        private double[] Get_Rating_TrainingCoordinates(DateTime when)
        {
            this.SetupPredictorsIfNeeded();
            this.ApplyPendingSkips();
            this.ApplyPendingParticipationsForShorttermAnalysis();

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
            //this.ApplyPendingParticipations();
            this.ApplyPendingData();
            this.ApplyPendingSkips(); // TODO: figure out why this line changes the results

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


        private double[] Get_Efficiency_PredictionCoordinates(DateTime when)
        {
            return this.Get_Rating_PredictionCoordinates(when);
        }


        // returns a bunch of estimates about how it will be rated at this date
        public List<Prediction> Get_ShortTerm_RatingEstimates(DateTime when)
        {
            this.ApplyPendingRatings();
            this.ApplyPendingParticipations();

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
            List<Prediction> results = new List<Prediction>(this.extraRatingPredictionLinks.Count + 1);
            Distribution estimate = new Distribution(this.shortTerm_ratingInterpolator.Interpolate(coordinates));
            double weight = this.NumRatings * 4;
            Distribution scaledEstimate = estimate.CopyAndReweightTo(weight);

            // add a little bit of uncertainty
            Distribution extraError = Distribution.MakeDistribution(0.5, 0.25, 2);

            // also figure out what is a normal result
            InterpolatorSuggestion_Justification interpolatorJustification = new InterpolatorSuggestion_Justification(this, estimate, coordinates);
            Distribution finalEstimate = scaledEstimate.Plus(extraError);

            // explanations of the interpolator results
            List<Justification> justifications;
            if (estimate.Weight > 0)
                justifications = new List<Justification>() { interpolatorJustification, new LabeledDistributionJustification(extraError, "extra uncertainty") };
            else
                justifications = new List<Justification>() { new LabeledDistributionJustification(extraError, "no history") };
            Composite_SuggestionJustification justification = new Composite_SuggestionJustification(finalEstimate, justifications);
            justification.Label = "Adjusted interpolator result";

            // final results
            Prediction prediction = new Prediction(this, finalEstimate, when, justification);
            results.Add(prediction);
            foreach (IPredictionLink link in this.extraRatingPredictionLinks)
            {
                Prediction otherPrediction = link.Guess(when);
                results.Add(otherPrediction);
            }
            return results;
        }

        // returns a bunch of estimates about the probability that the user would do this Doable if it were suggested now
        public List<Prediction> GetParticipationProbabilityEstimates(DateTime when)
        {
            // get the current coordinates
            List<Activity> activities = this.GetParticipationPredictionActivities();
            List<double> coordinateList = new List<double>(activities.Count * this.participationTestingProgressions.Count + 1);
            // concatenate all coordinates from all supercategories
            coordinateList.Add(this.timeOfDayProgression.GetValueAt(when, false).Value.Mean);
            foreach (Activity doable in activities)
            {
                doable.ApplyPendingSkips();
                doable.ApplyPendingParticipationsForShorttermAnalysis();
                foreach (IProgression progression in doable.participationTestingProgressions)
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
            List<Prediction> results = new List<Prediction>(this.extraRatingPredictionLinks.Count + 1);
            Distribution estimate = this.QueryParticipationProbabilityInterpolator(coordinates);
            double weight = this.NumConsiderations;
            Distribution scaledEstimate = estimate.CopyAndReweightTo(weight);

            // add a little bit of uncertainty
            Distribution extraError = Distribution.MakeDistribution(0.5, 0.25, 2);
            Distribution finalEstimate = scaledEstimate.Plus(extraError);
            Distribution typicalParticipationProbability = new Distribution(this.participationInterpolator.GetAverage());
            Justification justification = new InterpolatorSuggestion_Justification(this, finalEstimate, null);
            Prediction prediction = new Prediction(this, finalEstimate, when, "Participation probability from interpolator");
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

        public virtual List<Activity> GetChildren()
        {
            return new List<Activity>(0);
        }

        public bool HasChildCategory
        {
            get
            {
                foreach (Activity child in this.GetChildren())
                {
                    if (child is Category)
                        return true;
                }
                return false;
            }
        }

        // returns a list containing this activity and all of its descendents
        public List<Activity> GetChildrenRecursive()
        {
            List<Activity> subCategories = new List<Activity>();
            subCategories.Add(this);
            int i = 0;
            for (i = 0; i < subCategories.Count; i++)
            {
                Activity activity = subCategories[i];
                foreach (Activity child in activity.GetChildren())
                {
                    if (!subCategories.Contains(child))
                    {
                        subCategories.Add(child);
                    }
                }
            }
            return subCategories;
        }

        public List<ToDo> OtherOpenTodos
        {
            get
            {
                List<ToDo> openTodos = new List<ToDo>();
                foreach (Activity activity in this.GetChildrenRecursive())
                {
                    if (activity == this)
                        continue;
                    ToDo todo = activity as ToDo;
                    if (todo != null)
                    {
                        if (!todo.IsCompleted())
                            openTodos.Add(todo);
                    }
                }
                return openTodos;
            }
        }

        // Tells whether this activity has exactly one descendant such that that descendant has no other descendants.
        // This property could be interesting because an activity having only one leaf descendant might be more worth suggesting than its single leaf descendant
        public Activity GetUniqueLeafDescendant()
        {
            List<Activity> candidates = new List<Activity>();
            candidates.Add(this);
            Activity leaf = this; // if this activity has no children, we're counting itself as a leaf
            int numLeaves = 0;
            int i = 0;
            for (i = 0; i < candidates.Count; i++)
            {
                Activity activity = candidates[i];
                if (activity.GetChildren().Count < 1)
                {
                    numLeaves++;
                    if (numLeaves > 1)
                        return null;
                    leaf = activity;
                }
                else
                {
                    foreach (Activity child in activity.GetChildren())
                    {
                        if (!candidates.Contains(child))
                        {
                            candidates.Add(child);
                        }
                    }
                }
            }
            return leaf;
        }

        // Whether the user has ever asked for a suggestion specifically from this activity
        public bool EverRequestedFromDirectly { get; set; }

        // Returns a list of Metrics that are attached to this Activity directly rather than to another Activity
        // Any Metric in this list should have a stable index across runs even if the user adds another inheritance
        public List<Metric> IntrinsicMetrics
        {
            get
            {
                if (this.intrinsicMetrics == null)
                {
                    this.intrinsicMetrics = new List<Metric>(1);
                }
                return this.intrinsicMetrics;
            }
            set
            {
                this.intrinsicMetrics = value;
            }
        }
        public void AddIntrinsicMetric(Metric metric)
        {
            this.IntrinsicMetrics.Add(metric);
        }
        // Returns a list of Metrics that are attached to another Activity. The ordering of this list could change when the user adds a new inheritance
        public virtual List<Metric> InheritedMetrics
        {
            get
            {
                List<Metric> metricList = new List<Metric>();
                HashSet<Metric> metricSet = new HashSet<Metric>(metricList);
                foreach (Activity parent in this.parents)
                {
                    foreach (Metric metric in parent.AllMetrics)
                    {
                        if (!metricSet.Contains(metric))
                        {
                            metricList.Add(metric);
                            metricSet.Add(metric);
                        }
                    }
                }
                return metricList;
            }
        }

        // Tells whether this Activity is a Solution
        public bool IsSolution
        {
            get
            {
                if (this is Problem)
                    return false;
                foreach (Activity parent in this.parents)
                    if (parent is Problem)
                        return true;
                return false;
            }
        }
        public List<Metric> AllMetrics
        {
            get
            {
                return new List<Metric>(this.IntrinsicMetrics.Concat(this.InheritedMetrics));
            }
        }
        public bool HasAMetric
        {
            get
            {
                if (this.intrinsicMetrics != null && this.intrinsicMetrics.Count > 0)
                    return true;
                if (this.InheritedMetrics.Count > 0)
                    return true;
                return false;
            }
        }
        // Returns the default metric that we refer to if no other metric name is specified
        // This allows the data file to be shorter in the common case when an activity only has one metric
        public Metric DefaultMetric
        {
            get
            {
                if (this.intrinsicMetrics != null && this.intrinsicMetrics.Count > 0)
                    return this.intrinsicMetrics[0];
                return null;
            }
        }
        public bool DidUserAssignAMetric
        {
            get
            {
                if (this.HasAMetric && this.DefaultMetric == null)
                    return true;
                if (this.intrinsicMetrics != null && this.intrinsicMetrics.Count > 1)
                    return true;
                return false;
            }
        }

        public Metric MetricForName(string name)
        {
            Metric intrinsic = this.FindIntrinsicMetric(name);
            if (intrinsic != null)
                return intrinsic;
            foreach (Metric metric in this.InheritedMetrics)
            {
                if (metric.Name == name)
                    return metric;
            }
            return null;
        }
        public Metric FindIntrinsicMetric(string name)
        {
            if (this.intrinsicMetrics != null)
            {
                foreach (Metric metric in this.intrinsicMetrics)
                {
                    if (metric.Name == name)
                        return metric;
                }
            }
            return null;
        }


        #endregion

        #region Required by INumerifier<double>

        public double Combine(double a, double b)
        {
            return a + b;
        }
        public AdaptiveInterpolation.Distribution ConvertToDistribution(double a)
        {
            AdaptiveInterpolation.Distribution distribution = AdaptiveInterpolation.Distribution.MakeDistribution(a, 0, 1);
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
        public AdaptiveInterpolation.Distribution ConvertToDistribution(WillingnessSummary willingness)
        {
            double numParticipations = this.GetNumParticipations(willingness);
            double numSkips = willingness.NumSkips;

            AdaptiveInterpolation.Distribution distribution = new AdaptiveInterpolation.Distribution(numParticipations, numParticipations, numParticipations + numSkips);
            return distribution;
        }

        public WillingnessSummary Default()
        {
            return WillingnessSummary.Empty;
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
                bonus = (double)willingness.NumUnpromptedParticipations / ((double)willingness.NumUnpromptedParticipations + (double)willingness.NumSkips);
            
            return willingness.NumPromptedParticipations + bonus;
        }

        public void SetupPredictorsIfNeeded()
        {
            if (this.extraRatingPredictionLinks == null)
                this.SetupRatingPredictors();
        }

        public Distribution Ratings
        {
            get
            {
                this.ApplyPendingRatings();
                return this.ratingProgression.Distribution;

            }
        }

        // Smoothes by <smoothingWindowDuration> the participation intensities in <this> and matches them up against those of <activityToPredict>
        // The output coordinates are as defined by progressionToPredict
        // The input coordinates are measured in seconds (spend on this activity during the window)
        public List<Datapoint> compareParticipations(TimeSpan smoothingWindowDuration, LinearProgression progressionToPredict, DateTime cutoffDate)
        {
            // smoothing with a short duration is a hacky way of getting a LinearProgression that models the instantaneous rate of participation
            // ideally we'll add support directly into the LinearProgression class itself
            LinearProgression predictor = this.ParticipationsSmoothed(smoothingWindowDuration);
            
            StatList<DateTime, bool> union = new StatList<DateTime, bool>(new DateComparer(), new NoopCombiner<bool>());

            // find all the keys that either one contains
            foreach (DateTime date in progressionToPredict.Keys)
            {
                union.Add(date, true);
            }
            foreach (DateTime date in predictor.Keys)
            {
                union.Add(date, true);
            }

            // now compute the value of the formula
            DateTime prevDate = union.GetFirstValue().Key;
            double x1 = 0;
            double y1 = 0;
            List<Datapoint> results = new List<Datapoint>();
            foreach (ListItemStats<DateTime, bool> item in union.AllItems)
            {
                DateTime nextDate = item.Key;
                if (nextDate.CompareTo(prevDate) < 0)
                {
                    // skip going backwards, just in case
                    continue;
                }
                if (nextDate.CompareTo(cutoffDate) >= 0)
                {
                    // not enough data to compute the value for this window
                    // TODO: should we use a smaller window instead?
                    break;
                }
                double weight = nextDate.Subtract(prevDate).TotalSeconds;

                results.Add(new Datapoint(x1, y1, weight));
                double x2 = predictor.GetValueAt(nextDate, false).Value.Mean;
                double y2 = progressionToPredict.GetValueAt(nextDate, false).Value.Mean;
                results.Add(new Datapoint(x2, y2, weight));

                x1 = x2;
                y1 = y2;
                prevDate = nextDate;
            }

            return results;
        }

        public List<Participation> CommentedParticipations
        {
            get
            {
                List<Participation> results = new List<Participation>();
                foreach (Participation participation in this.Participations)
                {
                    if (participation.Comment != null)
                        results.Add(participation);
                }
                return results;
            }
        }

        // returns a list of Participations having comments, sorted by decreasing happiness score
        public List<Participation> CommentedParticipationsSortedByDecreasingScore
        {
            get
            {
                List<Participation> candidates = this.CommentedParticipations;
                candidates.Sort(new ParticipationScoreComparer());
                candidates.Reverse();
                return candidates;
            }
        }
        public List<Participation> getParticipationsSince(DateTime when)
        {
            this.ApplyPendingParticipations();
            return this.participationProgression.GetParticipationsSince(when);
        }
        public int GetNumParticipationsSince(DateTime when)
        {
            this.ApplyPendingParticipations();
            return this.participationProgression.GetNumParticipationsSince(when);
        }

        #endregion

        #region Private Member Functions

        private bool shouldIncludeRatingSummaryInInterpolator(ScoreSummary summary)
        {
            return summary.Item.Weight > 0;
        }


        private bool shouldUseNewlyAddedParentForPrediction(Activity parent)
        {
            // If the parent Doable has been done a bunch of times already, then it takes a long time to analyze it and we don't want that
            // If this Doable has been done a bunch of times already, then we already have enough information and don't need it
            if (parent.NumConsiderations < 1000)
                return true;
            else
                return false;
        }

        // initialize the PredictionLinks that estimate the rating of this Doable
        private void SetupRatingPredictors()
        {
            this.extraRatingPredictionLinks = new List<IPredictionLink>();

            this.ratingTrainingProgressions = new List<IProgression>();

            this.ratingTrainingProgressions.Add(TimeProgression.AbsoluteTime);

            this.ratingTrainingProgressions.Add(this.idlenessProgression);

            this.ratingTrainingProgressions.Add(this.participationProgression);

            this.ratingTrainingProgressions.Add(this.timeOfDayProgression);

            foreach (Activity parent in this.parents)
            {
                if (this.shouldUseNewlyAddedParentForPrediction(parent))
                {
                    this.parentsUsedForPrediction.Add(parent);

                    this.ratingTrainingProgressions.Add(parent.ratingProgression);

                    IPredictionLink link2 = new ExponentiallyWeightedPredictionLink(parent.ExpectedRatingProgression, this.RatingProgression, "Probably close to the rating of " + parent.Description);
                    this.extraRatingPredictionLinks.Add(link2);

                    IPredictionLink probabilityLink2 = new ConstantWeightedPredictionLink(parent.expectedParticipationProbabilityProgression, this.considerationProgression, "Probably just about as likely as " + parent.Description);
                    this.extraParticipationPredictionLinks.Add(probabilityLink2);
                }
            }

            this.SetupInterpolators();
        }

        public Engine engine
        {
            set
            {
                this.expectedRatingProgression.Engine = value;
                this.expectedParticipationProbabilityProgression.Engine = value;
            }
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
            this.shortTerm_EfficiencyInterpolator = new AdaptiveLinearInterpolator<Distribution>(new HyperBox<Distribution>(coordinates), new DistributionAdder());
        }
        // initialize the PredictionLinks that estimate the probability that the user will do this Doable
        private void SetupParticipationProbabilityPredictors()
        {
            this.extraParticipationPredictionLinks = new List<IPredictionLink>();

            this.participationTrainingProgressions = new List<IProgression>();
            this.participationTestingProgressions = new List<IProgression>();

            this.participationTrainingProgressions.Add(this.skipProgression);
            this.participationTestingProgressions.Add(this.skipProgression);

            this.participationTrainingProgressions.Add(this.considerationProgression);
            this.participationTestingProgressions.Add(this.considerationProgression);

            this.participationTrainingProgressions.Add(this.participationProgression);
            this.participationTestingProgressions.Add(this.participationProgression);

        }
        private void SetupParticipationProbabilityInterpolator()
        {
            List<Activity> activities = this.GetParticipationPredictionActivities();
            List<IProgression> progressions = new List<IProgression>(activities.Count * this.participationTrainingProgressions.Count + 1);
            progressions.Add(this.timeOfDayProgression);
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
        private List<Activity> parents;
        private List<Activity> parentsUsedForPrediction;
        private List<Activity> allAncestors;
        private List<ActivityDescriptor> parentDescriptors;

        //private List<IPredictionLink> ratingPredictors;
        List<IProgression> ratingTrainingProgressions;
        AdaptiveLinearInterpolator<Distribution> shortTerm_ratingInterpolator;  // this interpolator is used to estimate how happy the user feels after having done this Activity
        AdaptiveLinearInterpolator<Distribution> shortTerm_EfficiencyInterpolator;
        private List<IPredictionLink> extraRatingPredictionLinks;


        private RatingProgression ratingProgression;
        private ExpectedRatingProgression expectedRatingProgression;
        private AutoSmoothed_ParticipationProgression participationProgression;
        private IdlenessProgression idlenessProgression;
        private TimeProgression timeOfDayProgression;
        private TimeProgression timeOfWeekProgression;

        private List<IProgression> participationTrainingProgressions;
        private List<IProgression> participationTestingProgressions;
        AdaptiveLinearInterpolator<WillingnessSummary> participationInterpolator;
        private List<IPredictionLink> extraParticipationPredictionLinks;

        private ConsiderationProgression considerationProgression;
        private SkipProgression skipProgression;
        private ExpectedParticipationProbabilityProgression expectedParticipationProbabilityProgression;

        private int uniqueIdentifier;
        private DateTime? latestInteractionDate;
        private DateTime? earliestInteractionDate;
        private DateTime? latestParticipationDate;

        private DateTime? earliestInheritenceDate;
        private DateTime? latestInheritanceDate;
        private DateTime latestSuggestionDate = new DateTime(0);

        private DateTime defaultDiscoveryDate;
        private Distribution participationDurations;
        private List<Metric> inheritedMetrics;
        private List<Metric> intrinsicMetrics;
        
        int numSuggestions;

        Distribution ratingsWhenSuggested;
        Distribution ratingsWhenNotSuggested;
        Distribution thinkingTimes;
        ScoreSummarizer overallRatings_summarizer;
        ScoreSummarizer overallEfficiency_summarizer;

        #endregion

    }
}