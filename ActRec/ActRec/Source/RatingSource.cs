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
            RatingSource cause = new RatingSource("Participation");
            cause.Converted = source;
            return cause;
        }
        public static RatingSource FromSkip(ActivitySkip source)
        {
            RatingSource cause = new RatingSource("Skip");
            cause.Converted = source;
            return cause;
        }
        public static RatingSource FromRequest(ActivityRequest source)
        {
            RatingSource cause = new RatingSource("Request");
            cause.Converted = source;
            return cause;
        }
        public static RatingSource DirectRating(AbsoluteRating source)
        {
            RatingSource cause = new RatingSource("Direct");
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
        /*public static RatingSource GetSourceWithDescription(string description)
        {
            foreach (RatingSource source in RatingSource.AllSources)
            {
                if (source.Description.Equals(description))
                    return source;
            }
            return null;
        }*/

        public RatingSource(string sourceDescription)
        {
            this.description = sourceDescription;
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
        private string description;
    }
}
