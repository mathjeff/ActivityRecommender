﻿using ActivityRecommendation.Effectiveness;
using StatLists;
using System;
using System.Collections.Generic;
using System.Linq;

// An ActivityDatabase class stores all of the known Activities, for the purpose of resolving a name into an Activity
namespace ActivityRecommendation
{

    public interface ReadableActivityDatabase
    {
        Activity ResolveDescriptor(ActivityDescriptor activityDescriptor);
        IEnumerable<Activity> AllActivities { get; }
        Boolean ContainsCustomActivity();
        Activity GetRootActivity();
        List<Activity> FindBestMatches(ActivityDescriptor activityDescriptor, int count);
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
            this.activitiesByName = new StatList<string, IEnumerable<Activity>>(this, this);
            this.allActivities = new List<Activity>();
            this.happinessSummarizer = happinessSummarizer;
            this.efficiencySummarizer = efficiencySummarizer;
            this.rootActivity = new Category("Activity", happinessSummarizer, efficiencySummarizer);
            this.rootActivity.setSuggestible(false);
            this.AddActivity(this.rootActivity);
            this.todoCategory = new Category("ToDo", happinessSummarizer, efficiencySummarizer);
            this.todoCategory.setSuggestible(false);
            this.todoCategory.AddParent(this.rootActivity);
            this.AddActivity(this.todoCategory);
            this.problemCategory = new Category("Problem", happinessSummarizer, efficiencySummarizer);
            this.problemCategory.setSuggestible(false);
            this.problemCategory.AddParent(this.rootActivity);
            this.AddActivity(this.problemCategory);
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
            if (parent == this.todoCategory || parent == this.problemCategory)
                return "You can't assign an Activity of type Category as a child of the Category named " + parent.Name;
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
            if (parent == this.todoCategory || parent == this.rootActivity)
                return ""; // don't have to add the same parent twice
            return this.AddParent(inheritance);
        }

        public string CreateProblem(Inheritance inheritance)
        {
            string err = this.ValidateCandidateNewInheritance(inheritance);
            if (err != "")
                return err;
            Problem problem = this.CreateProblem(inheritance.ChildDescriptor);
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
                return "Parent name is required: try \"Activity\"";
            Activity parent = this.ResolveDescriptor(inheritance.ParentDescriptor);
            if (parent == null)
                return "Parent " + inheritance.ParentDescriptor.ActivityName + " does not exist";
            Category parentCategory = parent as Category;
            if (parentCategory != null)
            {
                if (child is Problem && child.Parents.Count == 0 && parentCategory != this.problemCategory)
                {
                    // If the first parent added to a Problem is neither a Problem nor this.problemCategory, then add this.problemCategory as a parent
                    child.AddParent(this.problemCategory);
                }
                child.AddParent(parentCategory);
            }
            else
            {
                Problem parentProblem = parent as Problem;
                if (parentProblem != null)
                {
                    child.AddParent(parentProblem);
                    if (!(child is Problem))
                        this.hasSolution = true;
                }
                else
                    return "Parent " + parent.Name + " is not of type Category or Problem";
            }
            if (this.InheritanceAdded != null)
                this.InheritanceAdded.Invoke(inheritance);
            return "";
        }

