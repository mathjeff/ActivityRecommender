using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ActivityInheritancesView : TitledControl
    {
        public event RequestDeletion_Handler RequestDeletion;
        public delegate void RequestDeletion_Handler(Activity activity);
        public ActivityInheritancesView(Activity activity, ActivityDatabase activityDatabase, bool allowDeletion = false)
        {
            this.activityDatabase = activityDatabase;
            this.supportDeletion = allowDeletion;
            this.setup(activity);
        }

        public void setup(Activity activity)
        {
            this.activity = activity;
            List<Activity> children = activity.GetChildren();
            List<Activity> parents = activity.Parents;
            string title;
            if (activity == this.activityDatabase.RootActivity)
            {
                title = activity.Name + " is the built-in root activity.";
            }
            else
            {
                title = activity.Name;
                if (activity is Category)
                {
                    if (activity.IsSolution)
                        title += " (Category, Solution)";
                    else
                        title += " (Category)";
                }
                else if (activity is ToDo)
                {
                    title += " (ToDo)";
                }
                else if (activity is Problem)
                {
                    title += " (Problem)";
                }
            }
            this.SetTitle(title);
            this.TitleLayout.AlignHorizontally(Xamarin.Forms.TextAlignment.Start);


            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();


            ToDo todo = activity as ToDo;
            if (todo != null)
            {
                if (todo.IsCompleted())
                {
                    if (todo.WasCompletedSuccessfully())
                        gridBuilder.AddLayout(new TextblockLayout("Status: Completed"));
                    else
                        gridBuilder.AddLayout(new TextblockLayout("Status: Obsolete"));
                }
                else
                {
                    gridBuilder.AddLayout(new TextblockLayout("Status: Incomplete"));
                }
            }

            foreach (Metric metric in activity.IntrinsicMetrics)
            {
                gridBuilder.AddLayout(new TextblockLayout("Has metric: " + metric.Name));
            }
            foreach (Metric metric in activity.InheritedMetrics)
            {
                gridBuilder.AddLayout(new TextblockLayout("Inherited metric: " + metric.Name));
            }
            if (parents.Count > 0)
                gridBuilder.AddLayout(this.newListView(parents.Count.ToString() + " Parents:", parents));

            List<Activity> openChildTodos = new List<Activity>();
            List<Activity> completedChildTodos = new List<Activity>();
            List<Activity> childCategories = new List<Activity>();
            List<Activity> childProblems = new List<Activity>();
            foreach (Activity child in children)
            {
                ToDo childToDo = child as ToDo;
                if (childToDo != null)
                {
                    if (childToDo.IsCompleted())
                        completedChildTodos.Add(childToDo);
                    else
                        openChildTodos.Add(childToDo);
                }
                else if (child is Category)
                {
                    childCategories.Add(child);
                }
                else if (child is Problem)
                {
                    childProblems.Add(child);
                }
                else
                {
                    throw new InvalidCastException("Unrecognized object type " + child);
                }
            }

            if (children.Count > 0)
            {
                List<Activity> sortedToDos = new List<Activity>();
                sortedToDos.AddRange(openChildTodos);
                sortedToDos.AddRange(completedChildTodos);

                List<Activity> allOpenTodos = new List<Activity>();
                foreach (Activity otherToDo in activity.OtherOpenTodos)
                    allOpenTodos.Add(otherToDo);

                if (childCategories.Count > 0)
                    gridBuilder.AddLayout(this.newListView(" " + childCategories.Count.ToString() + " Children of type Category:", childCategories));
                if (sortedToDos.Count > 0)
                    gridBuilder.AddLayout(this.newListView(" " + sortedToDos.Count.ToString() + " Children of type ToDo:", sortedToDos));
                if (childProblems.Count > 0)
                    gridBuilder.AddLayout(this.newListView(" " + childProblems.Count.ToString() + " Children of type Problem:", childProblems));
                if (allOpenTodos.Count > openChildTodos.Count)
                    gridBuilder.AddLayout(this.newListView(" " + allOpenTodos.Count.ToString() + " Total open ToDos:", allOpenTodos));
            }

            gridBuilder.AddLayout(new TextblockLayout("You've done this " + activity.NumParticipations + " times since " + activity.DiscoveryDate.ToString("yyyy-MM-dd")));

            // If this activity has never been used, then it should be safe to delete it
            if (this.supportDeletion)
            {
                if (activity.GetChildren().Count < 1 && activity.NumParticipations < 1 && activity.NumConsiderations < 1 && !activity.HasAMetric)
                {
                    Button deleteButton = new Button();
                    deleteButton.Clicked += DeleteButton_Clicked;
                    gridBuilder.AddLayout(new ButtonLayout(deleteButton, "Delete"));
                }
            }

            this.SetContent(gridBuilder.Build());
        }

        private void DeleteButton_Clicked(object sender, EventArgs e)
        {
            this.RequestDeletion.Invoke(this.activity);
        }

        private LayoutChoice_Set newListView(string title, List<Activity> activities)
        {
            ActivityListView listView = new ActivityListView(title, activities);
            listView.SelectedActivity += ListView_SelectedActivity;

            Shrunken_ActivityListView shrunkenListView = new Shrunken_ActivityListView(title, activities);
            shrunkenListView.SelectedActivities += ShrunkenListView_SelectedActivities;

            return new LayoutUnion(shrunkenListView, listView);
        }

        private void ShrunkenListView_SelectedActivities(string name, List<Activity> activities)
        {
            ActivityListView fullListView = new ActivityListView(name, activities);
            fullListView.SelectedActivity += ListView_SelectedActivity;
            this.SetContent(ScrollLayout.New(fullListView));
        }

        private void ListView_SelectedActivity(Activity activity)
        {
            this.setup(activity);
        }

        ActivityDatabase activityDatabase;
        Activity activity;
        bool supportDeletion = false;
    }

    class ActivityListView : TitledControl
    {
        public event SelectedActivityHandler SelectedActivity;
        public delegate void SelectedActivityHandler(Activity activity);

        public ActivityListView(string name, List<Activity> activities)
        {
            double fontSize = 16;

            // inline layout
            Vertical_GridLayout_Builder inlineBuilder = new Vertical_GridLayout_Builder().Uniform();
            foreach (Activity activity in activities)
            {
                Button otherActivityButton = new Button();
                otherActivityButton.Clicked += OtherActivityButton_Clicked;
                this.activitiesByButton[otherActivityButton] = activity;
                inlineBuilder.AddLayout(new ButtonLayout(otherActivityButton, activity.Name, fontSize));
            }
            this.SetContent(inlineBuilder.BuildAnyLayout());

            this.SetTitle(name);
        }

        private void OtherActivityButton_Clicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            Activity activity = this.activitiesByButton[button];
            this.SelectedActivity.Invoke(activity);
        }
        Dictionary<Button, Activity> activitiesByButton = new Dictionary<Button, Activity>();
    }
    class Shrunken_ActivityListView : TitledControl
    {
        public event SelectedActivitiesHandler SelectedActivities;
        public delegate void SelectedActivitiesHandler(string name, List<Activity> activities);

        public Shrunken_ActivityListView(string name, List<Activity> activities)
        {
            this.activities = activities;
            this.name = name;
            double fontSize = 16;

            // expand layout
            Button expandButton = new Button();
            expandButton.Clicked += ExpandButton_Clicked;
            this.SetContent(new ButtonLayout(expandButton, "See List", fontSize));
            this.SetTitle(name);
        }

        private void ExpandButton_Clicked(object sender, EventArgs e)
        {
            if (this.SelectedActivities != null)
                this.SelectedActivities.Invoke(this.name, this.activities);
        }

        string name;
        List<Activity> activities;
    }
}
