using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The DateComparer class simply compares DateTime objects, to allow for use in Generics
namespace ActivityRecommendation
{
    public class DateComparer : IComparer<DateTime>
    {
        public int Compare(DateTime date1, DateTime date2)
        {
            return date1.CompareTo(date2);
        }
    }
}