        // finds the Activity indicated by the ActivityDescriptor
        public Activity ResolveDescriptor(ActivityDescriptor descriptor)
        {
            IEnumerable<Activity> candidates = this.GetCandidateMatches(descriptor);
            Activity result = null;
            double bestMatchScore = 0;
            // figure out which activity matches best
            foreach (Activity activity in candidates)
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
        // returns a list containing this Activity and all of its ancestors
        public List<Activity> GetAllSuperactivitiesOf(Activity activity)
        {
            return activity.SelfAndAncestors;
        }
        // tells whether <ancestor> is either <descendant> or one of the ancestors of <descendant>
        public bool HasAncestor(Activity descendant, Activity ancestor)
        {
            return descendant.HasAncestor(ancestor);
        }

        // Find the top few matching activities
        public List<Activity> FindBestMatches(ActivityDescriptor descriptor, int count)
        {
            if (count < 1)
                return new List<Activity>(0);
            if (count == 1)
            {
                Activity best = this.ResolveDescriptor(descriptor);
                if (best != null)
                    return new List<Activity>() { best };
                return new List<Activity>() { };
            }
            IEnumerable<Activity> activities = this.GetCandidateMatches(descriptor);
            StatList<double, Activity> sortedItems = new StatList<double, Activity>(new ReverseDoubleComparer(), new NoopCombiner<Activity>());
            foreach (Activity activity in activities)
            {
                double quality = this.MatchQuality(descriptor, activity);
                if (quality > 0)
                    sortedItems.Add(quality, activity);
            }
            count = Math.Min(count, sortedItems.NumItems);
            List<Activity> top = new List<Activity>(count);
            for (int i = 0; i < count; i++)
            {
                top.Add(sortedItems.GetValueAtIndex(i).Value);
            }
            return top;
        }

        // Returns a list of Activity that might be considered to match <descriptor>
        private IEnumerable<Activity> GetCandidateMatches(ActivityDescriptor descriptor)
        {
            if (descriptor == null)
                return new List<Activity>(0);
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
            return activities;
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

        public Problem ResolveProblem(ActivityDescriptor descriptor)
        {
            return (Problem)this.ResolveDescriptor(descriptor);
        }

        public Problem GetOrCreateProblem(ActivityDescriptor descriptor)
        {
            Problem existing = this.ResolveProblem(descriptor);
            if (existing == null)
                return this.CreateProblem(descriptor);
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
            if (descriptor.RequiresPerfectMatch)
            {
                // make sure the name matches
                if (descriptor.ActivityName != null && !descriptor.ActivityName.Equals(activity.Name))
                    return 0;
                return 1;
            }

            int stringScore = 0;
            // points based on string similarity
            string desiredName = descriptor.ActivityName;
            if (desiredName.Length < 2 * activity.Name.Length)
            {
                stringScore = this.stringScore(activity.Name, desiredName);
            }
            else
            {
                // if the user enters a string that it ridiculously long, then we don't bother comparing it
                stringScore = 0;
            }
            if (desiredName.Length > 0 && stringScore <= 0)
            {
                // name has nothing in common
                return 0;
            }
            // now that we've verified that this activity is allowed to be a match, we calculate its score

            // extra points for a fully matching name, because it must be possible to select an activity by name directly
            double exactNameScore = 0;
            if (descriptor.ActivityName.ToLower().Equals(activity.Name.ToLower()))
                exactNameScore += 1;
            if (descriptor.ActivityName.Equals(activity.Name))
                exactNameScore += 1;

            // more points if the 'Suggestible' property matches
            double suggestibleScore = 0;
            if (descriptor.Suggestible != null)
            {
                if (descriptor.Suggestible.Value)
                {
                    // If we're looking for a suggestible activity, then we give more points to activities that are willing to be suggested
                    if (activity.Suggestible)
                        suggestibleScore += 1;
                    // If we're looking for a suggestible activity, then we give more points to activities that have no children
                    // (activities having child categories are generally less interesting than their children)
                    if (!activity.HasChildCategory)
                        suggestibleScore++;
                }
                else
                {
                    // If we're looking for a non-suggestible activity, then we give more points to an activity that's not willing to be suggested
                    if (!activity.Suggestible)
                        suggestibleScore += 1;
                }
            }


            double participationScore = 0;
            // more points based on the likelihood that the user did this activity
            if (descriptor.PreferMorePopular)
            {
                // Give better scores to activities that the user has logged more often
                participationScore += (1.0 - 1.0 / ((double)activity.NumParticipations + 1.0));
            }

            // fewer points for completed Todos
            double completedTodoScore = 1;
            ToDo t = activity as ToDo;
            if (t != null)
            {
                if (t.IsCompleted())
                {
                    completedTodoScore = 0;
                }
            }

            double isACompletedToDoScore = 1;
            if (descriptor.PreferAvoidCompletedToDos)
            {
                ToDo toDo = activity as ToDo;
                if (toDo != null)
                {
                    if (toDo.IsCompleted())
                        isACompletedToDoScore = 0;
                }
            }

            // Now we add up the score
            // We give lower priority to the factors at the top of this calculation and higher priority to the lower factors
            
            double finalScore = 0;
            // points for non-name properties
            finalScore += participationScore;
            finalScore += suggestibleScore;
            finalScore += completedTodoScore;
            finalScore += isACompletedToDoScore;
            finalScore /= 4;
            // more points for a more closely matching name
            finalScore += stringScore;

            return finalScore;
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
        public IEnumerable<Category> AllCategories
        {
            get
            {
                List<Category> results = new List<Category>();
                foreach (Activity activity in this.AllActivities)
                {
                    Category category = activity as Category;
                    if (category != null)
                        results.Add(category);
                }
                return results;
            }
        }
        public IEnumerable<ToDo> AllOpenTodos
        {
            get
            {
                return this.rootActivity.OtherOpenTodos;
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

        public bool HasTodo
        {
            get
            {
                return this.todoCategory.Children.Count > 0;
            }
        }

        public IEnumerable<Problem> AllProblems
        {
            get
            {
                List<Problem> problems = new List<Problem>();
                List<Activity> candidates = this.problemCategory.GetChildrenRecursive();
                foreach (Activity activity in candidates)
                {
                    // Determine whether this particular child is another Problem or is a solution
                    Problem p = activity as Problem;
                    problems.Add(p);
                }
                return problems;
            }
        }
        public bool HasProblem
        {
            get
            {
                return this.hasProblem;
            }
        }
        public bool HasSolution
        {
            get
            {
                return this.hasSolution;
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
        public Activity GetRootActivity()
        {
            return this.RootActivity;
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

        public Problem CreateProblem(ActivityDescriptor sourceDescriptor)
        {
            Activity existing = this.ResolveDescriptor(sourceDescriptor);
            if (existing != null)
            {
                throw new ArgumentException("Activity " + sourceDescriptor.ActivityName + " already exists");
            }
            Problem result = new Problem(sourceDescriptor.ActivityName, this.happinessSummarizer, this.efficiencySummarizer);
            this.AddActivity(result);
            this.hasProblem = true;
            return result;
        }

        // returns a string telling the error, or "" if no error
        public string AddMetric(Activity activity, Metric metric)
        {
            if (activity.MetricForName(metric.Name) != null)
            {
                return "Activity " + activity.Name + " already has metric " + metric.Name;
            }
            activity.AddIntrinsicMetric(metric);
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
            this.GetActivityOrCreateCategory(inheritance.ChildDescriptor);
            this.GetActivityOrCreateCategory(inheritance.ParentDescriptor);
            string message = this.AddParent(inheritance);
            if (message != "")
                throw new ArgumentException(message);
        }

        public bool ContainsCustomActivity()
        {
            return this.NumCustomActivities > 0;
        }

        public int NumCustomActivities
        {
            get
            {
                return this.NumActivities - 3;
            }
        }

        public bool RequestedActivityFromCategory { get; set; }
        public bool RequestedActivityAtLeastAsGoodAsOther { get; set; }


        #region Functions for ICombiner<List<Activity>>
        public IEnumerable<Activity> Combine(IEnumerable<Activity> list1, IEnumerable<Activity> list2)
        {
            return list1.Concat(list2);
        }
        public IEnumerable<Activity> Default()
        {
            return new List<Activity>(0);
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
            string activityName = newActivity.Name;
            // make a list containing just this Activity
            List<Activity> activityList = new List<Activity>(1);
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
                return "Parent name is required: try \"Activity\"";
            Activity parent = this.ResolveDescriptor(parentDescriptor);
            if (parent == null)
                return "Parent " + parentDescriptor.ActivityName + " does not exist";
            if (parent is ToDo)
                return "Parent " + parentDescriptor.ActivityName + " is a ToDo which cannot have child activities";
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
        private Category problemCategory; // a Category that is the parent of each problem
        private StringQueryMatcher stringQueryMatcher = new StringQueryMatcher();
        private bool hasProblem;
        private bool hasSolution;

        #endregion
    }
}
