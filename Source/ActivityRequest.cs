using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// An ActivityRequest embodies the statement "I want to do an activity in the category Fun" (or some other category)
// The fact that the user requested a suggestion from that category means that that category gets a better rating
namespace ActivityRecommendation
{
    public class ActivityRequest
    {
        public ActivityRequest()
        {
        }
        public ActivityRequest(ActivityDescriptor categoryDescriptor, DateTime when)
        {
            this.ActivityDescriptor = categoryDescriptor;
            this.Date = when;
        }

        public ActivityDescriptor ActivityDescriptor { get; set; }
        public DateTime Date { get; set; }
        public Rating RawRawing { get; set; }
        public Rating GetCompleteRating()
        {
            return null;
            /*
            Rating rating;
            if (this.RawRawing != null)
                rating = this.RawRawing.MakeCopy();
            else
                rating = new AbsoluteRating();
            rating.FillInFromRequest(this);
            return rating;
            */
        }
    }
}
