using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ActivityInheritancesView : TitledControl
    {
        public ActivityInheritancesView(Activity activity, ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
            this.setup(activity);
        }

        public void setup(Activity activity)
        {
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

            List<Activity> childTodos = new List<Activity>();
            List<Activity> childCategories = new List<Activity>();
            List<Activity> childProblems = new List<Activity>();
            foreach (Activity child in children)
            {
                if (child is ToDo)
                {
                    childTodos.Add(child);
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
                if (childCategories.Count > 0)
                    gridBuilder.AddLayout(this.newListView(" " + childCategories.Count.ToString() + " Children of type Category:", childCategories));
                if (childTodos.Count > 0)
                    gridBuilder.AddLayout(this.newListView(" " + childTodos.Count.ToString() + " Children of type ToDo:", childTodos));
                if (childProblems.Count > 0)
                    gridBuilder.AddLayout(this.newListView(" " + childProblems.Count.ToString() + " Children of type Problem:", childProblems));
            }

            gridBuilder.AddLayout(new TextblockLayout("You've done this " + activity.NumParticipations + " times since " + activity.DiscoveryDate.ToString("yyyy-MM-dd")));

            this.SetContent(gridBuilder.Build());
        }

        private ActivityListView newListView(string title, List<Activity> activities)
        {
            ActivityListView listView = new ActivityListView(title, activities);
            listView.SelectedActivity += ListView_SelectedActivity;
            return listView;
        }

        private void ListView_SelectedActivity(Activity activity)
        {
            this.setup(activity);
        }

        ActivityDatabase activityDatabase;
    }

    class ActivityListView : TitledControl
    {
        public event SelectedActivityHandler SelectedActivity;
        public delegate void SelectedActivityHandler(Activity activity);

        public ActivityListView(string name, List<Activity> activities)
        {
            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
            foreach (Activity activity in activities)
            {
                Button otherActivityButton = new Button();
                otherActivityButton.Clicked += OtherActivityButton_Clicked;
                this.activitiesByButton[otherActivityButton] = activity;
                gridBuilder.AddLayout(new ButtonLayout(otherActivityButton, activity.Name, 16));
            }
            LayoutChoice_Set scrollLayout = ScrollLayout.New(gridBuilder.BuildAnyLayout());
            this.SetContent(scrollLayout);

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
}
