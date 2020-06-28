using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class ActivityInheritancesView : TitledControl
    {
        public ActivityInheritancesView(Activity activity, ActivityDatabase activityDatabase)
        {
            List<Activity> children = activity.GetChildren();
            List<Activity> parents = activity.Parents;
            string title;
            if (activity == activityDatabase.RootActivity)
            {
                title = activity.Name + " is the built-in root activity.";
            }
            else
            {
                title = activity.Name;
                if (activity is Category)
                {
                    title += " (Category)";
                }
                else
                {
                    if (activity is ToDo)
                    {
                        title += " (ToDo)";
                    }
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

            foreach (Metric metric in activity.Metrics)
            {
                gridBuilder.AddLayout(new TextblockLayout("Has metric: " + metric.Name));
            }
            if (parents.Count > 0)
                gridBuilder.AddLayout(new ActivityListView(parents.Count.ToString() + " Parents:", parents));

            List<Activity> childTodos = new List<Activity>();
            List<Activity> childCategories = new List<Activity>();
            foreach (Activity child in children)
            {
                if (child is ToDo)
                {
                    childTodos.Add(child);
                }
                else
                {
                    if (child is Category)
                    {
                        childCategories.Add(child);
                    }
                    else
                    {
                        throw new InvalidCastException("Unrecognized object type " + child);
                    }
                }
            }

            if (children.Count > 0)
            {
                gridBuilder.AddLayout(new ActivityListView(" " + childCategories.Count.ToString() + " Children of type Category:", childCategories));
                gridBuilder.AddLayout(new ActivityListView(" " + childTodos.Count.ToString() + " Children of type ToDo:", childTodos));
            }

            this.SetContent(gridBuilder.Build());
        }
    }

    class ActivityListView : TitledControl
    {
        public ActivityListView(string name, List<Activity> activities)
        {
            if (activities.Count > 1)
            {
                Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();
                foreach (Activity activity in activities)
                {
                    gridBuilder.AddLayout(new TextblockLayout(activity.Name, 16));
                }
                LayoutChoice_Set scrollLayout = ScrollLayout.New(gridBuilder.Build());
                this.SetContent(scrollLayout);
            }
            else
            {
                if (activities.Count > 0)
                    this.SetContent(new TextblockLayout(activities[0].Name, 16));
            }

            this.SetTitle(name);
        }
    }
}
