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
            this.ratingProgression = new RatingProgression(this);
            this.participationProgression = new ParticipationProgression(this);
            this.predictorFromOwnRatings = new PredictionLink(this.ratingProgression, this.ratingProgression);
            this.predictorFromOwnRatings.InitializeIncreasing();
            this.predictorFromOwnParticipations = new PredictionLink(this.participationProgression, this.ratingProgression);
            this.parentPredictionLinks = new List<PredictionLink>();
            this.latestRatingEstimationDate = new DateTime(0);
            this.latestParticipationDate = new DateTime(0);
            this.latestInteractionDate = new DateTime(0);
            this.uniqueIdentifier = nextID;
            this.defaultDiscoveryDate = DateTime.Now;
            nextID++;
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
                PredictionLink newLink = new PredictionLink(newParent.RatingProgression, this.RatingProgression);
                this.AddPredictionLink(newLink);
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
        public ParticipationProgression ParticipationProgression
        {
            get
            {
                return this.participationProgression;
            }
        }
        public void AddPredictionLink(PredictionLink newLink)
        {
            this.parentPredictionLinks.Add(newLink);
        }
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
        public Distribution SuggestionValue { get; set; }
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

        public List<PredictionLink> ParentPredictionLinks
        {
            get
            {
                return this.parentPredictionLinks;
            }
        }
        public int NumRatings
        {
            get
            {
                return this.ratingProgression.NumRatings;
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
        public void AddRating(AbsoluteRating newRating)
        {
            // keep track of the ratings
            this.ratingProgression.AddRating(newRating);
            // keep track of the latest date at which anything happened
            this.ApplyKnownInteractionDate(newRating.Date);
        }
        public void AddParticipation(Participation newParticipation)
        {
            // keep track of the participation
            this.participationProgression.AddParticipation(newParticipation);
            // keep track of the earliest and latest date at which anything happened
            this.ApplyKnownInteractionDate(newParticipation.StartDate);
            this.ApplyKnownInteractionDate(newParticipation.EndDate);
            DateTime when = newParticipation.EndDate;
            // keep track of the latest date at which the user interacted with the activity
            if (when.CompareTo(this.latestParticipationDate) > 0)
            {
                this.latestParticipationDate = when;
            }
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
        private RatingProgression ratingProgression;
        private ParticipationProgression participationProgression;
        private List<PredictionLink> parentPredictionLinks;    // a list of all PredictionLinks that are used to predict the value of this Activity's RatingProgression from parent ratings
        private PredictionLink predictorFromOwnRatings;
        private PredictionLink predictorFromOwnParticipations;
        private int uniqueIdentifier;
        DateTime? latestInteractionDate;
        DateTime? earliestInteractionDate;
        DateTime? latestParticipationDate;
        DateTime defaultDiscoveryDate;
        #endregion

    }
}