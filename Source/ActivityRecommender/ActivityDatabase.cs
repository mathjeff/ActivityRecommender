using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// An ActivityDatabase class stores all of the known Activities, for the purpose of resolving a name into an Activity
namespace ActivityRecommendation
{
    public class ActivityDatabase : IComparer<string>, IAdder<IEnumerable<Activity>>
    {
        #region Constructor

        public ActivityDatabase()
        {
            //this.activitiesByName = new Dictionary<string, List<Activity> >();
            this.activitiesByName = new StatList<string, IEnumerable<Activity>>(this, this);
            this.allActivities = new List<Activity>();
        }

        #endregion

        #region Public Member Functions

        // returns the newly created Activity, or null if none was created
        public Activity AddOrCreateActivity(ActivityDescriptor descriptor)
        {
            // attempt to find an activity that matches
            Activity activity = this.ResolveDescriptor(descriptor);
            if (activity == null)
            {
                // if this descriptor indicates a new activity, then create it
                activity = this.CreateActivity(descriptor);
                this.AddActivity(activity);
                return activity;
            }
            // no Activity was created, so we don't return one
            return null;
        }
        // puts an Activity in the database
        public void AddActivity(Activity newActivity)
        {
            string activityName = newActivity.Name;
            // make a list containing just this Activity
            List<Activity> activityList = new List<Activity>();
            activityList.Add(newActivity);
            // add it to the database
            this.activitiesByName.Add(newActivity.Name, activityList);
            // add it to the list of all activities
            this.allActivities.Add(newActivity);
        }

        // finds the Activity indicated by the ActivityDescriptor
        public Activity ResolveDescriptor(ActivityDescriptor descriptor)
        {
            // get the required prefix for the name of the activity
            string firstValidName = descriptor.NamePrefix;
            Activity result = null;
            if (firstValidName == null)
            {
                firstValidName = "";
            }
            // determine the last (in dictionary order) acceptable name for a matching activity
            string lastValidName;
            int i;
            for (i = firstValidName.Length - 1; i >= 0; i--)
            {
                //if (firstValidName[i] != Char.MaxValue)
                if (firstValidName[i] != 'z' && firstValidName[i] != 'Z')
                {
                    break;
                }
            }
            if (i < 0)
            {
                lastValidName = null;
            }
            else
            {
                char lastChar = (char)(firstValidName[i] + 1);
                lastValidName = firstValidName.Substring(0, i) + lastChar;
                //lastValidName[i] = lastValidName[i] + 1;
            }
            // get a list of all activities with names in that range
            IEnumerable<Activity> activities = null; 
            if (lastValidName == null)
            {
                activities = this.activitiesByName.SumAfterKey(firstValidName, true);
            }
            else
            {
                activities = this.activitiesByName.SumBetweenKeys(firstValidName, true, lastValidName, false);
            }
            double bestMatchScore = 0;
            int count = 0;
            // figure out which activity matches best
            foreach (Activity activity in activities)
            {
                count++;
                double matchScore = this.MatchQuality(descriptor, activity);
                if (matchScore > bestMatchScore)
                {
                    result = activity;
                    bestMatchScore = matchScore;
                }
                // if we don't need the best match, then just return once we've found the best from among few matches
                if (count > 20 && result != null && !descriptor.RequiresPerfectMatch)
                {
                    break;
                }
            }
            // now we've found the activity that is indicated by that descriptor
            //descriptor.Activity = result;
            return result;
        }
        // returns 0 if there is a discrepancy, otherwise 1 + (the number of fields that match)
        public double MatchQuality(ActivityDescriptor descriptor, Activity activity)
        {
            double matchScore = 1; // if there are no problems, we default to a match quality of 1
            // require that the name matches
            if (descriptor.ActivityName != null)
            {
                if (!descriptor.ActivityName.Equals(activity.Name))
                {
                    return 0;
                }
            }
            // require that the 'Choosable' property matches
            if (descriptor.Choosable != null)
            {
                if (descriptor.Choosable.Value != activity.Choosable)
                {
                    return 0;
                }
                else
                {
                    matchScore += 1;
                }
            }
            // prefer an activity where the name prefix is the entire name
            if (string.Compare(descriptor.NamePrefix, activity.Name, true) == 0)
            {
                matchScore += 2;
            }
            // prefer an activity where the case matches exactly
            if (activity.Name.StartsWith(descriptor.NamePrefix, StringComparison.Ordinal))
            {
                matchScore += 1;
            }
            if (descriptor.PreferBetterRatings)
            {
                // give better scores to activities that the user will probably rate higher
                //Console.WriteLine("Considering activity named" + activity.Name.ToString());
                matchScore += activity.SuggestionValue.Mean;
                //Console.WriteLine("score = " + matchScore.ToString());
            }
            return matchScore;
        }
        public int NumActivities
        {
            get
            {
                return this.allActivities.Count;
            }
        }
        public List<Activity> AllActivities
        {
            get
            {
                return this.allActivities;
            }
        }
        public void Clear()
        {
            this.activitiesByName.Clear();
        }
        #endregion

        // constructs an Activity from the given ActivityDescriptor
        private Activity CreateActivity(ActivityDescriptor sourceDescriptor)
        {
            Activity result = new Activity(sourceDescriptor.ActivityName);
            result.Choosable = sourceDescriptor.Choosable.GetValueOrDefault(true);
            return result;
        }

        #region Functions for IAdder<List<Activity>>
        public IEnumerable<Activity> Sum(IEnumerable<Activity> list1, IEnumerable<Activity> list2)
        {
            return list1.Concat(list2);
            //return null;
        }
        public IEnumerable<Activity> Zero()
        {
            return new List<Activity>();
        }
        #endregion

        #region Functions for IComparer<string>
        public int Compare(string string1, string string2)
        {
            //return string.CompareOrdinal(string1, string2);
            return string.Compare(string1, string2, true);
        }
        #endregion

        #region Private Variables

        //private Dictionary<string, List<Activity> > activitiesByName;
        private List<Activity> allActivities;
        private StatList<string, IEnumerable<Activity>> activitiesByName;

        #endregion
    }
}
