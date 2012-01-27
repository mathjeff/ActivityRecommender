using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            this.latestParticipationDate = new DateTime(0);
            this.latestInteractionDate = new DateTime(0);
            this.uniqueIdentifier = nextID;
            this.defaultDiscoveryDate = DateTime.Now;
            this.participationDurations = Distribution.MakeDistribution(3600, 0, 1);    // default duration of an activity is 1 hour
            nextID++;
        }

        // initialize the Progressions from which we predict different values
        private void SetupProgressions()
        {
            this.ratingProgression = new RatingProgression(this);
            this.participationProgression = new ParticipationProgression(this);
            this.idlenessProgression = new IdlenessProgression(this);
            this.timeOfDayProgression = new TimeProgression(new DateTime(), new TimeSpan(24, 0, 0));
            this.considerationProgression = new ConsiderationProgression(this);
            this.skipProgression = new SkipProgression(this);
            this.expectedRatingProgression = new ExpectedRatingProgression(this);
            this.expectedParticipationProbabilityProgression = new ExpectedParticipationProbabilityProgression(this);
        }
        // initialize the PredictionLinks that estimate the rating of this Activity
        private void SetupRatingPredictors()
        {
            this.ratingPredictors = new List<IPredictionLink>();

            PredictionLink predictorFromOwnRatings = new PredictionLink(this.ratingProgression, this.ratingProgression);
            predictorFromOwnRatings.InitializeIncreasing();
            // It actually turns out that the Root(Mean(Squared(Error))) goes down if we don't use this
            //this.ratingPredictors.Add(predictorFromOwnRatings);

            PredictionLink predictorFromOwnParticipations = new PredictionLink(this.participationProgression, this.ratingProgression);
            this.ratingPredictors.Add(predictorFromOwnParticipations);

            PredictionLink predictorFromOwnIdleness = new PredictionLink(this.idlenessProgression, this.ratingProgression);
            this.ratingPredictors.Add(predictorFromOwnIdleness);

            PredictionLink predictorFromTimeOfDay = new PredictionLink(this.timeOfDayProgression, this.ratingProgression);
            predictorFromTimeOfDay.InputWrapsAround = true;
            this.ratingPredictors.Add(predictorFromTimeOfDay);
        }
        // initialize the PredictionLinks that estimate the probability that the user will do this activity
        private void SetupParticipationProbabilityPredictors()
        {
            this.participationProbabilityPredictors = new List<IPredictionLink>();
            this.participationProbabilityPredictors.Add(new PredictionLink(this.skipProgression, this.considerationProgression));
            this.participationProbabilityPredictors.Add(new PredictionLink(this.idlenessProgression, this.considerationProgression));
            //this.participationProbabilityPredictors.Add(new PredictionLink(this.timeOfDayProgression, this.considerationProgression));
            //this.participationProbabilityPredictors.Add(new PredictionLink(this.participationProgression, this.considerationProgression));
            this.participationProbabilityPredictors.Add(new PredictionLink(this.considerationProgression, this.considerationProgression));
            this.participationProbabilityPredictors.Add(new PredictionLink(this.expectedRatingProgression, this.considerationProgression));
            this.participationProbabilityPredictors.Add(new PredictionLink(this.ratingProgression, this.considerationProgression));
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

                PredictionLink link1 = new PredictionLink(newParent.RatingProgression, newParent.ExpectedRatingProgression, this.RatingProgression);
                link1.Justification = "predicted based on the rating of " + newParent.Description;
                this.ratingPredictors.Add(link1);

                SimplePredictionLink link2 = new SimplePredictionLink(newParent.ExpectedRatingProgression, this.RatingProgression, "Probably close to the rating of " + newParent.Description);
                this.ratingPredictors.Add(link2);

                //PredictionLink probabilityLink1 = new PredictionLink(newParent.considerationProgression, newParent.expectedParticipationProbabilityProgression, this.considerationProgression);
                //probabilityLink1.Justification = "predicted based on the probability of doing " + newParent.Description;
                //this.participationProbabilityPredictors.Add(probabilityLink1);

                SimplePredictionLink probabilityLink2 = new SimplePredictionLink(newParent.expectedParticipationProbabilityProgression, this.considerationProgression, "Probably just about as likely as " + newParent.Description);
                this.participationProbabilityPredictors.Add(probabilityLink2);
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
            {
                this.latestInteractionDate = when;
            }
            if ((this.earliestInteractionDate == null) || (((DateTime)this.earliestInteractionDate).CompareTo(when) > 0))
            {
                this.earliestInteractionDate = when;
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
        public void AddRating(AbsoluteRating newRating)
        {
            // keep track of the ratings
            this.ratingProgression.AddRating(newRating);
            // keep track of the latest date at which anything happened
            if (newRating.Date != null)
                this.ApplyKnownInteractionDate((DateTime)newRating.Date);
        }
        public void AddParticipation(Participation newParticipation)
        {
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
        }
        public void AddSkip(ActivitySkip newSkip)
        {
            this.skipProgression.AddSkip(newSkip);
            this.considerationProgression.AddSkip(newSkip);
            // keep track of the earliest and latest date at which anything happened
            this.ApplyKnownInteractionDate(newSkip.Date);
        }
        // returns a bunch of estimates about how it will be rated at this date
        public List<Prediction> GetRatingEstimates(DateTime when)
        {
            List<Prediction> predictions = new List<Prediction>();
            foreach (IPredictionLink link in this.ratingPredictors)
            {
                predictions.Add(link.Guess(when));
            }
            return predictions;
        }
        // returns a bunch of estimates about the probability that the user would do this activity if it were suggested now
        public List<Prediction> GetParticipationProbabilityEstimates(DateTime when)
        {
            List<Prediction> predictions = new List<Prediction>();
            foreach (IPredictionLink link in this.participationProbabilityPredictors)
            {
                predictions.Add(link.Guess(when));
            }
            return predictions;
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

        private List<IPredictionLink> ratingPredictors;

        private RatingProgression ratingProgression;
        private ExpectedRatingProgression expectedRatingProgression;
        //private PredictionLink predictorFromOwnRatings;
        private ParticipationProgression participationProgression;
        //private PredictionLink predictorFromOwnParticipations;
        private IdlenessProgression idlenessProgression;
        //private PredictionLink predictorFromOwnIdleness;
        private TimeProgression timeOfDayProgression;
        //private PredictionLink predictorFromTimeOfDay;

        private List<IPredictionLink> participationProbabilityPredictors;

        private ConsiderationProgression considerationProgression;
        private SkipProgression skipProgression;
        private ExpectedParticipationProbabilityProgression expectedParticipationProbabilityProgression;
        //private PredictionLink 

        //private List<IPredictionLink> parentPredictionLinks;    // a list of all PredictionLinks that are used to predict the value of this Activity's RatingProgression from parent ratings

        
        private int uniqueIdentifier;
        private DateTime? latestInteractionDate;
        private DateTime? earliestInteractionDate;
        private DateTime? latestParticipationDate;
        private DateTime defaultDiscoveryDate;
        private Distribution participationDurations;

        #endregion

    }
}