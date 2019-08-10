using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActivityRecommendation.Effectiveness;
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
        public delegate void ActivityAddedHandler(Activity activity);

        public event InheritanceAddedHandler InheritanceAdded;
        public delegate void InheritanceAddedHandler(Inheritance inheritance);

        public event MetricAddedHandler MetricAdded;
        public delegate void MetricAddedHandler(Metric metric, Activity activity);

        #region Constructor

        public ActivityDatabase(ScoreSummarizer happinessSummarizer, ScoreSummarizer efficiencySummarizer)
        {
            //this.activitiesByName = new Dictionary<string, List<Activity> >();
            this.activitiesByName = new StatList<string, IEnumerable<Activity>>(this, this);
            this.allActivities = new List<Activity>();
            this.happinessSummarizer = happinessSummarizer;
            this.efficiencySummarizer = efficiencySummarizer;
            this.rootActivity = new Category("Activity", happinessSummarizer, efficiencySummarizer);
            this.rootActivity.setSuggestible(false);
            this.AddActivity(this.rootActivity);
            this.todoCategory = new Category("ToDo", happinessSummarizer, efficiencySummarizer);
            this.todoCategory.setSuggestible(false);
            this.AddActivity(this.todoCategory);
        }

        #endregion

        #region Public Member Functions

        // returns a string other than "" in case of error
        public string CreateCategory(Inheritance inheritance)
        {
            string err = this.ValidateCandidateNewInheritance(inheritance);
            if (err != "")
                return err;
            Activity parent = this.ResolveDescriptor(inheritance.ParentDescriptor);
            if (parent == this.todoCategory)
                return "You can't assign an Activity of type Category as a child of the Category named ToDo";
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
            Activity parent = this.ResolveDescriptor(inheritance.ParentDescriptor);
            if (parent == this.todoCategory)
                return ""; // don't have to add the same parent twice
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
                this.InheritanceAdded.Invoke(inheritance);
            return "";
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
            if (descriptor == null)
                return null;
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

        public bool HasActivity(ActivityDescriptor descriptor)
        {
            return (this.ResolveDescriptor(descriptor) != null);
        }

        public Category ResolveToCategory(ActivityDescriptor descriptor)
        {
            return (Category)this.ResolveDescriptor(descriptor);
        }

        public Activity GetActivityOrCreateCategory(ActivityDescriptor descriptor)
        {
            Activity existing = this.ResolveDescriptor(descriptor);
            if (existing == null)
            {
                return this.CreateCategory(descriptor);
            }
            return existing;
        }

        public ToDo ResolveTodo(ActivityDescriptor descriptor)
        {
            return (ToDo)this.ResolveDescriptor(descriptor);
        }

        public ToDo GetOrCreateTodo(ActivityDescriptor descriptor)
        {
            ToDo existing = this.ResolveTodo(descriptor);
            if (existing == null)
            {
                return this.CreateToDo(descriptor);
            }
            return existing;
        }

        // tells whether the given descriptor can match the given activity
        public bool Matches(ActivityDescriptor descriptor, Activity activity)
        {
            if (this.MatchQuality(descriptor, activity) > 0)
                return true;
            else
                return false;

        }
        // If there is a discrepancy, returns 0
        // Otherwise, returns a number that gets larger if it's more likely that the user meant to match <activity>
        public double MatchQuality(ActivityDescriptor descriptor, Activity activity)
        {
            double matchScore = 0;
            if (descriptor.RequiresPerfectMatch)
            {
                // make sure the name matches
                if (descriptor.ActivityName != null && !descriptor.ActivityName.Equals(activity.Name))
                    return 0;
                // make sure the 'Choosable' property matches
                // require that the 'Choosable' property matches
                if (descriptor.Suggestible != null && descriptor.Suggestible.Value != activity.Suggestible)
                    return 0;
                return 1;
            }
            else
            {
                // a bunch of points based on string similarity
                string desiredName = descriptor.ActivityName;
                if (desiredName.Length < 2 * activity.Name.Length)
                {
                    matchScore = this.stringScore(activity.Name, desiredName);
                }
                else
                {
                    // if the user enters a string that it rediculously long, then we don't bother comparing it
                    matchScore = 0;
                }
                if (desiredName.Length > 0 && matchScore <= 0)
                {
                    // name has nothing in common
                    return 0;
                }
            }
            // now that we've verified that this activity is allowed to be a match, we calculate its score

            // +1 point if the 'Suggestible' property matches
            if (descriptor.Suggestible != null && descriptor.Suggestible.Value == activity.Suggestible)
                matchScore += 1;


            // up to 1 extra point based on the likelihood that the user did this activity
            if (descriptor.PreferMorePopular)
            {
                // Give better scores to activities that the user has logged more often
                matchScore += (1.0 - 1.0 / ((double)activity.NumParticipations + 1.0));
            }
            else
            {
                matchScore += 1;
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
        public IEnumerable<ToDo> AllOpenTodos
        {
            get
            {
                List<ToDo> results = new List<ToDo>();
                foreach (ToDo todo in this.AllTodos)
                {
                    if (!todo.IsCompleted())
                    {
                        results.Add(todo);
                    }
                }
                return results;
            }

        }
        public IEnumerable<ToDo> AllTodos
        {
            get
            {
                List<ToDo> toDos = new List<ToDo>(this.todoCategory.Children.Count);
                foreach (Activity child in this.todoCategory.Children)
                {
                    ToDo todo = child as ToDo;
                    if (todo == null)
                    {
                        throw new InvalidCastException("Internal error: Activity " + child + " was assigned as a child of " + this.todoCategory + " but is not of type ToDo");
                    }
                    toDos.Add(todo);
                }
                return toDos;
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
        public Category CreateCategory(ActivityDescriptor sourceDescriptor)
        {
            Activity existing = this.ResolveDescriptor(sourceDescriptor);
            if (existing != null)
            {
                throw new ArgumentException("Activity " + sourceDescriptor.ActivityName + " already exists");
            }
            Category result = new Category(sourceDescriptor.ActivityName, this.happinessSummarizer, this.efficiencySummarizer);
            this.AddActivity(result);
            return result;
        }

        public ToDo CreateToDo(ActivityDescriptor sourceDescriptor)
        {
            Activity existing = this.ResolveDescriptor(sourceDescriptor);
            if (existing != null)
            {
                throw new ArgumentException("Activity " + sourceDescriptor.ActivityName + " already exists");
            }
            ToDo result = new ToDo(sourceDescriptor.ActivityName, this.happinessSummarizer, this.efficiencySummarizer);
            result.AddParent(this.todoCategory);
            this.AddActivity(result);
            return result;
        }

        // returns a string telling the error, or "" if no error
        public string AddMetric(Activity activity, Metric metric)
        {
            if (activity.Metrics.Count > 0)
            {
                // TODO: remove this requirement when the ParticipationEntryView can support more than one metric per Activity
                return activity.Name + " already has a metric (" + activity.Metrics[0].Name + ")";
            }
            activity.AddMetric(metric);
            this.MetricAdded.Invoke(metric, activity);
            return "";
        }

        public void AssignDefaultParent()
        {
            foreach (Activity activity in this.allActivities)
            {
                if (activity.Parents.Count < 1 && activity != this.rootActivity)
                    activity.Parents.Add(this.rootActivity);
            }
        }

        public void AddInheritance(Inheritance inheritance)
        {
            this.GetActivityOrCreateCategory(inheritance.ParentDescriptor);
            Category parent = this.ResolveToCategory(inheritance.ParentDescriptor);
            Activity child = this.GetActivityOrCreateCategory(inheritance.ChildDescriptor);
            if (inheritance.DiscoveryDate != null)
                child.ApplyInheritanceDate((DateTime)inheritance.DiscoveryDate);
            child.AddParent(parent);
            if (this.InheritanceAdded != null)
                this.InheritanceAdded.Invoke(inheritance);
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
        private int stringScore(string item, string query)
        {
            return this.stringQueryMatcher.StringScore(item, query);
        }
        // puts an Activity in the database
        private void AddActivity(Activity newActivity)
        {
            System.Diagnostics.Debug.WriteLine("Adding " + newActivity);
            if (newActivity.Name == "Adding  binary trees into Bluejay")
            {
                System.Diagnostics.Debug.WriteLine("That's weird");
            }
            string activityName = newActivity.Name;
            // make a list containing just this Activity
            List<Activity> activityList = new List<Activity>();
            activityList.Add(newActivity);
            // add it to the database
            this.activitiesByName.Add(newActivity.Name, activityList);
            // add it to the list of all activities
            this.allActivities.Add(newActivity);

            if (this.ActivityAdded != null)
                this.ActivityAdded.Invoke(newActivity);
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
        private ScoreSummarizer happinessSummarizer;
        private ScoreSummarizer efficiencySummarizer;
        private Category rootActivity;
        private Category todoCategory; // a Category that is the parent of each ToDo
        private StringQueryMatcher stringQueryMatcher = new StringQueryMatcher();

        #endregion
    }
}
