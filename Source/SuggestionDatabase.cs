using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// A SuggestionDatabase is a database of ActivitySuggestion objects
namespace ActivityRecommendation
{
    class SuggestionDatabase
    {
        public void AddSuggestion(ActivitySuggestion skip)
        {
            DateTime? maybeKey = skip.CreatedDate;
            if (maybeKey != null) // Some old skips might not know their creation date but we're mostly concerned with recent ones that do know
            {
                DateTime key = maybeKey.GetValueOrDefault();
                LinkedList<ActivitySuggestion> nearbySkips;
                if (!this.skipsByDate.ContainsKey(key))
                {
                    nearbySkips = new LinkedList<ActivitySuggestion>();
                    this.skipsByDate[key] = nearbySkips;
                }
                else
                {
                    nearbySkips = this.skipsByDate[key];
                }
                nearbySkips.AddLast(skip);
            }
        }
        public ActivitySuggestion GetSuggestion(ActivityDescriptor activityDescriptor, DateTime skipCreationDate)
        {
            LinkedList<ActivitySuggestion> candidates;
            if (!this.skipsByDate.TryGetValue(skipCreationDate, out candidates))
                return null;
            foreach (ActivitySuggestion skip in candidates)
            {
                if (activityDescriptor.CanMatch(skip.ActivityDescriptor))
                {
                    return skip;
                }
            }
            return null;
        }
        private Dictionary<DateTime, LinkedList<ActivitySuggestion>> skipsByDate = new Dictionary<DateTime, LinkedList<ActivitySuggestion>>();
    }
}
