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
        public event ActivityAddedHandler ActivityAdded;
        public delegate void ActivityAddedHandler(object sender, EventArgs e);

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
            double bestMatchScore = 0;
            // figure out which activity matches best
            foreach (Activity activity in activities)
            {
                double matchScore = this.MatchQuality(descriptor, activity);
                if (matchScore > bestMatchScore)
                {
                    result = activity;
                    bestMatchScore = matchScore;
                }
            }
            // now we've found the activity that is indicated by that descriptor
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
                return 1;
            }
            else
            {
                // a bunch of points based on string similarity
                string desiredName = descriptor.ActivityName;
                // if the user enters a string that it rediculously long, then we don't bother comparing it
                if (desiredName.Length < 2 * activity.Name.Length)
                {
                    //matchScore += 0.5 * this.EditScore(desiredName, activity.Name);
                    matchScore = this.stringScore(activity.Name, desiredName);
                }
            }
            // now that we've verified that this activity is allowed to be a match, we calculate its score

            // +1 point if the 'Choosable' property matches
            if (descriptor.Choosable != null && descriptor.Choosable.Value == activity.Choosable)
                matchScore += 1;


            // up to 0.5 extra points based on the likelihood that the user did this activity
            if (descriptor.PreferMorePopular)
            {
                // Give better scores to activities that the user has logged more often
                // Ideally, we would actually only count participations that were directly assigned to this activity to begin with
                // but that's not information that we often need and this probably isn't enough of a reason to track it for this case
                if (activity.Choosable)
                    matchScore += (1.0 - 1.0 / ((double)activity.NumParticipations + 1.0));
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
        public List<Activity> LeafActivities
        {
            get
            {
                List<Activity> results = new List<Activity>();
                foreach (Activity activity in this.allActivities)
                {
                    if (activity.Children.Count == 0)
                        results.Add(activity);
                }
                return results;
            }

        }
        #endregion

        // constructs an Activity from the given ActivityDescriptor
        private Activity CreateActivity(ActivityDescriptor sourceDescriptor)
        {
            Activity result = new Activity(sourceDescriptor.ActivityName, this.ratingSummarizer);
            if (sourceDescriptor.Choosable != null)
                result.Choosable = sourceDescriptor.Choosable.Value;
            //result.AddParent(this.rootActivity);
            if (this.ActivityAdded != null)
                this.ActivityAdded.Invoke(this, new EventArgs());
            return result;
        }

        public void AssignDefaultParent()
        {
            foreach (Activity activity in this.allActivities)
            {
                if (activity.Parents.Count < 1 && activity != this.rootActivity)
                    activity.Parents.Add(this.rootActivity);
            }
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

        

        /*double stringMatchScore(string item, string query)
        {
            double caseSensitiveScore = this.numWordPrefixMatches(item, query);
            double caseInsensitiveScore = this.numWordPrefixMatches(item, query);

            return caseSensitiveScore + caseInsensitiveScore;
        }*/

        int stringScore(string item, string query)
        {
            int totalScore = 0;
            if (item == query)
                totalScore++;
            if (item.ToLower() == query.ToLower())
                totalScore++;
            char separator = ' ';
            List<string> itemWords = new List<string>(item.Split(separator));
            string[] queryWords = query.Split(separator);

            foreach (string queryWord in queryWords)
            {
                if (queryWord.Length < 1)
                    continue;
                string queryWordLower = queryWord.ToLower();
                for (int i = 0; i < itemWords.Count; i++)
                {
                    string itemWord = itemWords[i];
                    string itemWordLower = itemWord.ToLower();

                    int matchScore = 0;
                    if (itemWordLower.StartsWith(queryWordLower))
                        matchScore += 2;
                    if (itemWord.StartsWith(queryWord))
                        matchScore++;
                    if (itemWordLower == queryWordLower)
                        matchScore++;
                    if (itemWord == queryWord)
                        matchScore++;
                    if (matchScore > 0)
                    {
                        if (i == 0)
                            matchScore++;
                        totalScore += matchScore * queryWord.Length;
                        itemWords.RemoveAt(i);
                        break;
                    }
                }
            }

            return totalScore;
        }
        #endregion

        #region Private Variables

        private List<Activity> allActivities;
        private StatList<string, IEnumerable<Activity>> activitiesByName;
        private RatingSummarizer ratingSummarizer;
        private Activity rootActivity;

        #endregion
    }
}
