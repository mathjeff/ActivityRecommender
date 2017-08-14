using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// An ActivityDatabase class stores all of the known Activities, for the purpose of resolving a name into an Activity
namespace ActivityRecommendation
{
    public class ActivityDatabase : IComparer<string>, ICombiner<IEnumerable<Activity>>
    {
        #region Constructor

        public ActivityDatabase(RatingSummarizer ratingSummarizer)
        {
            //this.activitiesByName = new Dictionary<string, List<Activity> >();
            this.activitiesByName = new StatList<string, IEnumerable<Activity>>(this, this);
            this.allActivities = new List<Activity>();
            this.ratingSummarizer = ratingSummarizer;
            this.rootActivity = new Activity("Activity", ratingSummarizer);
            this.rootActivity.Choosable = false;
            this.AddActivity(this.rootActivity);
        }

        #endregion

        #region Public Member Functions

        // returns the newly created Activity, or null if none was created
        public Activity CreateActivityIfMissing(ActivityDescriptor descriptor)
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

        public Activity GetOrCreate(ActivityDescriptor descriptor)
        {
            Activity activity = this.ResolveDescriptor(descriptor);
            if (activity == null)
                activity = this.CreateActivityIfMissing(descriptor);
            return activity;
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
            IEnumerable<Activity> activities = null;
            if (descriptor.RequiresPerfectMatch)
            {
                // requiring a perfect match means that we can do a sorted lookup to find the activity by name
                activities = this.activitiesByName.CombineBetweenKeys(descriptor.ActivityName, true, descriptor.ActivityName, true);
            }
            else
            {
                // if we allow approximate string matches, then we have to check all activities
                activities = this.activitiesByName.CombineAll();
            }
            Activity result = null;
            /*
            // get the required prefix for the name of the activity
            string firstValidName = descriptor.ActivityName;
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
                activities = this.activitiesByName.CombineAfterKey(firstValidName, true);
            }
            else
            {
                activities = this.activitiesByName.CombineBetweenKeys(firstValidName, true, lastValidName, false);
            }*/
            double bestMatchScore = 0;
            //int count = 0;
            // figure out which activity matches best
            foreach (Activity activity in activities)
            {
                //count++;
                double matchScore = this.MatchQuality(descriptor, activity);
                if (matchScore > bestMatchScore)
                {
                    result = activity;
                    bestMatchScore = matchScore;
                }
                // if we don't need the best match, then just return once we've found the best from among few matches
                /* if (count > 20 && result != null && !descriptor.RequiresPerfectMatch)
                {
                    break;
                }*/
            }
            // now we've found the activity that is indicated by that descriptor
            //descriptor.Activity = result;
            return result;
        }
        // tells whether the given descriptor can match the given activity
        public bool Matches(ActivityDescriptor descriptor, Activity activity)
        {
            if (this.MatchQuality(descriptor, activity) > 0)
                return true;
            else
                return false;

        }
        // returns 0 if there is a discrepancy, otherwise 1 + (the number of fields that match)
        public double MatchQuality(ActivityDescriptor descriptor, Activity activity)
        {
            double matchScore = 1; // if there are no problems, we default to a match quality of 1
            if (descriptor.RequiresPerfectMatch)
            {
                // make sure the name matches
                if (descriptor.ActivityName != null && !descriptor.ActivityName.Equals(activity.Name))
                    return 0;
                // make sure the 'Choosable' property matches
                // require that the 'Choosable' property matches
                if (descriptor.Choosable != null && descriptor.Choosable.Value != activity.Choosable)
                    return 0;
            }
            else
            {
                // up to 0.5 extra points based on the string similarity
                string desiredName = descriptor.ActivityName;
                // if the user enters a string that it rediculously long, then we don't bother comparing it
                if (desiredName.Length < 2 * activity.Name.Length)
                {
                    matchScore += 0.5 * this.EditScore(desiredName, activity.Name);
                }
            }
            // now that we've verified that this activity is allowed to be a match, we calculate its score

            // +1 point if the descriptor's text is a prefix for the actual name
            if (activity.Name.StartsWith(descriptor.ActivityName, StringComparison.CurrentCultureIgnoreCase))
            {
                matchScore += 1;


                // +1 point if the name matches without case
                if (descriptor.ActivityName.Equals(activity.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    matchScore += 1;


                    // +1 point if the name matches with case
                    if (descriptor.ActivityName.Equals(activity.Name))
                    {
                        matchScore += 1;
                    }
                }
            }

            // +1 point if the 'Choosable' property matches
            if (descriptor.Choosable != null && descriptor.Choosable.Value == activity.Choosable)
                matchScore += 1;


            // up to 0.5 extra points based on the likelihood that the user did this activity
            if (descriptor.PreferHigherProbability)
            {
                // give better scores to activities that the user will probably rate higher
                if (activity.PredictedParticipationProbability.Distribution != null)
                    matchScore += 0.5 * activity.PredictedParticipationProbability.Distribution.Mean;
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
        public Activity RootActivity
        {
            get
            {
                return this.rootActivity;
            }
        }
        #endregion

        // constructs an Activity from the given ActivityDescriptor
        private Activity CreateActivity(ActivityDescriptor sourceDescriptor)
        {
            Activity result = new Activity(sourceDescriptor.ActivityName, this.ratingSummarizer);
            if (sourceDescriptor.Choosable != null)
                result.Choosable = sourceDescriptor.Choosable.Value;
            result.AddParent(this.rootActivity);
            return result;
        }

        #region Functions for ICombiner<List<Activity>>
        public IEnumerable<Activity> Combine(IEnumerable<Activity> list1, IEnumerable<Activity> list2)
        {
            return list1.Concat(list2);
            //return null;
        }
        public IEnumerable<Activity> Default()
        {
            return new List<Activity>();
        }
        #endregion

        #region Functions for IComparer<string>
        public int Compare(string string1, string string2)
        {
            return string.Compare(string1, string2, StringComparison.CurrentCultureIgnoreCase);
        }
        #endregion


        #region Private Member Functions
        // computes the cost of transforming string1 into string2
        double EditScore(string string1, string string2)
        {
            int numRows = string1.Length + 1;
            int numColumns = string2.Length + 1;
            double[,] scores = new double[numRows, numColumns];
            int i, j;
            double gapScore = -1;
            double matchWithCaseScore = 1;
            double matchWithoutCaseScore = 0.5;
            double mismatchScore = -1;
            double bestScore = 0;
            for (i = 0; i < numRows; i++)
            {
                scores[i, 0] = 0;
            }
            for (j = 0; j < numColumns; j++)
            {
                scores[0, j] = 0;
            }
            double startingScore = 1;
            scores[0, 0] = startingScore;
            // iterate over all the possible combinations to find the best score
            for (i = 1; i < numRows; i++)
            {
                for (j = 1; j < numColumns; j++)
                {
                    double a, b, c;
                    a = scores[i, j - 1] + gapScore;
                    // check whether they're the same letter (ignoring case)
                    if (string.Compare(string1.Substring(i - 1, 1), string2.Substring(j - 1, 1), StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        // check whether the case is the same
                        if (string1[i - 1] == string2[j - 1])
                            b = scores[i - 1, j - 1] + matchWithCaseScore;
                        else
                            b = scores[i - 1, j - 1] + matchWithoutCaseScore;
                    }
                    else
                    {
                        // different letters
                        b = scores[i - 1, j - 1] + mismatchScore;
                    }
                    c = scores[i - 1, j] + gapScore;
                    // record the best possible score of matching the first i letters of string1 with the first j letters of string2
                    scores[i, j] = Math.Max(Math.Max(a, b), Math.Max(c, 0));
                    // update the score of the highest-scoring match between subsequences
                    bestScore = Math.Max(scores[i, j], bestScore);
                }
            }
            // the best possible score equals minlength
            // now we rescale it to the range [0,1]
            double scaledScore = bestScore / (Math.Min(string1.Length, string2.Length) + startingScore);
            return bestScore;
        }
        #endregion

        #region Private Variables

        //private Dictionary<string, List<Activity> > activitiesByName;
        private List<Activity> allActivities;
        private StatList<string, IEnumerable<Activity>> activitiesByName;
        private RatingSummarizer ratingSummarizer;
        private Activity rootActivity;

        #endregion
    }
}
