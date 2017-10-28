using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The RatingSource class tells where a rating came from
namespace ActivityRecommendation
{
    public class RatingSource
    {
        public static RatingSource FromParticipation(Participation source)
        {
            RatingSource cause = new RatingSource("Participation", true, false, true);
            cause.Converted = source;
            return cause;
        }
        public static RatingSource FromSkip(ActivitySkip source)
        {
            RatingSource cause = new RatingSource("Skip", false, true, false);
            cause.Converted = source;
            return cause;
        }
        public static RatingSource FromRequest(ActivityRequest source)
        {
            RatingSource cause = new RatingSource("Request", false, true, false);
            cause.Converted = source;
            return cause;
        }
        public static RatingSource DirectRating(AbsoluteRating source)
        {
            RatingSource cause = new RatingSource("Direct", false, true, true);
            cause.Converted = source;
            return cause;
        }
        public static List<RatingSource> AllSources
        {
            get
            {
                List<RatingSource> sources = new List<RatingSource>();
                sources.Add(RatingSource.FromParticipation(null));
                sources.Add(RatingSource.FromSkip(null));
                sources.Add(RatingSource.FromRequest(null));
                sources.Add(RatingSource.DirectRating(null));
                return sources;
            }
        }
        public static RatingSource GetSourceWithDescription(string description)
        {
            foreach (RatingSource source in RatingSource.AllSources)
            {
                if (source.Description.Equals(description))
                    return source;
            }
            return null;
        }

        public RatingSource(string sourceDescription, bool basedOnPastEvent, bool dateExact, bool scoreExact)
        {
            this.description = sourceDescription;
            this.isBasedOnPastEvent = basedOnPastEvent;
            this.isDateExact = dateExact;
            this.isScoreExact = scoreExact;
        }
        // Returns true if it comes from the user's evaluation of a future event. This would be something like "I think I would like to listen to music."
        public bool IsBasedOnFutureEvent()
        {
            return !this.isBasedOnPastEvent;
        }

        // Returns true if it comes from the user's evaluation of a past event. This would be something like "I watched TV and it was horrible."
        public bool IsBasedOnPastEvent()
        {
            return this.isBasedOnPastEvent;
        }
        
        // returns true if all of the information stored in the rating was directly provided by the user
        // If, for example, the user skipped an activity and we had to create a fake rating for it, then WasCreatedDirectlyByUser() returns false
        public bool WasCreatedDirectlyByUser()
        {
            if (!isDateExact)
                return false;
            if (!isScoreExact)
                return false;
            return true;
        }
        
        // Returns true if the date was directly provided by the user. Returns false if we had to make a reasonable estimate for the date
        public bool IsDateExact()
        {
            return this.isDateExact;
        }
        
        // Returns true if the rating is numerically equal to the value provided by the user. Returns false if we had to calculate a value arbitrarily
        public bool IsScoreExact()
        {
            return this.isScoreExact;
        }
        
        // a string describing the rating source
        public string Description
        {
            get
            {
                return this.description;
            }
        }

        // an object that generated the rating
        public object Converted
        {
            get;
            set;
        }
        public Participation ConvertedAsParticipation
        {
            get
            {
                return this.Converted as Participation;
            }
            set
            {
                this.Converted = value;
            }
        }

        
        // private
        private bool isBasedOnPastEvent;
        private bool isDateExact;
        private bool isScoreExact;
        private string description;
    }
}
