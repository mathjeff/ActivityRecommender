using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The RatingSource class tells where a rating came from
namespace ActivityRecommendation
{
    public class RatingSource
    {
        public RatingSource(bool basedOnPastEvent, bool createdDirectlyByUser)
        {
            this.isBasedOnPastEvent = basedOnPastEvent;
            this.wasCreatedDirectlyByUser = createdDirectlyByUser;
        }
        // returns true if it comes from the user's evaluation of a future event
        public bool IsBasedOnFutureEvent()
        {
            return !this.isBasedOnPastEvent;
        }
        // returns true if it comes from the user's evaluation of a past event
        public bool IsBasedOnPastEvent()
        {
            return this.isBasedOnPastEvent;
        }
        // returns true if this rating was created in this form by the user
        public bool WasCreatedDirectlyByUser()
        {
            return this.wasCreatedDirectlyByUser;
        }
        // private
        bool isBasedOnPastEvent;
        bool wasCreatedDirectlyByUser;
    }
}
