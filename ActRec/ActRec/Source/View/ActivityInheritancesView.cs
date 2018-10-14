using ActivityRecommendation.Effectiveness;
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
                if (children.Count < 1)
                {
                    title = activity.Name;
                }
                else
                {
                    title = activity.Name;
                }
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
            this.TitleBlock.HorizontalTextAlignment = Xamarin.Forms.TextAlignment.Start;


            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();


            ToDo todo = activity as ToDo;
            if (todo != null)
            {
                if (todo.IsCompleted())
                    gridBuilder.AddLayout(new TextblockLayout("Status: Completed"));
                else
                    gridBuilder.AddLayout(new TextblockLayout("Status: Incomplete"));
            }

            foreach (Metric metric in activity.Metrics)
            {
                gridBuilder.AddLayout(new TextblockLayout("Has metric: " + metric.Name));
            }
            if (parents.Count > 0)
                gridBuilder.AddLayout(new ActivityListView(parents.Count.ToString() + " Parents:", parents));
            if (children.Count > 0)
                gridBuilder.AddLayout(new ActivityListView(children.Count.ToString() + " Children:", children));

            this.SetContent(gridBuilder.Build());
        }
    }

    class ActivityListView : TitledControl
    {
        public ActivityListView(string name, List<Activity> activities)
        {
            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();
            foreach (Activity activity in activities)
            {
                gridBuilder.AddLayout(new TextblockLayout(activity.Name, 16));
            }
            LayoutChoice_Set scrollLayout = ScrollLayout.New(gridBuilder.Build());

            this.SetTitle(name);
            this.SetContent(scrollLayout);
        }
    }
}
