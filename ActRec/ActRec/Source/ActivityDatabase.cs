﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// An ActivityDatabase class stores all of the known Activities, for the purpose of resolving a name into an Activity
namespace ActivityRecommendation
{

    public interface ReadableActivityDatabase
    {
        Activity ResolveDescriptor(ActivityDescriptor activityDescriptor);
        IEnumerable<Activity> AllActivities { get; }
    }

    public class ActivityDatabase : IComparer<string>, ICombiner<IEnumerable<Activity>>, ReadableActivityDatabase
    {
        public event ActivityAddedHandler ActivityAdded;
        public delegate void ActivityAddedHandler(object sender, Activity activity);

        public event InheritanceAddedHandler InheritanceAdded;
        public delegate void InheritanceAddedHandler(object sender, Inheritance inheritance);

        #region Constructor

        public ActivityDatabase(RatingSummarizer ratingSummarizer)
        {
            //this.activitiesByName = new Dictionary<string, List<Activity> >();
            this.activitiesByName = new StatList<string, IEnumerable<Activity>>(this, this);
            this.allActivities = new List<Activity>();
            this.ratingSummarizer = ratingSummarizer;
            this.rootActivity = new Category("Activity", ratingSummarizer);
            this.rootActivity.setChooseable(false);
            this.AddActivity(this.rootActivity);
        }

        #endregion

        #region Public Member Functions

        // returns a string other than "" in case of error
        public string CreateCategory(Inheritance inheritance)
        {
            string err = this.ValidateCandidateNewInheritance(inheritance);
            if (err != "")
                return err;
            ActivityDescriptor childDescriptor = inheritance.ChildDescriptor;
            Activity child = this.CreateCategory(inheritance.ChildDescriptor);
            return this.AddParent(inheritance);
        }

        public string CreateToDo(Inheritance inheritance)
        {
            string err = this.ValidateCandidateNewInheritance(inheritance);
            if (err != "")
                return err;
            ToDo toDo = this.CreateToDo(inheritance.ChildDescriptor);
            return this.AddParent(inheritance);
        }

        public string AddParent(Inheritance inheritance)
        {
            if (inheritance.ChildDescriptor == null)
                return "Child name is required";
            Activity child = this.ResolveDescriptor(inheritance.ChildDescriptor);
            if (child == null)
                return "Child " + inheritance.ChildDescriptor.ActivityName + " does not exist";
            if (inheritance.ParentDescriptor == null)
                return "Parent name is required";
            Activity parent = this.ResolveDescriptor(inheritance.ParentDescriptor);
            if (parent == null)
                return "Parent " + inheritance.ParentDescriptor.ActivityName + " does not exist";
            Category parentCategory = parent as Category;
            if (parentCategory == null)
                return "Parent " + parent.Name + " is not of type Category";
            child.AddParent(parentCategory);
            if (this.InheritanceAdded != null)
                this.InheritanceAdded.Invoke(this, inheritance);
            return "";
        }

        // returns the newly created Activity, or null if none was created
        public Activity CreateCategoryIfMissing(ActivityDescriptor descriptor)
        {
            // attempt to find an activity that matches
            Activity activity = this.ResolveDescriptor(descriptor);
            if (activity == null)
            {
                // if this descriptor indicates a new activity, then create it
                return this.CreateCategory(descriptor);
            }
            // no Activity was created, so we don't return one
            return null;
        }

        /*public Activity GetOrCreate(ActivityDescriptor descriptor)
        {
            Activity activity = this.ResolveDescriptor(descriptor);
            if (activity == null)
                activity = this.CreateActivityIfMissing(descriptor);
            return activity;
        }*/
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

        public Category ResolveToCategory(ActivityDescriptor descriptor)
        {
            return (Category)this.ResolveDescriptor(descriptor);
        }

        public ToDo ResolveTodo(ActivityDescriptor descriptor)
        {
            return (ToDo)this.ResolveDescriptor(descriptor);
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
        public IEnumerable<Activity> AllActivities
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
                    Category category = activity as Category;
                    if (category == null || category.Children.Count == 0)
                        results.Add(activity);
                }
                return results;
            }

        }
        #endregion

        // constructs an Activity from the given ActivityDescriptor
        private Activity CreateCategory(ActivityDescriptor sourceDescriptor)
        {
            Category result = new Category(sourceDescriptor.ActivityName, this.ratingSummarizer);
            if (sourceDescriptor.Choosable != null)
                result.setChooseable(sourceDescriptor.Choosable.Value);

            this.AddActivity(result);
            return result;
        }

        private ToDo CreateToDo(ActivityDescriptor sourceDescriptor)
        {
            ToDo result = new ToDo(sourceDescriptor.ActivityName, this.ratingSummarizer);
            this.AddActivity(result);
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

        // puts an Activity in the database
        private void AddActivity(Activity newActivity)
        {
            string activityName = newActivity.Name;
            // make a list containing just this Activity
            List<Activity> activityList = new List<Activity>();
            activityList.Add(newActivity);
            // add it to the database
            this.activitiesByName.Add(newActivity.Name, activityList);
            // add it to the list of all activities
            this.allActivities.Add(newActivity);

            if (this.ActivityAdded != null)
                this.ActivityAdded.Invoke(this, newActivity);
        }

        // confirms that it is valid to create a new activity with the given name and parent
        private string ValidateCandidateNewInheritance(Inheritance inheritance)
        {
            ActivityDescriptor childDescriptor = inheritance.ChildDescriptor;
            if (childDescriptor == null)
                return "Child name is required";
            Activity existingChild = this.ResolveDescriptor(childDescriptor);
            if (existingChild != null)
                return "Child " + existingChild.Name + " already exists";
            ActivityDescriptor parentDescriptor = inheritance.ParentDescriptor;
            if (parentDescriptor == null)
                return "Parent name is required";
            Activity parent = this.ResolveDescriptor(parentDescriptor);
            if (parent == null)
                return "Parent " + parentDescriptor.ActivityName + " does not exist";
            if (!(parent is Activity))
                return "Parent " + parentDescriptor.ActivityName + " is not of type Category";
            return "";
        }


        #endregion

        #region Private Variables

        private List<Activity> allActivities;
        // We only allow one Activity for each name, but we want to be able to find activities having certain name prefixes, and StatList currently
        // requires that its value type is the same as its aggregation type
        private StatList<string, IEnumerable<Activity>> activitiesByName;
        private RatingSummarizer ratingSummarizer;
        private Category rootActivity;

        #endregion
    }
}
