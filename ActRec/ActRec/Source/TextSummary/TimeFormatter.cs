using System;
using System.Collections.Generic;
using System.Text;

// Converts DateTime's into strings
namespace ActivityRecommendation.TextSummary
{
    class TimeFormatter
    {
        public static string summarizeTimespan(DateTime startDate, DateTime endDate)
        {
            DateTime now = DateTime.Now;
            if (endDate.Day.Equals(now.Day))
            {
                return summarizeTimespanSince(startDate, now);
            }
            if (startDate.Day.Equals(endDate.Day))
            {
                return "On " + startDate.ToString("MM/dd/yyyy") + " from " + startDate.ToString("HH:mm") + " to " + endDate.ToString("HH:mm");
            }
            return "Between " + startDate + " and " + endDate;
        }

        private static string summarizeTimespanSince(DateTime startDate, DateTime now)
        {
            if (startDate.Day != now.Day)
                return "Since " + startDate.ToString("MM/dd/yyyy");
            else
                return "From " + startDate.ToString("HH:mm") + " to " + now.ToString("HH:mm");
        }

    }
}
